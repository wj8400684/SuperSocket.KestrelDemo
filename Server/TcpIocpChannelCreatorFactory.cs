using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;
using SuperSocket.Server;
using SuperSocket;
using SuperSocket.ProtoBase;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.Kestrel.Channel;

namespace Server;

internal sealed class TcpIocpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;

    public TcpIocpChannelCreatorFactory(IServiceProvider serviceProvider)
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