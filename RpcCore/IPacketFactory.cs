namespace RpcCore;

public interface IPacketFactory
{
    RpcPackageBase Create();
}

public sealed class DefaultPacketFactory<TPacket> : IPacketFactory
        where TPacket : RpcPackageBase, new()
{
    public RpcPackageBase Create()
    {
        return new TPacket();
    }
}