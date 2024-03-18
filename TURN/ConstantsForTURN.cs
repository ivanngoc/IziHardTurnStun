using IziHardGames.Networking.IANA;
using IziHardGames.STUN;
using System;
using System.Net;
using System.Threading;
using static IziHardGames.STUN.StunHeader;

namespace IziHardGames.TURN
{
    public static class TurnInit
    {
        static TurnInit()
        {
            var dic = AttributeForStun.stunAttributeByValue;
            //TURN
            dic.Add(ConstantsForTurn.ATTR_Channel_Number, nameof(ConstantsForTurn.ATTR_Channel_Number));
            dic.Add(ConstantsForTurn.ATTR_Lifetime, nameof(ConstantsForTurn.ATTR_Lifetime));
            dic.Add(ConstantsForTurn.ATTR_XOR_Peer_Address, nameof(ConstantsForTurn.ATTR_XOR_Peer_Address));
            dic.Add(ConstantsForTurn.ATTR_Data, nameof(ConstantsForTurn.ATTR_Data));
            dic.Add(ConstantsForTurn.ATTR_XOR_Relayed_Address, nameof(ConstantsForTurn.ATTR_XOR_Relayed_Address));
            dic.Add(ConstantsForTurn.ATTR_Even_Port, nameof(ConstantsForTurn.ATTR_Even_Port));
            dic.Add(ConstantsForTurn.ATTR_Requested_Transport, nameof(ConstantsForTurn.ATTR_Requested_Transport));
            dic.Add(ConstantsForTurn.ATTR_Dont_Fragment, nameof(ConstantsForTurn.ATTR_Dont_Fragment));
            dic.Add(ConstantsForTurn.ATTR_Reservation_Token, nameof(ConstantsForTurn.ATTR_Reservation_Token));


            var methods = MessageType.methodsByValueLittleEndian;

            // TURN
            methods.Add(ConstantsForTurn.METHOD_ALLOCATE, nameof(ETurnMethod.Allocate));
            methods.Add(ConstantsForTurn.METHOD_REFRASH, nameof(ETurnMethod.Refresh));
            methods.Add(ConstantsForTurn.METHOD_SEND, nameof(ETurnMethod.Send));
            methods.Add(ConstantsForTurn.METHOD_DATA, nameof(ETurnMethod.Data));
            methods.Add(ConstantsForTurn.METHOD_CREATE_PERMISSION, nameof(ETurnMethod.CreatePermission));
            methods.Add(ConstantsForTurn.METHOD_CHANNEL_BIND, nameof(ETurnMethod.ChannelBind));

            var errors = StunErrorCode.errorCodesByValue;
            // TURN
            errors.Add(ConstantsForTurn.ERROR_FORBIDDEN, nameof(ConstantsForTurn.ERROR_FORBIDDEN));
            errors.Add(ConstantsForTurn.ERROR_ALLOCATION_MISMATCH, nameof(ConstantsForTurn.ERROR_ALLOCATION_MISMATCH));
            errors.Add(ConstantsForTurn.ERROR_WRONG_CREDENTIALS, nameof(ConstantsForTurn.ERROR_WRONG_CREDENTIALS));
            errors.Add(ConstantsForTurn.ERROR_UNSUPPORTED_TRANSPORT_PROTOCOL, nameof(ConstantsForTurn.ERROR_UNSUPPORTED_TRANSPORT_PROTOCOL));
            errors.Add(ConstantsForTurn.ERROR_ALLOCATION_QUOTA_REACHED, nameof(ConstantsForTurn.ERROR_ALLOCATION_QUOTA_REACHED));
            errors.Add(ConstantsForTurn.ERROR_INSUFFICIENT_CAPACITY, nameof(ConstantsForTurn.ERROR_INSUFFICIENT_CAPACITY));


            errors.Add(ConstantsForTurn.ERROR_CONNECTION_ALREADY_EXISTS, nameof(ConstantsForTurn.ERROR_CONNECTION_ALREADY_EXISTS));
            errors.Add(ConstantsForTurn.ERROR_CONNECTION_TIMEOUT_OR_FAILURE, nameof(ConstantsForTurn.ERROR_CONNECTION_TIMEOUT_OR_FAILURE));
        }

        public static void Initilize()
        {

        }
    }
    public class ConstantsForTurn
    {
        #region Extended By TURN
        // Little endian
        // All extensions described here: https://datatracker.ietf.org/doc/html/rfc5766#section-6.1
        
        //public const UInt16 Channel_Number = 0x000C;      
        //public const UInt16 Lifetime = 0x000D;
        //public const UInt16 XOR_Peer_Address = 0x0012;
        //public const UInt16 Data = 0x0013;
        //public const UInt16 XOR_Relayed_Address = 0x0016;
        //public const UInt16 Even_Port = 0x0018;
        //public const UInt16 Requested_Transport = 0x0019;
        //public const UInt16 Dont_Fragment = 0x001A;
        //public const UInt16 Reservation_Token = 0x0022;

