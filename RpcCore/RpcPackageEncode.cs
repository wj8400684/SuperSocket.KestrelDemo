using System.Buffers;
using SuperSocket.ProtoBase;
using System.Buffers.Binary;

namespace RpcCore;

public sealed class RpcPackageEncode : IPackageEncoder<RpcPackageBase>
{
    private const byte HeaderSize = sizeof(short);

    public int Encode(IBufferWriter<byte> writer, RpcPackageBase pack)
    {
        #region ��ȡͷ���ֽڻ�����

        var headSpan = writer.GetSpan(HeaderSize);
        writer.Advance(HeaderSize);

        #endregion

        #region д�� command

        var length = writer.WriteLittleEndian((byte)pack.Key);

        #endregion

        #region д������

        length += pack.Encode(writer);

        #endregion

        #region д�� body �ĳ���

        BinaryPrimitives.WriteInt16LittleEndian(headSpan, (short)length);

        #endregion

        return HeaderSize + length;
    }
}