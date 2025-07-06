using System;
using System.BinaryString;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using IziHardGames.Libs.Networking.BinaryConverters;
using static IziHardGames.STUN.ConstantsForStun;
using static IziHardGames.STUN.StunHeader.MessageType;
namespace IziHardGames.STUN
{
    /// <summary>
    /// Contain 20-bytes buffer of STUN header for reusing.
    /// Provide manipulation methods for changing STUN class.
    /// </summary>
    /// <remarks>
    /// Total size:20 bytes<br/>
    /// 0(2  bytes): - zeros + MEssage type <br/>
    /// 2(2  bytes): - Message length <br/>
    /// 4(4  bytes): - Magic Cookie <br/>
    /// 8(12 bytes): - Transaction ID<br/> 
    /// </remarks>
    public class StunHeader
    {
        private Random random = new Random(100);
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
        /// </summary>
        public const int SIZE = 20;
        public const int INDEX_TRANSANCTION_ID_BEGIN = 8;
        /// <summary>
        /// Original in documentation: 0x2112A442.
        /// Here initizlied with 0x42A41221 because of host reverse order (int is bigendian)
        /// </summary>
        public const int MAGIC_COOKIE_FOR_CLIENT = 0x42A41221;
        private const byte MAGIC_COOKIE_BYTE_0 = 0x42;
        private const byte MAGIC_COOKIE_BYTE_1 = 0xA4;
        private const byte MAGIC_COOKIE_BYTE_2 = 0x12;
        private const byte MAGIC_COOKIE_BYTE_3 = 0x21;

        private static readonly byte[] headerPreset = new byte[] {
            0x00, 0x00,	/// zeros + MessageType <see cref="EStunMethod"/>
			0x00, 0x00, // MessageLength
			MAGIC_COOKIE_BYTE_3, MAGIC_COOKIE_BYTE_2, MAGIC_COOKIE_BYTE_1, MAGIC_COOKIE_BYTE_0, // Magic Cookie reverse (BigEndian)
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Transaction ID
		};

        public readonly Memory<byte> bufferHeader;
        public readonly Memory<byte> bufferTrunsunctionId;

        private readonly byte[] buffer;

        private bool isAwaitTransanctionId;

        public int MsgLength => GetMsgLength();
        public DataStunHeader Data => MemoryMarshal.Cast<byte, DataStunHeader>(bufferHeader.Span)[0];
        public TransactionID TransactionId => MemoryMarshal.Cast<byte, TransactionID>(bufferTrunsunctionId.Span)[0];

        public StunHeader(byte[] buffer)
        {
            this.buffer = buffer;
            bufferHeader = new Memory<byte>(buffer, 0, 20);
            Memory<byte> source = new Memory<byte>(headerPreset, 0, 20);
            source.CopyTo(bufferHeader);
            bufferTrunsunctionId = new Memory<byte>(buffer, 8, TransactionID.SIZE);
        }
        public void NewMessage()
        {
            SetTransactionIdRandomly();
        }

