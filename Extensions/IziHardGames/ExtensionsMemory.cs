using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    public static class ExtensionsMemory
    {
        /// <see cref="System.Runtime.InteropServices.Marshal"/>
        /// <see cref="System.Runtime.InteropServices.MemoryMarshal"/>
        /// <see cref="System.Runtime.InteropServices.SequenceMarshal"/>
        /// 
        /// <see cref="System.Runtime.CompilerServices.Unsafe"/>
        /// <see cref="System.Buffer"/>

        /// https://stackoverflow.com/questions/59577415/understanding-the-return-value-of-memorymarshal-cast
        public static T ToStruct<T>(this Memory<byte> memory) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(memory.Span)[0];
        } 
        public static T ToStruct<T>(this ReadOnlySpan<byte> memory) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(memory)[0];
        }
        public static T ToStruct<T>(this Span<byte> memory) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(memory)[0];
        }
        public static string ToUtf16(this ReadOnlySpan<byte> memory)
        {
            return new string(MemoryMarshal.Cast<byte, char>(memory));
        }
        public static string ToUtf16(this Span<byte> memory) 
        {
            return new string(MemoryMarshal.Cast<byte, char>(memory));
        }
        public static string ToAscii(this Span<byte> memory)
        {
            return Encoding.ASCII.GetString(memory);
        }
        public static string ToAscii(this ReadOnlySpan<byte> memory)
        {
            return Encoding.ASCII.GetString(memory);
        }
        public static string ToBase64(this ReadOnlySpan<byte> memory)
        {
            return Convert.ToBase64String(memory);
        }
        public static string ToHex(this ReadOnlySpan<byte> memory)
        {
            return Convert.ToHexString(memory);
        }
    }
}