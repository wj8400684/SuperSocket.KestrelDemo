using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Client;

internal sealed class DefaultStringPackageDecoder : IPackageDecoder<StringPackageInfo>
{
    private readonly static ReadOnlyMemory<byte> Spen = " "u8.ToArray();

    public Encoding Encoding { get; private set; }

    public DefaultStringPackageDecoder()
        : this(new UTF8Encoding(false))
    {
    }

    public DefaultStringPackageDecoder(Encoding encoding)
    {
        Encoding = encoding;
    }

    //key paramter \r\n
    //ADD 1 2 3\r\n
    public StringPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);

        //尝试读取到 Spen.Span 空格的位置
        if (!reader.TryReadTo(out ReadOnlySequence<byte> sequence, Spen.Span, true))
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

        var list = new List<string>();

        while (!reader.UnreadSpan.IsEmpty)
        {
            string parameter;

            //读取到多个参数
            if (reader.TryReadTo(out sequence, Spen.Span, true))
            {
                parameter = Encoding.GetString(sequence.FirstSpan);
                list.Add(parameter);
                continue;
            }

            if (reader.UnreadSpan.IsEmpty)
                continue;

            parameter = Encoding.GetString(reader.UnreadSpan);

            list.Add(parameter);
            
            reader.AdvanceToEnd();
        }

        return new StringPackageInfo
        {
            Key = key,
            Body = reader.UnreadSequence.GetString(Encoding),
            Parameters = list.ToArray()
        };
    }
}