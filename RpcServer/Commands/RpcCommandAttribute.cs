using SuperSocket.Command;

namespace RpcServer.Abp.Commands;

public sealed class RpcCommandAttribute : CommandAttribute
{
    public RpcCommandAttribute(CommandKey key)
    {
        Key = (byte)key;
    }
}
