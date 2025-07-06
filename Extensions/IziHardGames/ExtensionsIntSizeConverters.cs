using System.Runtime.CompilerServices;

namespace System
{
    public static class ExtensionsIntSizeConverters
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignToBoundry(this int value, int boundry)
        {
            var v = value % boundry;
            if (v > 0)
                return value + (boundry - v);
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AlignToBoundry(this ushort value, int boundry)
        {
            var v = value % boundry;
            if (v > 0) return (ushort)(value + (boundry - v));
            return value;
        }
        public static double ByteToKibiByteAsDouble(this int bytes)
        {
            return bytes / 1024.0;
        }
        public static int ByteToKibiByte(this int bytes)
        {
            return bytes >> 10;
        }
    }
}
