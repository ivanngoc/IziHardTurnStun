using System;
using System.BinaryString;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IziHardGames.STUN.STUN;
using static IziHardGames.STUN.ConstantsForStun;
namespace IziHardGames.STUN.Domain.Headers
{
    /// <summary>
    /// <see cref="EStunMethod"/><br/>
    /// <cref="https://www.iana.org/assignments/stun-parameters/stun-parameters.xhtml"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2, CharSet = CharSet.Ansi)]
        public struct MessageType
        {


            // class little endian
            /// <summary>
            /// Из битов где стоит 1 формируется флаг 0b00, который определяется класс сообщения<br/>
            /// 0b00-request<br/>
            /// 0b01-indication<br/>
            /// 0b10-success response<br/>
            /// 0b11-error response<br/>
            /// <cref="https://datatracker.ietf.org/doc/html/rfc5389#section-6"/>
            /// <see cref="EStunMethod"/>
            /// </summary>
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_BITS = 0b_0000_0001_0001_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INV = 0b_1111_1110_1110_1111;
            public const int MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INVERT_AS_INT = ~MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            public const ushort MASK_MESSAGE_CLASS_BITS_BIG_ENDIAN = 0b_0001_0000_0000_0001;

            public const byte MASK_CLASS_INVERT_BYTE0 = 0b0011_1110;
            public const byte MASK_CLASS_INVERT_BYTE1 = 0b1110_1111;

            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_REQUEST = 0b_0000_0000_0000_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_INDICATION = 0b_0000_0000_0001_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_SUCCESS = 0b_0000_0001_0000_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_ERROR = MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            // class bigendian
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS = 0b_0001_0000_0000_0001;
            public const int BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INVERT = ~BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_REQUEST = 0b_0000_0000_0000_0000;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_INDICATION = 0b_0001_0000_0000_0000;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_SUCCESS = 0b_0000_0000_0000_0001;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_ERROR = BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            /// <summary>
            /// по битам где стоит 1 образуется флаг определяющий STUN метод
            /// </summary>
            public const ushort MASK_STUN_MESSAGE_METHOD_BITS = 0b_1111_0001_1110_1111;
            public const ushort BE_MASK_STUN_MESSAGE_METHOD_BITS = 0b_1110_1111_1111_0001;
            public const int BE_MASK_STUN_MESSAGE_METHOD_BITS_INVERTED = ~BE_MASK_STUN_MESSAGE_METHOD_BITS;

            [FieldOffset(0)] public ushort value;
            public ushort Value => BinaryPrimitives.ReverseEndianness(value);
            public int Class => GetStunClassValue();
            public byte ClassAsByte => (byte)GetStunMethodValue();
            public int Method => GetStunMethodValue();
            public string MethodName => GetStunMethodName();

            public readonly static Dictionary<int, string> methodsByValueLittleEndian = new Dictionary<int, string>();
            static MessageType()
            {
                methodsByValueLittleEndian = new Dictionary<int, string>()
                {
                    // STUN
                    [METHOD_RESERVED] = nameof(METHOD_RESERVED),
                    [METHOD_BINDING] = nameof(METHOD_BINDING),
                    [METHOD_RESERVER_WAS_SHARED_SECRET] = nameof(METHOD_RESERVER_WAS_SHARED_SECRET),
                };
            }
            public int GetStunClassValue()
            {
                return Value >> 7 & 0b0010 | Value >> 4 & 0b0001;
            }
            public int GetStunMethodValue()
            {
                return Value >> 2 & 0b1111_1000_0000 | Value >> 1 & 0b0111_0000 | Value & 0b1111;
            }
            public string GetStunMethodName()
            {
                return methodsByValueLittleEndian[Value & MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INVERT_AS_INT];
            }
            public EStunClass GetStunClass()
            {
                int classValue = GetStunClassValue();
                if (classValue == CLASS_REQUEST) return EStunClass.Request;
                if (classValue == CLASS_ERROR_RESPONSE) return EStunClass.ResponseError;
                if (classValue == CLASS_SUCCESS_RESPONSE) return EStunClass.ResponseSuccess;
                if (classValue == CLASS_INDICATION) return EStunClass.Indication;
                throw new ArgumentOutOfRangeException(Value.ToStringBinaryPerCharForHumanSeparateQuad(' '));
            }
        }
    

}
