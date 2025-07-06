using System;
using System.Collections.Generic;
using System.Text;

namespace IziHardGames.Libs.Networking.BinaryConverters
{
    public static class BinaryConverter
    {
        public static int ToInt32(byte first, byte second)
        {
            int result = 0;

            unsafe
            {
                byte* ptr = (byte*)&result;
                ptr[0] = first;
                ptr[1] = second;
            }
            return result;
        }

        public static ushort ToUshort(byte first, byte second)
        {
            throw new NotImplementedException();
        }

    }
    public static class BinaryOrder
    {
        public static ushort ReverseEndian(int value)
        {
            unsafe
            {
                ushort result = default;
                byte* dest = (byte*)&result;
                byte* source = (byte*)&value;
                dest[1] = source[0];
                dest[0] = source[1];
                return result;
            }
        }
    }
}