        #region Buffer per bit formating
        public void SetAwaitedTransanctionId(StunHeader headerForSending)
        {
            headerForSending.bufferTrunsunctionId.CopyTo(bufferTrunsunctionId);
            isAwaitTransanctionId = true;
        }

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
        /// </summary>
        protected void SetStunMessageType(ushort value)
        {
            unsafe
            {
                var span = bufferHeader.Span;
                byte* ptr = (byte*)&value;
                span[0] = ptr[1];
                span[1] = ptr[0];
            }
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
        /// </summary>
        /// <param name="length"></param>
        public void SetMessageLength(int length)
        {
            unsafe
            {
                var span = bufferHeader.Span;
                byte* ptr = (byte*)&length;
                /// <see cref="DataStunHeader.Length"/>
                span[2] = ptr[1];
                span[3] = ptr[0];
            }
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
        /// </summary>
        /// <param name="cookie"></param>
        private void SetMagicCookie(int cookie)
        {
            unsafe
            {
                var span = bufferHeader.Span;
                byte* bytePtr = (byte*)&cookie;

                for (int i = 4; i < 8; i++)
                {
                    span[i] = *bytePtr;
                    bytePtr++;
                }
            }
        }
        /// <summary>
        /// <see cref="MASK_STUN_MESSAGE_TYPE_CLASS_BITS"/>
        /// </summary>
        public void SetMessageClassToRequest()
        {
            var span = bufferHeader.Span;
            span[0] = (byte)(span[0] & 0b_0011_1110);
            span[1] = (byte)(span[1] & 0b_1110_1111);
        }
        /// <summary>
        /// <see cref="EStunClass.Indication"/>
        /// <see cref="CLASS_INDICATION"/>
        /// </summary>
        public void SetMessageClassToIndication()
        {
            var span = bufferHeader.Span;
            span[0] = (byte)(span[0] & 0b_0011_1110);
            span[1] = (byte)(span[1] | 0b_0001_0000);
        }
        public void SetHeaderMethod(int method)
        {
            DataStunHeader.SetMethod(bufferHeader.Span, (ushort)method);
        }
        /// <summary>
        /// Not supported by coturn server. Do not send to coturn server
        /// </summary>
        public void SetHeaderMethodToNone()
        {
            var span = bufferHeader.Span;
            span[0] = (byte)(span[0] & 0b_0000_0001);
            span[1] = (byte)(span[1] & 0b_0001_0000);
        }
        /// <summary>
        /// <see cref="ConstantsForStun.METHOD_BINDING"/>
        /// </summary>
        public void SetHeaderMethodToBinding()
        {
            var span = bufferHeader.Span;
            span[0] = (byte)(span[0] & 0b_0000_0001);
            span[1] = (byte)(span[1] & 0b_0001_0000);
            span[1] = (byte)(span[1] | 0b_0000_0001);
        }
        /// <summary>
        /// TURN extended
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-6.1
        /// </summary>
        public void SetHeaderMethodToAllocate()
        {
            var span = bufferHeader.Span;
            span[0] = (byte)(span[0] & 0b_0000_0001);
            span[1] = (byte)(span[1] & 0b_0001_0000);
            span[1] = (byte)(span[1] | 0b_0000_0011);
        }
        private void SetTransactionIdRandomly()
        {
            random.NextBytes(bufferTrunsunctionId.Span);
            ConsoleWrap.Magenta(Data.ToStringInfo());
        }
        internal void SetTransactionIdFromUnixDateTime()
        {
            unsafe
            {
                var span = bufferHeader.Span;
                long unitTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                byte* ptr = (byte*)&unitTime;

                for (int i = 8; i < 20; i++)
                {
                    span[i] = *ptr;
                    ptr++;
                }
            }
        }
        #endregion
        /// <summary>
        /// optional check of STUN protocol format. Ensure that message is not from another protocol.<br/>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.3
        /// </summary>
        /// <param name="bufferResponse"></param>
        /// <returns></returns>
        public bool IsValidLeadingBytes(byte[] bufferResponse)
        {
            return bufferResponse[0] == default && bufferResponse[1] == default;
        }
        /// <summary>
        /// ..the agent checks that the transaction ID matches a transaction that is still in progress..
        /// Check <see cref="TransactionID"/> match<br/>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.3
        /// </summary>
        /// <param name="bufferResponse"></param>
        /// <returns></returns>
        public bool IsTransactionIdOfCurrentProcess(byte[] bufferResponse)
        {
            var span = bufferHeader.Span;

            for (int i = StunHeader.INDEX_TRANSANCTION_ID_BEGIN; i < StunHeader.SIZE; i++)
            {
                if (span[i] != bufferResponse[i]) return false;
            }
            return true;
        }
        /// <summary>
        /// optional check of STUN protocol format. Ensure that message is not from another protocol.<br/>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.3
        /// </summary>
        /// <param name="bufferResponse">20 bytes STUN Header as byte array</param>
        /// <returns></returns>
        public bool IsMatchMagicCookie(byte[] bufferResponse)
        {
            return bufferResponse[4] == MAGIC_COOKIE_BYTE_3 &&
                    bufferResponse[5] == MAGIC_COOKIE_BYTE_2 &&
                    bufferResponse[6] == MAGIC_COOKIE_BYTE_1 &&
                    bufferResponse[7] == MAGIC_COOKIE_BYTE_0;
        }




        #region Getters
        public int GetMsgLength()
        {
            return BinaryConverter.ToInt32(bufferHeader.Span[3], bufferHeader.Span[2]);
        }
        #endregion

        #region Converters


        #endregion

        #region ToString
        public string ToStringTransactionIdAsHex()
        {
            return BitConverter.ToString(bufferTrunsunctionId.ToArray());
        }
        #endregion

        #region Static
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.4
        /// </summary>	
        public static byte[] GenerateHMACSHA1(string key, byte[] message)
        {
            var md5 = MD5.Create();
            var hashkey = md5.ComputeHash(Encoding.Default.GetBytes(key));
            byte[] hash;
            using (HMACSHA1 sha1 = new HMACSHA1(hashkey))
            {
                hash = sha1.ComputeHash(message);
            }
            return hash;
        }

        public static byte[] GenerateHMACSHA1(byte[] key, byte[] message, int offset, int count)
        {
            using (HMACSHA1 sha1 = new HMACSHA1(key))
            {
                return sha1.ComputeHash(message, offset, count);
            }
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.4
        /// https://datatracker.ietf.org/doc/html/rfc4013
        /// </summary>
        /// <param name="username"></param>
        /// <param name="realm"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] GenerateHashKey(string username, string realm, string password)
        {
            var md5 = MD5.Create();
            var hashkey = md5.ComputeHash(Encoding.Default.GetBytes($"{username}:{realm}:{password}"));
            return hashkey;
        }


        #endregion

        #region Subtype
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
            public const int MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INVERT_AS_INT = ~(int)MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            public const ushort MASK_MESSAGE_CLASS_BITS_BIG_ENDIAN = 0b_0001_0000_0000_0001;

            public const byte MASK_CLASS_INVERT_BYTE0 = 0b0011_1110;
            public const byte MASK_CLASS_INVERT_BYTE1 = 0b1110_1111;

            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_REQUEST = 0b_0000_0000_0000_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_INDICATION = 0b_0000_0000_0001_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_SUCCESS = 0b_0000_0001_0000_0000;
            public const ushort MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_ERROR = MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            // class bigendian
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS = 0b_0001_0000_0000_0001;
            public const int BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS_INVERT = ~(int)BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_REQUEST = 0b_0000_0000_0000_0000;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_INDICATION = 0b_0001_0000_0000_0000;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_SUCCESS = 0b_0000_0000_0000_0001;
            public const ushort BE_MASK_STUN_MESSAGE_TYPE_CLASS_RESPONSE_ERROR = BE_MASK_STUN_MESSAGE_TYPE_CLASS_BITS;

            /// <summary>
            /// по битам где стоит 1 образуется флаг определяющий STUN метод
            /// </summary>
            public const ushort MASK_STUN_MESSAGE_METHOD_BITS = 0b_1111_0001_1110_1111;
            public const ushort BE_MASK_STUN_MESSAGE_METHOD_BITS = 0b_1110_1111_1111_0001;
            public const int BE_MASK_STUN_MESSAGE_METHOD_BITS_INVERTED = ~(int)BE_MASK_STUN_MESSAGE_METHOD_BITS;

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
                return ((Value >> 7) & 0b0010) | ((Value >> 4) & 0b0001);
            }
            public int GetStunMethodValue()
            {
                return ((Value >> 2) & 0b1111_1000_0000) | ((Value >> 1) & 0b0111_0000) | (Value & 0b1111);
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

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-6
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 12, CharSet = CharSet.Ansi)]
        public struct TransactionID
        {
            public const int SIZE = 12;

            [FieldOffset(0)] public int value0;
            [FieldOffset(4)] public int value1;
            [FieldOffset(8)] public int value2;

            public string Info => ToStringInfo();

            public static bool operator ==(TransactionID a, TransactionID b) => a.value0 == b.value0 && a.value1 == b.value1 && a.value2 == b.value2;
            public static bool operator !=(TransactionID a, TransactionID b) => a.value0 != b.value0 && a.value1 != b.value1 && a.value2 != b.value2;

            public string ToStringInfo()
            {
                return $"{Convert.ToHexString(BitConverter.GetBytes(value0))}{Convert.ToHexString(BitConverter.GetBytes(value1))}{Convert.ToHexString(BitConverter.GetBytes(value2))}";
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 20, CharSet = CharSet.Ansi)]
        public struct DataStunHeader
        {
            [FieldOffset(0)] public MessageType messageType;
            [FieldOffset(2)] private ushort length;
            /// <summary>
            /// <see cref="StunHeader.MAGIC_COOKIE_FOR_CLIENT"/>
            /// </summary>
            [FieldOffset(4)] public uint magicCookie;
            [FieldOffset(8)] public TransactionID transactionId;

            public ushort Length => BinaryPrimitives.ReverseEndianness(length);

            public static DataStunHeader FromBuffer(byte[] rawHeader)
            {
                return MemoryMarshal.Cast<byte, DataStunHeader>(new Span<byte>(rawHeader))[0];
            }

            public static void WriteHeaderToBuffer(byte[] buffer, int offset)
            {
                throw new NotImplementedException();
            }

            public string ToStringInfo()
            {
                return
                    $"{nameof(DataStunHeader)} " +
                    $"Class: {messageType.GetStunClass()}    |   " +
                    $"Method: {messageType.GetStunMethodName()} |   " +
                    $"Length: {Length}  |   " +
                    $"MagicCookie: {BinaryPrimitives.ReverseEndianness(magicCookie).ToString("X2")} |   " +
                    $"TransactionId {transactionId.ToStringInfo()}";
            }

            public static void SetMethod(Span<byte> span, ushort method)
            {
                var spanCasted = MemoryMarshal.Cast<byte, ushort>(span);
                spanCasted[0] = (ushort)(((int)spanCasted[0] & MessageType.MASK_MESSAGE_CLASS_BITS_BIG_ENDIAN) | (int)method.ReverseByteOrder());
            }
        }
        #endregion
    }
}
