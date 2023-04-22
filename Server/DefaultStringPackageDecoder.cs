using SuperSocket.ProtoBase;
using System.Buffers;
using System.Text;

namespace Server;

internal sealed class DefaultStringPackageDecoder1 : IPackageDecoder<StringPackageInfo>
{
    private readonly static ReadOnlyMemory<byte> Space = " "u8.ToArray();

    public Encoding Encoding { get; private set; }

    public DefaultStringPackageDecoder1()
        : this(new UTF8Encoding(false))
    {
    }

    public DefaultStringPackageDecoder1(Encoding encoding)
    {
        Encoding = encoding;
    }

    //key paramter \r\n
    //ADD 1 2 3\r\n
    public StringPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);

        //尝试读取到 Spen.Span 空格的位置
        if (!reader.TryReadTo(out ReadOnlySequence<byte> sequence, Space.Span, true))
            return default!;

        //获取key
        var key = Encoding.GetString(sequence.FirstSpan);

        //只有命令没有参数
        if (reader.UnreadSpan.IsEmpty)
        {
            return new StringPackageInfo
            {
                Key = key
            };
        }

        var list = new string[1];

        while (!reader.UnreadSpan.IsEmpty)
        {
            string parameter;

            //读取到多个参数
            if (reader.TryReadTo(out sequence, Space.Span, true))
            {
                parameter = Encoding.GetString(sequence.FirstSpan);
                list[0] = parameter;
                continue;
            }

            if (reader.UnreadSpan.IsEmpty)
                continue;

            parameter = Encoding.GetString(reader.UnreadSpan);

            list[0] = parameter;

            reader.AdvanceToEnd();
        }

        return new StringPackageInfo
        {
            Key = key,
            Body = reader.UnreadSequence.GetString(Encoding),
            Parameters = list
        };
    }
}
