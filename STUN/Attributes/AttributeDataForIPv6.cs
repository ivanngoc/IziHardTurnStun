using System;
using System.Runtime.InteropServices;
using IziHardGames.STUN.Domain.Headers;

namespace IziHardGames.STUN.Attributes
{
    [StructLayout(LayoutKind.Explicit, Size = AttributeDataForIPv6.SIZE)]
    public struct AttributeDataForIPv6
    {
        public const int SIZE = 16;
        [FieldOffset(0)] public long address0;
        [FieldOffset(8)] public long address1;

        public static AttributeDataForIPv6 FromMemory(ReadOnlySpan<byte> memory)
        {
            return MemoryMarshal.Cast<byte, AttributeDataForIPv6>(memory.Slice(StunAddress.SIZE, SIZE))[0];
        }

        public string ToStringInfo()
        {
            throw new NotImplementedException();
        }
        public string ToStringInfoXor()
        {
            throw new NotImplementedException();
        }
    }
}
