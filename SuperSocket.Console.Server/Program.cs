using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.Console.Server;
using SuperSocket.Console.Server.Commands;
using SuperSocket.ProtoBase;

var host = SuperSocketHostBuilder.Create<RpcPackageBase, RpcPipeLineFilter>()
    .UseHostedService<RpcServer>()
    .UseSession<RpcSession>()
    .UsePackageDecoder<RpcPackageDecoder>()
    .UseCommand(options => options.AddCommandAssembly(typeof(Login).Assembly))
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddSingleton<IPackageEncoder<RpcPackageBase>, RpcPackageEncode>();
    })
    .Build();

await host.RunAsync();