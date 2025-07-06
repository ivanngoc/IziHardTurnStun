using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace System
{
    public static class ExtensionsForStringToByte
    {
        public static ReadOnlySpan<byte> ToByteSpan(this string s)
        {
            return MemoryMarshal.Cast<char, byte>(s.AsSpan());
        }
    }
    public static class ExtensionsForByteArray
    {
        public static int ToIntReverse(this byte[] bytes)
        {
            return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(bytes));
        }
        public static ushort ToUInt(this byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        public static ushort ToUShort(this byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes);
        }
    }
    public static class ExtensionsArrayByteBuffer
    {
        /// <summary>
        /// https://ladeak.wordpress.com/2018/12/28/struct-serialization-with-spans/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="result"></param>
        private static unsafe void ReadPointer<T>(this byte[] data, int offset, out T result) where T : unmanaged
        {
            fixed (byte* pData = &data[offset])
            {
                result = *(T*)pData;
            }
        }
    }
}