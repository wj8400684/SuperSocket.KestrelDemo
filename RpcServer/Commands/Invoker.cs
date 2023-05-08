using MemoryPack;
using RpcCore.Packages;
using SuperSocket;

namespace RpcServer.Abp.Commands;

/// <summary>
/// 转发命令
/// 客户端a=>服务端=>客户端b
/// </summary>
//[RpcCommand(CommandKey.Invoker)]
//public sealed class Invoker : RpcAsyncCommand<InvokerPackage>
//{
//    private readonly IPacketFactoryPool _packetFactoryPool;
//    private readonly IPackageHandlingScheduler<RpcPackageBase> _packageHandlingScheduler;

//    public Invoker(IServiceProvider serviceProvider)
//    {
//        _packetFactoryPool = serviceProvider.GetRequiredService<IPacketFactoryPool>();
//        _packageHandlingScheduler = serviceProvider.GetRequiredService<IPackageHandlingScheduler<RpcPackageBase>>();
//    }

//    protected override async ValueTask ExecuteAsync(RpcSession session, InvokerPackage packet, CancellationToken cancellationToken)
//    {
//        var packetFactory = _packetFactoryPool.Get(packet.Command);

//        if (packetFactory == null)
//            return;

//        var invokerPackage = packetFactory.Create();

//        if (invokerPackage == null)
//            return;

//        MemoryPackSerializer.Deserialize(packet.Body.Span, ref invokerPackage);

//        if (invokerPackage == null)
//            return;

//        await _packageHandlingScheduler.HandlePackage(session, invokerPackage);
//    }
//}
