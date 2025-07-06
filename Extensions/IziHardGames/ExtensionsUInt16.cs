namespace System
{
	public static class ExtensionsUInt16
	{
		public static void WriteToBufferBigEndian(this UInt16 value, byte[] buffer, int offset)
		{
			unsafe
			{
				byte* ptrvalue = (byte*)&value;
				buffer[offset] = ptrvalue[1];
				buffer[offset + 1] = ptrvalue[0];
			}
		}
	
		public static UInt16 ReverseBytesCopy(this UInt16 value)
		{
			unsafe
			{
				UInt16 result = default;
				byte* ptrvalue = (byte*)&value;
				byte* ptrResult = (byte*)&result;

				ptrResult[0] = ptrvalue[1];
				ptrResult[1] = ptrvalue[0];

				return result;
			}
		}
	}
}
