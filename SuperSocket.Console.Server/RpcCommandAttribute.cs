using SuperSocket.Command;

namespace SuperSocket.Console.Server;

public sealed class RpcCommandAttribute : CommandAttribute
{
    public RpcCommandAttribute(CommandKey key)
    {
        Key = (byte)key;
    }
}
