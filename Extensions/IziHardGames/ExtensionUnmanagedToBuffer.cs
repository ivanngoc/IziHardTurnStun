using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	public static class ExtensionUnmanagedToBuffer
	{
		public static bool TryCopyToBuffer<T>(this T value, byte[] buffer, int offset, ref int position) where T : unmanaged
		{
			int size = Marshal.SizeOf<T>();

			if ((offset + size) > buffer.Length)
			{
				return false;
			}
			position = value.CopyToBuffer(buffer, offset);

			return true;
		}
		public static int CopyToBuffer<T>(this T value, byte[] buffer, int offset) where T : unmanaged
		{
			int size = Marshal.SizeOf<T>();
			Span<byte> span = new Span<byte>(buffer, offset, size);
			MemoryMarshal.Write(span, ref value);
			return offset + size;
		}
	}
}