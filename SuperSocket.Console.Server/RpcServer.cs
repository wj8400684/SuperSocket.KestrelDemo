using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.Console.Server;

public sealed class RpcServer : SuperSocketService<RpcPackageBase>
{
    public RpcServer(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
    {
    }
}
