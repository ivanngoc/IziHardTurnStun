using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TData = System.ReadOnlySpan<byte>;

namespace IziHardGames.STUN.Domain.Headers
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-15.6
    /// https://www.iana.org/assignments/stun-parameters/stun-parameters.xhtml
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4, CharSet = CharSet.Unicode)]
    public struct StunErrorCode
    {
        [FieldOffset(0)] public ushort reserved;
        /// <summary>
        /// 0x0004 = 0x0400 в BIG ENDIAN. это ошибка 400
        /// </summary>
        [FieldOffset(2)] public ushort comboClassAndNumber;

        public const ushort MASK_CLASS = 0b_0000_0000_1110_0000;
        public const ushort MASK_NUMBER = 0b_0000_0000_0001_1111;

        public const int BE_MASK_CLASS = 0b_1110_0000_0000_0000;
        public const int BE_MASK_NUMBER = 0b_0001_1111_0000_0000;

        public const int ERROR_NONE = 0;
        public const int ERROR_NOT_DEFINED = 0xFFFF;
        //Stun Origin Error Codes
        public const int BAD_REQUEST = 0x0004;              // 400;
        public const int UNAUTHORIZED = 0x0104;             // 401;
        public const int UNKNOWN_ATTRIBUTE = 0x2004;        // 420;
        public const int STALE_CREDENTIALS = 0x3004;        // 430;
        public const int INTEGRITY_CHECK_FAILURE = 0x3104;  // 431;
        public const int MISSING_USERNAME = 0x3204;         // 432;
        public const int USE_TLS = 0x3304;                  // 433;
        public const int SERVER_ERROR = 0x0005;             // 500;
        public const int GLOBAL_FAILURE = 0x0006;           // 600;



        //ICE https://datatracker.ietf.org/doc/html/rfc5245#section-21.3
        public const int ROLE_CONFLICT = 0x8704;    // 487;

        // unusigned but defined by coturn server https://www.iana.org/assignments/stun-parameters/stun-parameters.xhtml
        public const int TOO_EARLY = 0x2504;            // 425;

        public readonly static Dictionary<int, string> errorCodesByValue = new Dictionary<int, string>()
        {
            //Origin
            [ERROR_NONE] = nameof(ERROR_NONE),
            [BAD_REQUEST] = nameof(BAD_REQUEST),
            [UNAUTHORIZED] = nameof(UNAUTHORIZED),
            [UNKNOWN_ATTRIBUTE] = nameof(UNKNOWN_ATTRIBUTE),
            [STALE_CREDENTIALS] = nameof(STALE_CREDENTIALS),
            [INTEGRITY_CHECK_FAILURE] = nameof(INTEGRITY_CHECK_FAILURE),
            [MISSING_USERNAME] = nameof(MISSING_USERNAME),
            [USE_TLS] = nameof(USE_TLS),
            [SERVER_ERROR] = nameof(SERVER_ERROR),
            [GLOBAL_FAILURE] = nameof(GLOBAL_FAILURE),
            //ICE
            [ROLE_CONFLICT] = nameof(ROLE_CONFLICT),
            // UNASIGNED
            [TOO_EARLY] = nameof(TOO_EARLY),
            [ERROR_NOT_DEFINED] = nameof(ERROR_NOT_DEFINED),
        };

        public string GetCode()
        {
#if DEBUG
            if (!errorCodesByValue.TryGetValue(comboClassAndNumber, out string stunErrorCode))
            {
                return nameof(ERROR_NOT_DEFINED);
            }
            return stunErrorCode;
#else
			return errorCodesByValue[comboClassAndNumber];
#endif


        }

        public static StunErrorCode FromBuffer(TData memory)
        {
            return memory.ToStruct<StunErrorCode>();
        }
    }
}
