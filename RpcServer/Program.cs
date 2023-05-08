using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.Kestrel;
using SuperSocket.ProtoBase;
using SuperSocket.IOCPTcpChannelCreatorFactory;
using RpcServer.Abp;
using Server;
using RpcServer.Abp.Commands;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel((context, options) =>
//{
//    var serverOptions = context.Configuration.GetSection("ServerOptions").Get<ServerOptions>()!;

//    foreach (var listeners in serverOptions.Listeners)
//    {
//        options.Listen(listeners.GetListenEndPoint(), listenOptions =>
//        {
//            listenOptions.UseConnectionHandler<KestrelChannelCreator>();
//        });
//    }
//});

builder.Host.AsSuperSocketHostBuilder<RpcPackageBase, RpcPipeLineFilter>()
            .UseHostedService<RpcSocketServer>()
            .UseSession<RpcSession>()
            .UsePackageDecoder<RpcPackageDecoder>()
            .UseCommand(options => options.AddCommandAssembly(typeof(Login).Assembly))
            .UseClearIdleSession()
            .UseInProcSessionContainer()
            //.UseChannelCreatorFactory<TcpIocpChannelWithKestrelCreatorFactory>()
            .UseIOCPTcpChannelCreatorFactory()
            .AsMinimalApiHostBuilder()
            .ConfigureHostBuilder();

builder.Services.AddLogging(s => s.AddConsole().AddDebug());
builder.Services.AddSingleton<IPackageEncoder<RpcPackageBase>, RpcPackageEncode>();

var app = builder.Build();

await app.RunAsync();