        // little endian
        public const int ATTR_Channel_Number = 12;           // 0x000C;
        public const int ATTR_Lifetime = 13;                 // 0x000D; https://datatracker.ietf.org/doc/html/rfc5766#section-14.2
        public const int ATTR_XOR_Peer_Address = 18;         // 0x0012;
        public const int ATTR_Data = 19;                     // 0x0013;
        public const int ATTR_XOR_Relayed_Address = 22;      // 0x0016; https://datatracker.ietf.org/doc/html/rfc5766#section-14.5  The XOR-RELAYED-ADDRESS is present in Allocate responses.  It specifies the address and port that the server allocated to the client.
        public const int ATTR_Even_Port = 24;                // 0x0018;
        public const int ATTR_Requested_Transport = 25;      // 0x0019; https://datatracker.ietf.org/doc/html/rfc5766#section-14.7 This attribute is used by the client to request a specific transport protocol for the allocated transport address.
        public const int ATTR_Dont_Fragment = 26;            // 0x001A;
        public const int ATTR_Reservation_Token = 34;        // 0x0022;

        //#### https://datatracker.ietf.org/doc/html/rfc6062
        /// <summary>
        /// The CONNECTION-ID attribute uniquely identifies a peer data connection.It is a 32-bit unsigned integral value.<br/>
        /// https://datatracker.ietf.org/doc/html/rfc6062#section-6.2.1
        /// </summary>
        public const int ATTR_CONNECTION_ID = 42;           // 0x002a;


        // big endian
        public const int BE_ATTR_Channel_Number = 3072;           // 0x000C; => 0x0C00
        public const int BE_ATTR_Lifetime = 3328;                 // 0x000D; => 0x0D00
        public const int BE_ATTR_XOR_Peer_Address = 4608;         // 0x0012; => 0x1200
        public const int BE_ATTR_Data = 4864;                     // 0x0013; => 0x1300
        public const int BE_ATTR_XOR_Relayed_Address = 5632;      // 0x0016; => 0x1600
        public const int BE_ATTR_Even_Port = 6144;                // 0x0018; => 0x1800
        public const int BE_ATTR_Requested_Transport = 6400;      // 0x0019; => 0x1900
        public const int BE_ATTR_Dont_Fragment = 6656;            // 0x001A; => 0x1A00
        public const int BE_ATTR_Reservation_Token = 8704;        // 0x0022; => 0x2200



        #endregion

        // Stun extended for TURN https://datatracker.ietf.org/doc/html/rfc5766#section-15
        public const int ERROR_FORBIDDEN = 0x0304;                        // 403;
        public const int ERROR_ALLOCATION_MISMATCH = 0x3704;              // 437;
        public const int ERROR_WRONG_CREDENTIALS = 0x4104;                // 441;
        public const int ERROR_UNSUPPORTED_TRANSPORT_PROTOCOL = 0x4204;   // 442
        public const int ERROR_ALLOCATION_QUOTA_REACHED = 0x8604;         // 486;
        public const int ERROR_INSUFFICIENT_CAPACITY = 0x0805;            // 508;

        // Stun extended for TURN https://datatracker.ietf.org/doc/html/rfc6062#section-6.3
        public const int ERROR_CONNECTION_ALREADY_EXISTS = 0x4604;                        // 446;
        public const int ERROR_CONNECTION_TIMEOUT_OR_FAILURE = 0x4704;                    // 447;


        // METHODS TURN Extension
        /// <summary>
        /// The client uses TURN commands to create and manipulate an ALLOCATION
        /// on the server.  An allocation is a data structure on the server.
        /// This data structure contains, amongst other things, the RELAYED
        /// TRANSPORT ADDRESS for the allocation.  The relayed transport address
        /// is the transport address on the server that peers can use to have the
        /// server relay data to the client.  An allocation is uniquely
        /// identified by its relayed transport address.<br/>
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-13 <br/>
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-2.2 <br/>
        /// </summary>
        public const ushort METHOD_ALLOCATE = 0b_0000_0000_0000_0011;                  //0x003
        public const ushort METHOD_REFRASH = 0b_0000_0000_0000_0100;                   //0x004
        public const ushort METHOD_SEND = 0b_0000_0000_0000_0110;                      //0x006
        public const ushort METHOD_DATA = 0b_0000_0000_0000_0111;                      //0x007
        public const ushort METHOD_CREATE_PERMISSION = 0b_0000_0000_0000_1000;          //0x008
        public const ushort METHOD_CHANNEL_BIND = 0b_0000_0000_0000_1001;               //0x009

        //https://datatracker.ietf.org/doc/html/rfc6062#section-6.1
        public const ushort METHOD_CONNECT = 0x000a;
        public const ushort METHOD_CONNECTION_BIND = 0x000b;
        public const ushort METHOD_CONNECTION_ATTEMPT = 0x000c;



        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.1
        /// </summary>
        public int mtuIpv4 = 576;
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.1
        /// </summary>
        public int mtuIpv6 = 1280;
    }
}
