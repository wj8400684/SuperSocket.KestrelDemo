using System.Buffers;
using SuperSocket.ProtoBase;

namespace RpcCore;


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
    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.TryReadLittleEndian(out short bodyLength);

        return bodyLength;
    }
}