using System;
using System.BinaryString;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using IziHardGames.STUN.Attributes;
using IziHardGames.STUN.STUN;
using static IziHardGames.STUN.ConstantsForStun;

namespace IziHardGames.STUN.Domain.Headers
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15
    /// https://www.iana.org/assignments/stun-parameters/stun-parameters.xhtml
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = SIZE, CharSet = CharSet.Ansi)]
    public struct AttributeForStun
    {
        /// <summary>
        /// Key = const of fields with name INT_* below<br/>
        /// Value = <see cref="EStunClass"/>
        /// </summary>
        public readonly static Dictionary<int, string> stunAttributeByValue;

        public const int SIZE = 4;              // 0xFFFF;
        public const int INT_NOT_SET = 0xFFFF;  // 0xFFFF;

        public const ushort NOT_SET = ushort.MaxValue;



        [FieldOffset(0)] private ushort type;
        [FieldOffset(2)] private ushort length;
        public ushort Length => BinaryPrimitives.ReverseEndianness(length);
        public ushort Type => BinaryPrimitives.ReverseEndianness(type);
        public string Name => GetAttributeName();

        static AttributeForStun()
        {
            //10 STUN original + 9 extension from TURN
            stunAttributeByValue = new Dictionary<int, string>()
            {
                [NOT_SET] = nameof(NOT_SET),
                // ORIGIN
                [ATTR_Mapped_Address] = nameof(ATTR_Mapped_Address),
                [ATTR_XOR_Mapped_Address] = nameof(ATTR_XOR_Mapped_Address),
                [ATTR_Username] = nameof(ATTR_Username),
                [ATTR_MessageIntegrity] = nameof(ATTR_MessageIntegrity),
                [ATTR_Fingerprint] = nameof(ATTR_Fingerprint),
                [ATTR_Error_Code] = nameof(ATTR_Error_Code),
                [ATTR_Realm] = nameof(ATTR_Realm),
                [ATTR_Nonce] = nameof(ATTR_Nonce),
                [ATTR_Unknown_Attributes] = nameof(ATTR_Unknown_Attributes),
                [ATTR_Software] = nameof(ATTR_Software),
                [ATTR_Alternate_Server] = nameof(ATTR_Alternate_Server),

                //ICE
                [ATTR_PRIORITY] = nameof(ATTR_PRIORITY),
                [ATTR_USE_CANDIDATE] = nameof(ATTR_USE_CANDIDATE),
                [ATTR_ICE_CONTROLLED] = nameof(ATTR_ICE_CONTROLLED),
                [ATTR_ICE_CONTROLLING] = nameof(ATTR_ICE_CONTROLLING),

                [ATTR_CHANGE_REQUEST] = nameof(ATTR_CHANGE_REQUEST),
                [ATTR_PADDING] = nameof(ATTR_PADDING),
                [ATTR_RESPONSE_PORT] = nameof(ATTR_RESPONSE_PORT),
                [ATTR_CACHE_TIMEOUT] = nameof(ATTR_CACHE_TIMEOUT),
                [RESPONSE_ORIGIN] = nameof(RESPONSE_ORIGIN),
                [OTHER_ADDRESS] = nameof(OTHER_ADDRESS),
            };
        }

        public string GetAttributeName()
        {
#if DEBUG
            if (stunAttributeByValue.TryGetValue(Type, out string stunAttribute))
            {
                return stunAttribute;
            }
            else
            {
                ConsoleWrap.Red($"GetAttribute(): STUN TYPE IS NOT SET TO HANDLE type {Type}; HEX: {BitConverter.ToString(BitConverter.GetBytes(type))}");
                return stunAttributeByValue[NOT_SET];
            }
#else
				return eStunAttributeByValue[key];
#endif
        }

        public string ToStringInfo()
        {
            return
                $"Type: {BitConverter.ToString(BitConverter.GetBytes(Type.ReverseByteOrder()))}	" +
                $"EStunAttribute: {GetAttributeName()}  " +
                $"Length: {Length}";
        }

        #region Static
        public static AttributeForStun FromBuffer(byte[] buffer, int indexStart)
        {
            return MemoryMarshal.Cast<byte, AttributeForStun>(new Span<byte>(buffer, indexStart, SIZE))[0];
        }
        public static T GetObject<T>(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Данные переставлятся с Network byte order к Host Byte Order == BE => LE
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Положить <see cref="AttributeForStun"/> в буфер<br/>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-15
        /// </summary>
        /// <param name="bufferSend"></param>
        /// <param name="offset">index in bufferSend to write <see cref="AttributeForStun"/> then data</param>
        public static int PutToBuffer(byte[] bufferSend, int offset, ushort type, ReadOnlySpan<byte> data, ushort lengthDataAligned)
        {
            unsafe
            {
                ushort dataLenght = (ushort)data.Length;
                byte* ptrType = (byte*)&type;
                byte* ptrlength = (byte*)&dataLenght;

                bufferSend[offset] = ptrType[1];
                bufferSend[offset + 1] = ptrType[0];
                bufferSend[offset + 2] = ptrlength[1];
                bufferSend[offset + 3] = ptrlength[0];

                Span<byte> dist = new Span<byte>(bufferSend, offset + SIZE, data.Length);
                data.CopyTo(dist);

                return offset + SIZE + lengthDataAligned;
            }
        }
        public static int PutToBuffer(byte[] bufferSend, int offset, ushort type, Memory<byte> data, ushort lengthDataAligned)
        {
            unsafe
            {
                var span = data.Span;
                var lengthData = span.Length;
                byte* ptrType = (byte*)&type;
                byte* ptrlength = (byte*)&lengthData;

                bufferSend[offset] = ptrType[1];
                bufferSend[offset + 1] = ptrType[0];
                bufferSend[offset + 2] = ptrlength[1];
                bufferSend[offset + 3] = ptrlength[0];

                Span<byte> dist = new Span<byte>(bufferSend, offset + SIZE, data.Length);
                span.CopyTo(dist);

                return offset + SIZE + lengthDataAligned;
            }
        }
        public static int PutToBuffer(byte[] bufferSend, int offset, ushort type, int dataLength, byte[] data, ushort messageLength)
        {
            unsafe
            {
                byte* ptrType = (byte*)&type;
                byte* ptrlength = (byte*)&dataLength;

                bufferSend[offset] = ptrType[1];
                bufferSend[offset + 1] = ptrType[0];
                bufferSend[offset + 2] = ptrlength[1];
                bufferSend[offset + 3] = ptrlength[0];

                Buffer.BlockCopy(data, 0, bufferSend, offset + SIZE, dataLength);

                return offset + SIZE + messageLength;
            }
        }
        public static int PutToBufferDummy(byte[] bufferSend, int offset, ushort type, int lengthData)
        {
            unsafe
            {
                byte* ptr = (byte*)&type;
                byte* ptrlength = (byte*)&lengthData;

                bufferSend[offset] = ptr[1];
                bufferSend[offset + 1] = ptr[0];
                bufferSend[offset + 2] = ptrlength[1];
                bufferSend[offset + 3] = ptrlength[0];

                return offset + SIZE + lengthData;
            }
        }
        public static int PutToBuffer<T>(byte[] buffer, int offset, ushort length, ushort type, T item) where T : unmanaged
        {
            unsafe
            {
                byte* ptrType = (byte*)&type;
                byte* ptrlength = (byte*)&length;

                buffer[offset] = ptrType[1];
                buffer[offset + 1] = ptrType[0];

                buffer[offset + 2] = ptrlength[1];
                buffer[offset + 3] = ptrlength[0];
            }
            Span<T> span = MemoryMarshal.Cast<byte, T>(new Span<byte>(buffer, offset + SIZE, length));
            span[0] = item;
            return offset + length + SIZE;
        }
        public static int PutToBuffer(byte[] buffer, int offset, ushort type, StunAddress address, AttributeDataForIPv4 data)
        {
            unsafe
            {
                ushort length = AttributeDataForIPv4.SIZE_WHOLE;

                byte* ptrType = (byte*)&type;
                byte* ptrlength = (byte*)&length;

                buffer[offset] = ptrType[1];
                buffer[offset + 1] = ptrType[0];

                buffer[offset + 2] = ptrlength[1];
                buffer[offset + 3] = ptrlength[0];

                Span<StunAddress> spanAddress = MemoryMarshal.Cast<byte, StunAddress>(new Span<byte>(buffer, offset + SIZE, StunAddress.SIZE));
                spanAddress[0] = address;
                Span<AttributeDataForIPv4> spanData = MemoryMarshal.Cast<byte, AttributeDataForIPv4>(new Span<byte>(buffer, offset + SIZE + StunAddress.SIZE, AttributeDataForIPv4.SIZE));
                spanData[0] = data;
                return offset + AttributeDataForIPv4.SIZE_WHOLE + SIZE;
            }
        }
        #endregion
    }
}
