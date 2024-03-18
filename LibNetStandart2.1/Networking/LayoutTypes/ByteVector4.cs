using System;
using System.Runtime.InteropServices;

namespace IziHardGames.Networking.ByteVectors
{


	[StructLayout(LayoutKind.Explicit, Size = 2)]
	public struct ByteVector2
	{
		[FieldOffset(0)] public byte byte0;
		[FieldOffset(1)] public byte byte1;

		public static implicit operator ByteVector2(UInt16 value)
		{
			unsafe
			{
				ByteVector2 byteVector2 = default;
				byte* ptr = (byte*)&byteVector2;
				byte* ptrVal = (byte*)&value;
				ptr[0] = ptrVal[1];
				ptr[1] = ptrVal[0];
				return byteVector2;
			}
		}
		public static implicit operator UInt16(ByteVector2 value)
		{
			unsafe
			{
				UInt16 result = default;
				byte* ptr = (byte*)&result;
				byte* ptrVal = (byte*)&value;
				ptr[0] = ptrVal[1];
				ptr[1] = ptrVal[0];
				return result;
			}
		}

		public static bool operator !=(ByteVector2 a, ByteVector2 b)
		{
			return a.byte0 != b.byte0 || a.byte1 != b.byte1;
		}
		public static bool operator ==(ByteVector2 a, ByteVector2 b)
		{
			return a.byte0 == b.byte0 && a.byte1 == b.byte1;
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 3)]
	public struct ByteVector3
	{
		[FieldOffset(0)] public byte byte0;
		[FieldOffset(1)] public byte byte1;
		[FieldOffset(1)] public byte byte3;
	}
	/// <summary>
	/// <see cref="Int32"/> <see cref="int"/> <see cref="uint"/>
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct ByteVector4
	{
		[FieldOffset(0)] public byte byte0;
		[FieldOffset(1)] public byte byte1;
		[FieldOffset(2)] public byte byte2;
		[FieldOffset(3)] public byte byte3;

		public UInt16 ToUInt16()
		{
			throw new NotImplementedException();
		}

		public static ByteVector4 FromUInt(UInt16 value)
		{
			ByteVector4 byteVector4 = 0b0000_0000;

			throw new NotImplementedException();
		}

		/// <summary>
		/// From Little Endian <see cref="UInt16"/> to BigEndian (Human Readable)
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator ByteVector4(UInt16 value)
		{
			unsafe
			{
				ByteVector4 byteVector = default;
				byte* ptr = (byte*)&byteVector;
				byte* ptrVal = (byte*)&value;
				ptr[0] = ptrVal[3];
				ptr[1] = ptrVal[2];
				ptr[2] = ptrVal[1];
				ptr[3] = ptrVal[0];
				return byteVector;
			}
		}
	}
}
