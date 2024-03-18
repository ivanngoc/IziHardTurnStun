using IziHardGames.Networking.ByteVectors;
using IziHardGames.Networking.IANA;
using System;
using System.Runtime.InteropServices;
using TData = System.ReadOnlySpan<byte>;

namespace IziHardGames.TURN
{
	/// <summary>
	/// https://datatracker.ietf.org/doc/html/rfc5766#section-14.7
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct TurnRequestedTransport
	{
		[FieldOffset(0)] public byte protool;
		/// <summary>
		/// Reserved For Future Uses <br/>
		///  The RFFU field MUST be set to zero on transmission and MUST be ignored on reception.It is reserved for future uses.
		/// </summary>
		[FieldOffset(1)] public ByteVector3 rffu;

		public TurnRequestedTransport(byte protool) : this()
		{
			this.protool = protool;
			this.rffu = default;
		}

		public string ToStringInfo()
		{
			return $"{ProtocolNumber.GetProtocol(protool)}";
		}
		public static TurnRequestedTransport FromBuffer(TData memory)
		{
			return memory.ToStruct<TurnRequestedTransport>();
		}
		public static int ToBuffer(byte[] memory, int offset, EProtocolNumber eProtocolNumber)
		{
			memory[offset] = (byte)eProtocolNumber;
			int end = offset + 4;
			for (int i = offset; i < end; i++)
			{
				memory[i] = default;
			}
			return end;
		}
	}
}
