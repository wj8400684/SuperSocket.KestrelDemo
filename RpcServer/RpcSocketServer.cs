using Microsoft.Extensions.Options;
using SuperSocket.Server;
using SuperSocket;

namespace RpcServer.Abp;

public sealed class RpcSocketServer : SuperSocketService<RpcPackageBase>
{
    public RpcSocketServer(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
    {
    }
}
