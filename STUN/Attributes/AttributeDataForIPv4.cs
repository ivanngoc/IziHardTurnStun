using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IziHardGames.STUN.Domain.Headers;

namespace IziHardGames.STUN.Attributes
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.1
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.2
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = AttributeDataForIPv4.SIZE)]
    public struct AttributeDataForIPv4
    {
        public const int SIZE = 4;
        public const int SIZE_WHOLE = SIZE + StunAddress.SIZE;

        [FieldOffset(0)] public int xAddress;

        public IPAddress IPAddress
        {
            get
            {
                return new IPAddress(BitConverter.GetBytes(xAddress));
            }
        }
        public IPAddress IPAddressXor
        {
            get
            {
                return new IPAddress(BitConverter.GetBytes((xAddress ^ StunHeader.MAGIC_COOKIE_FOR_CLIENT)));
            }
        }
        private AttributeDataForIPv4(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = address.GetAddressBytes();
                xAddress = BitConverter.ToInt32(bytes) ^ StunHeader.MAGIC_COOKIE_FOR_CLIENT;
            }
            else
            {
                throw new ArgumentException($"Address must be IPv4");
            }
        }
        public static AttributeDataForIPv4 Xor(IPAddress iPAddress)
        {
            return new AttributeDataForIPv4(iPAddress);
        }
        public static AttributeDataForIPv4 FromMemory(ReadOnlySpan<byte> memory)
        {
            return MemoryMarshal.Cast<byte, AttributeDataForIPv4>(memory.Slice(StunAddress.SIZE, SIZE))[0];
        }

        public string ToStringInfo()
        {
            return $"{nameof(AttributeDataForIPv4)} {IPAddress.ToString()}";
        }
        public string ToStringInfoXor()
        {
            return $"{nameof(AttributeDataForIPv4)} {IPAddressXor.ToString()}";
        }


    }
}
