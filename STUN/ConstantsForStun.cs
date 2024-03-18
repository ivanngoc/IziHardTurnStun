using static IziHardGames.STUN.AttributeForStun;

namespace IziHardGames.STUN
{
    public static class ConstantsForStun
    {
        public const byte IPv4 = 0x01;
        public const byte IPv6 = 0x02;

        public const ushort CLASS_REQUEST = 0b00;
        public const ushort CLASS_INDICATION = 0b01;
        public const ushort CLASS_SUCCESS_RESPONSE = 0b10;
        public const ushort CLASS_ERROR_RESPONSE = 0b11;

        // METHODS STUN Original
        /// little endian
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-18.1
        public const ushort METHOD_RESERVED = 0b_0000_0000_0000_0000;                  //0x000
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-3
        /// the client can learn its reflexive transport address allocated by the outermost NAT with respect to the STUN server.
        /// </summary>
        public const ushort METHOD_BINDING = 0b_0000_0000_0000_0001;                    //0x001
        public const ushort METHOD_RESERVER_WAS_SHARED_SECRET = 0b_0000_0000_0000_0010; //0x002


        #region Attributes
        //https://datatracker.ietf.org/doc/html/rfc5389#section-18.2
        public const int ATTR_Mapped_Address = 1;                   // 0x0001;  The MAPPED-ADDRESS attribute indicates a reflexive transport address of the client. https://datatracker.ietf.org/doc/html/rfc5389#section-15.1
        public const int ATTR_XOR_Mapped_Address = 32;              // 0x0020;  The XOR-MAPPED-ADDRESS attribute is identical to the MAPPED-ADDRESS attribute, except that the reflexive transport address is obfuscated through the XOR function.<br/>  https://datatracker.ietf.org/doc/html/rfc5389#section-15.2
        public const int ATTR_Username = 6;                         // 0x0006;
        public const int ATTR_MessageIntegrity = 8;                 // 0x0008;
        public const int ATTR_Fingerprint = 32808;                  // 0x8028;
        public const int ATTR_Error_Code = 9;                       // 0x0009;
        public const int ATTR_Realm = 20;                           // 0x0014;
        public const int ATTR_Nonce = 21;                           // 0x0015;
        public const int ATTR_Unknown_Attributes = 10;              // 0x000A;
        public const int ATTR_Software = 32802;                     // 0x8022;
        public const int ATTR_Alternate_Server = 14;                // 0x000E;

        #region Extended with ICE
        // https://datatracker.ietf.org/doc/html/rfc5245#section-21.2

        public const int ATTR_PRIORITY = 0x0024;//0x0024;
        public const int ATTR_USE_CANDIDATE = 0x0025;
        public const int ATTR_ICE_CONTROLLED = 0x8029;
        public const int ATTR_ICE_CONTROLLING = 0x802A;
        #endregion

        #region Extended By NAT
        // https://www.rfc-editor.org/rfc/rfc5780.html#section-7
        // https://www.rfc-editor.org/rfc/rfc5780.html#section-9.1
        public const int ATTR_CHANGE_REQUEST = 0x0003; //0x0003
        public const int ATTR_PADDING = 0x0026;
        public const int ATTR_RESPONSE_PORT = 0x0027;
        public const int ATTR_CACHE_TIMEOUT = 0x8027;

        /// <summary>
        /// The RESPONSE-ORIGIN attribute is inserted by the server and indicates
        /// the source IP address and port the response was sent from.It is
        /// useful for detecting double NAT configurations.It is only present
        /// in Binding Responses.<br/>
        /// https://www.rfc-editor.org/rfc/rfc5780.html#section-7.3
        /// </summary>
        public const int RESPONSE_ORIGIN = 0x802b;
        public const int OTHER_ADDRESS = 0x802c;
        #endregion 
        #endregion
    }
}
