using Microsoft.AspNetCore.Connections;
using Server;
using SuperSocket;
using SuperSocket.Kestrel;
using SuperSocket.ProtoBase;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel((context, options) =>
//{
//    var serverOptions = builder.Configuration.GetSection("ServerOptions").Get<ServerOptions>()!;

//    foreach (var listeners in serverOptions.Listeners)
//    {
//        options.Listen(listeners.GetListenEndPoint(),
//            listenOptions => { listenOptions.UseConnectionHandler<KestrelChannelCreator>(); });
//    }
//});

builder.Host.AsSuperSocketHostBuilder<StringPackageInfo, CommandLinePipelineFilter>()
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .UseChannelCreatorFactory<TcpIocpChannelCreatorFactory>()
    //.UseKestrelChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

var app = builder.Build();

await app.RunAsync();