using System.Buffers;
using SuperSocket.ProtoBase;

namespace RpcCore;

public sealed class RpcPackageDecoder : IPackageDecoder<RpcPackageBase>
{
    private const int HeaderSize = sizeof(short);

    private IPacketFactory[] _packetFactories;

    interface IPacketFactory
    {
        RpcPackageBase Create();
    }

    class DefaultPacketFactory<TPacket> : IPacketFactory
        where TPacket : RpcPackageBase, new()
    {
        public RpcPackageBase Create()
        {
            return new TPacket();
        }
    }

    public RpcPackageDecoder()
    {
        var commands = RpcPackageBase.GetCommands();

        _packetFactories = new IPacketFactory[commands.Count + 1];

        foreach (var command in commands)
        {
            var genericType = typeof(DefaultPacketFactory<>).MakeGenericType(command.Key);

            if (Activator.CreateInstance(genericType) is not IPacketFactory packetFactory)
                continue;

            _packetFactories[(int)command.Value] = packetFactory;
        }
    }

    RpcPackageBase IPackageDecoder<RpcPackageBase>.Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.Advance(HeaderSize);

        //¶ÁÈ¡ command
        reader.TryRead(out var command);

        var packetFactory = _packetFactories[command];

        if (packetFactory == null)
            throw new ProtocolException($"ÃüÁî£º{command}Î´×¢²á");

        var package = packetFactory.Create();

        package.DecodeBody(ref reader, package);

        return package;
    }
}

/// <summary>
/// | bodyLength | body |
/// | header | cmd | body |
/// </summary>
public sealed class RpcPipeLineFilter : FixedHeaderPipelineFilter<RpcPackageBase>
{
    private const int HeaderSize = sizeof(short);

    public RpcPipeLineFilter()
        : base(HeaderSize)
    {
        Decoder = new RpcPackageDecoder();
    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.TryReadLittleEndian(out short bodyLength);

        return bodyLength;
    }
}