using Microsoft.AspNetCore.Connections;
using SuperSocket;
using SuperSocket.Kestrel;
using SuperSocket.ProtoBase;

var builder = WebApplication.CreateBuilder(args);

var serverOptions = builder.Configuration.GetSection("ServerOptions").Get<ServerOptions>()!;

builder.WebHost.ConfigureKestrel((context, options) =>
{
    foreach (var listeners in serverOptions.Listeners)
    {
        options.ListenNamedPipe(string.Join(":", listeners.Ip, listeners.Port),
            listenOptions => listenOptions.UseConnectionHandler<KestrelChannelCreator>());
        // options.Listen(listeners.GetListenEndPoint(),
        //     listenOptions => { listenOptions.UseConnectionHandler<KestrelChannelCreator>(); });
    }
});

builder.Host.AsSuperSocketHostBuilder<StringPackageInfo, CommandLinePipelineFilter>()
    .UseKestrelChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

var app = builder.Build();

await app.RunAsync();