using System;
using System.Runtime.InteropServices;
namespace IziHardGames.STUN.Domain.Headers
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 12, CharSet = CharSet.Ansi)]
    public struct TransactionID
    {
        public const int SIZE = 12;

        [FieldOffset(0)] public int value0;
        [FieldOffset(4)] public int value1;
        [FieldOffset(8)] public int value2;

        public string Info => ToStringInfo();

        public static bool operator ==(TransactionID a, TransactionID b) => a.value0 == b.value0 && a.value1 == b.value1 && a.value2 == b.value2;
        public static bool operator !=(TransactionID a, TransactionID b) => a.value0 != b.value0 && a.value1 != b.value1 && a.value2 != b.value2;

        public string ToStringInfo()
        {
            return $"{Convert.ToHexString(BitConverter.GetBytes(value0))}{Convert.ToHexString(BitConverter.GetBytes(value1))}{Convert.ToHexString(BitConverter.GetBytes(value2))}";
        }
    }
}
