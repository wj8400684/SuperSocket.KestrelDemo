using SuperSocket.ProtoBase;
using System.Buffers;

namespace RpcCore;

public sealed class RpcPackageDecoder : IPackageDecoder<RpcPackageBase>
{
    private const int HeaderSize = sizeof(short);

    private readonly IPacketFactoryPool _packetFactoryPool;

    public RpcPackageDecoder(IPacketFactoryPool packetFactoryPool)
    {
        _packetFactoryPool = packetFactoryPool;
    }

    public RpcPackageBase Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.Advance(HeaderSize);

        //读取 command
        reader.TryRead(out var command);

        var packetFactory = _packetFactoryPool.Get(command) ?? throw new ProtocolException($"命令：{command}未注册");
        
        var package = packetFactory.Create();

        package.DecodeBody(ref reader, package);

        return package;
    }
}
