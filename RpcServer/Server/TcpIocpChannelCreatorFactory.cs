using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;
using SuperSocket.Server;
using SuperSocket;
using SuperSocket.ProtoBase;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.Kestrel.Channel;
using System.IO.Pipelines;
using System.Buffers;
using SuperSocket.Kestrel.Application;

namespace Server;

internal sealed class TcpIocpChannelWithKestrelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;

    public TcpIocpChannelWithKestrelCreatorFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<SocketConnectionContextFactory>>();
        var socketConnectionFactoryOptions = serviceProvider.GetRequiredService<IOptions<SocketConnectionFactoryOptions>>().Value;

        _socketConnectionContextFactory = new SocketConnectionContextFactory(socketConnectionFactoryOptions, logger);
    }

    public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions,
    ILoggerFactory loggerFactory, object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

        var logger = loggerFactory.CreateLogger<TcpChannelCreator>();

        return new TcpChannelCreator(options, (Socket socket) =>
        {
            ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

            var context = _socketConnectionContextFactory.Create(socket);

            var pipelineFilter = filterFactory.Create(context);

            var channel = new KestrelPipeChannel<TPackageInfo>(context, pipelineFilter, channelOptions);

            return new ValueTask<IChannel>(channel);

        }, logger);
    }
}

internal sealed class TcpIocpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    private int _settingsIndex;
    private readonly int _settingsCount;
    private readonly QueueSettings[] _settings;
    private readonly SocketTransportOptions _socketTransportOptions;

    public TcpIocpChannelCreatorFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _socketTransportOptions = serviceProvider.GetRequiredService<IOptions<SocketTransportOptions>>().Value;

        _settingsCount = _socketTransportOptions.IOQueueCount;

        var maxReadBufferSize = _socketTransportOptions.MaxReadBufferSize ?? 0;
        var maxWriteBufferSize = _socketTransportOptions.MaxWriteBufferSize ?? 0;
        var applicationScheduler = _socketTransportOptions.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;

        // Socket callbacks run on the threads polling for IO if we're using the old Windows thread pool
        var dispatchSocketCallbacks = OperatingSystem.IsWindows() &&
                                      (Environment.GetEnvironmentVariable("DOTNET_ThreadPool_UsePortableThreadPoolForIO") == "0" ||
                                      Environment.GetEnvironmentVariable("COMPlus_ThreadPool_UsePortableThreadPoolForIO") == "0");

        PipeScheduler SelectSocketsScheduler(PipeScheduler dispatchingScheduler) =>
    dispatchSocketCallbacks ? dispatchingScheduler : PipeScheduler.Inline;

        if (_settingsCount > 0)
        {
            _settings = new QueueSettings[_settingsCount];

            for (var i = 0; i < _settingsCount; i++)
            {
                var memoryPool = new PinnedBlockMemoryPool();
                var transportScheduler = _socketTransportOptions.UnsafePreferInlineScheduling ? PipeScheduler.Inline : new IOQueue();
                var socketsScheduler = SelectSocketsScheduler(transportScheduler);

                _settings[i] = new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(socketsScheduler),
                    MemoryPool = memoryPool,
                };
            }
        }
        else
        {
            var memoryPool = new PinnedBlockMemoryPool();
            var transportScheduler = _socketTransportOptions.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;
            var socketsScheduler = SelectSocketsScheduler(transportScheduler);

            _settings = new QueueSettings[]
            {
                new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(socketsScheduler),
                    MemoryPool = memoryPool,
                }
            };
            _settingsCount = 1;
        }
    }

    public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options,
                                                                  ChannelOptions channelOptions,
                                                                  ILoggerFactory loggerFactory,
                                                                  object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

        var logger = loggerFactory.CreateLogger<TcpChannelCreator>();

        return new TcpChannelCreator(options, (Socket socket) =>
        {
            QueueSettings setting = _settings[Interlocked.Increment(ref _settingsIndex) % (long)_settingsCount];

            var newChannelOptions = new ChannelOptions
            {
                SendBufferSize = channelOptions.SendBufferSize,
                SendTimeout = channelOptions.SendTimeout,
                ReceiveBufferSize = channelOptions.ReceiveBufferSize,
                Logger = channelOptions.Logger,
                MaxPackageLength = channelOptions.MaxPackageLength,
                ReadAsDemand = channelOptions.ReadAsDemand,
                ReceiveTimeout = channelOptions.ReceiveTimeout,
                Values = channelOptions.Values,
                In = new Pipe(setting.InputOptions),
                Out = new Pipe(setting.OutputOptions),
            };

            ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

            var pipelineFilter = filterFactory.Create(socket);

            var channel = new KestrelIOCPChannel<TPackageInfo>(socket: socket,
                                                               pipelineFilter: pipelineFilter,
                                                               options: newChannelOptions,
                                                               socketSenderPool: setting.SocketSenderPool,
                                                               socketScheduler: setting.SocketSenderPool.Scheduler,
                                                               waitForData: _socketTransportOptions.WaitForDataBeforeAllocatingBuffer);

            return ValueTask.FromResult<IChannel>(channel);

        }, logger);
    }
}