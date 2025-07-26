using IziHardGames.STUN.Attributes;
using IziHardGames.STUN.Domain.Headers;
using System;
using System.Net;

namespace IziHardGames.TURN
{
    public class TurnPeer
    {
        public string name;
        public bool isPermissionCreated;
        public int permissionLifetime;

        public StunAddress PeerAddressXor { get => addressMappedXor; }
        public IPAddress PeerAddressXorIp { get => addressMappedXorIp; }
        public string SendingHost { get => PeerAddressXorIp.ToString(); }
        public string SendingPort { get => PeerAddressXor.PortXor.ToString(); }

        public StunAddress addressMapped;
        public IPAddress addressMappedIp;
        public AttributeDataForIPv4 addressMappedDataV4;
        public AttributeDataForIPv6 addressMappedDataV6;

        public StunAddress addressMappedXor;
        public IPAddress addressMappedXorIp;
        public AttributeDataForIPv4 addressMappedXorDataV4;
        public AttributeDataForIPv6 addressMappedXorDataV6;

        public StunAddress addressRelayedXor;
        public IPAddress addressRelayedXorIp;
        public AttributeDataForIPv4 addressRelayedXorDataV4;
        public AttributeDataForIPv6 addressRelayedXorDataV6;
    }
}
