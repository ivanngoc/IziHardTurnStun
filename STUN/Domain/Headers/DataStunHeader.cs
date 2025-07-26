using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
namespace IziHardGames.STUN.Domain.Headers
{
    [StructLayout(LayoutKind.Explicit, Size = 20, CharSet = CharSet.Ansi)]
    public struct DataStunHeader
    {
        [FieldOffset(0)] public MessageType messageType;
        [FieldOffset(2)] private ushort length;
        /// <summary>
        /// <see cref="StunHeader.MAGIC_COOKIE_FOR_CLIENT"/>
        /// </summary>
        [FieldOffset(4)] public uint magicCookie;
        [FieldOffset(8)] public TransactionID transactionId;

        public ushort Length => BinaryPrimitives.ReverseEndianness(length);

        public static DataStunHeader FromBuffer(byte[] rawHeader)
        {
            return MemoryMarshal.Cast<byte, DataStunHeader>(new Span<byte>(rawHeader))[0];
        }

        public static void WriteHeaderToBuffer(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public string ToStringInfo()
        {
            return
                $"{nameof(DataStunHeader)} " +
                $"Class: {messageType.GetStunClass()}    |   " +    
                $"Method: {messageType.GetStunMethodName()} |   " +
                $"Length: {Length}  |   " +
                $"MagicCookie: {BinaryPrimitives.ReverseEndianness(magicCookie).ToString("X2")} |   " +
                $"TransactionId {transactionId.ToStringInfo()}";
        }

        public static void SetMethod(Span<byte> span, ushort method)
        {
            var spanCasted = MemoryMarshal.Cast<byte, ushort>(span);
            spanCasted[0] = (ushort)(spanCasted[0] & MessageType.MASK_MESSAGE_CLASS_BITS_BIG_ENDIAN | method.ReverseByteOrder());
        }
    }
}
