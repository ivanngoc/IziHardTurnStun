using System.Runtime.InteropServices;

namespace System
{
	public static class ExtensionUnmanaged
	{
		public static byte GetByte<T>(this T value, int index) where T : unmanaged
		{
			return value.ToSpanOfBytes<T>()[index];
		}
		public static Span<byte> ToSpanOfBytes<T>(this T value) where T : unmanaged
		{
			unsafe
			{
				return new Span<byte>(&value, Marshal.SizeOf<T>());
			}
		}
		public static T ReverseByteOrder<T>(this T value) where T : unmanaged
		{
			int size = Marshal.SizeOf<T>();
			int countSwap = size >> 1;
			int left = -1;
			int right = size;

			unsafe
			{
				byte* ptr = (byte*)&value;

				for (int i = 0; i < countSwap; i++)
				{
					left++;
					right--;

					byte temp = ptr[left];
					ptr[left] = ptr[right];
					ptr[right] = temp;
				}
				return value;
			}
		}
	}
}