using MemoryPack;
using System.Buffers;

namespace RpcCore.Packages;

[MemoryPackable]
public sealed partial class InvokerPackage : RpcPackageBase
{
    public InvokerPackage() 
        : base(CommandKey.Invoker)
    {
    }

    public CommandKey Command { get; set; }

    public ReadOnlyMemory<byte> Body { get; set; }

    public override int Encode(IBufferWriter<byte> bufWriter)
    {
        var length = base.Encode(bufWriter);

        return length;
    }

    protected internal override void DecodeBody(ref SequenceReader<byte> reader, object? context)
    {
        base.DecodeBody(ref reader, context);
    }
}

[MemoryPackable]
public sealed partial class InvokerRespPackage : RpcRespPackage
{
    public InvokerRespPackage()
        : base(CommandKey.InvokerAck)
    {
    }

    public CommandKey Command { get; set; }

    public ReadOnlyMemory<byte> Body { get; set; }
}
