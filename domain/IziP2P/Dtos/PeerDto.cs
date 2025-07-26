using System;

namespace IziHardGames.IziP2P.Dtos
{
    public class PeerDto
    {
        public int IPv4 { get; set; }
        public long Timestamp { get; set; }
        public string? RoutingKey { get; set; }
    }
}
