using IziHardGames.Libs.Networking.BinaryConverters;
using IziHardGames.Networking.ByteVectors;
using IziHardGames.Networking.IANA;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static IziHardGames.STUN.StunHeader;
using TData = System.ReadOnlySpan<byte>;

namespace IziHardGames.STUN.Attributes
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.1
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.2
    ///  The XOR-MAPPED-ADDRESS attribute is identical to the MAPPED-ADDRESS
    ///  attribute, except that the reflexive transport address is obfuscated
    ///  through the XOR function.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = StunAddress.SIZE)]
    public struct StunAddress
    {
        public const int SIZE = 4;

        [FieldOffset(0)] private byte placeholder;
        [FieldOffset(1)] public byte family;
        [FieldOffset(2)] public ushort xport;

        public int Port
        {
            get => BinaryOrder.ReverseEndian(xport);
            set => BinaryOrder.ReverseEndian(value);
        }
        public int PortXor
        {
            get => xport.ReverseBytesCopy() ^ 0x2112;
            set => xport = BinaryOrder.ReverseEndian(value ^ 0x2112);
        }


        public AddressFamily AddressFamily
        {
            get
            {
                if (family == ConstantsForStun.IPv4) return AddressFamily.InterNetwork;
                if (family == ConstantsForStun.IPv6) return AddressFamily.InterNetworkV6;
                throw new ArgumentOutOfRangeException($"{family}");
            }
        }


        private StunAddress(IPAddress address, int port) : this()
        {
            var bytes = address.GetAddressBytes();

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                family = ConstantsForStun.IPv4;
            }
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                family = ConstantsForStun.IPv6;
            }
            this.PortXor = port;
        }

        public static StunAddress Xor(IPAddress address, int port)
        {
            return new StunAddress(address, port);
        }
        public static StunAddress FromMemory(TData memory)
        {
            return memory.ToStruct<StunAddress>();
        }
        public static StunAddress FromMemory(Memory<byte> memory)
        {
            return memory.ToStruct<StunAddress>();
        }

        public string GetFamilty()
        {
            switch (family)
            {
                case ConstantsForStun.IPv4: return nameof(ConstantsForStun.IPv4);
                case ConstantsForStun.IPv6: return nameof(ConstantsForStun.IPv6);
                default: throw new ArgumentOutOfRangeException($"{family}");
            }
        }
        public string ToStringInfo()
        {
            return $"{nameof(StunAddress)}: Family:{GetFamilty()} Port {Port}  ";
        }
        public string ToStringInfoXor()
        {
            return $"{nameof(StunAddress)}: Family:{GetFamilty()} PortXor {PortXor}  ";
        }
    }
}
