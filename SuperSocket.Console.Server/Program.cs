using SuperSocket;
using SuperSocket.Command;
using SuperSocket.Console.Server;
using SuperSocket.Console.Server.Commands;

var host = SuperSocketHostBuilder.Create<RpcPackageBase, RpcPipeLineFilter>()
    .UseHostedService<RpcServer>()
    .UseSession<RpcSession>()
    .UsePackageDecoder<RpcPackageDecoder>()
    .UsePackageEncoder<RpcPackageEncode>()
    .UseCommand(options => options.AddCommandAssembly(typeof(Login).Assembly))
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
    })
    .Build();

await host.RunAsync();