using IziHardGames.STUN.Domain.Headers;
using IziHardGames.TURN;
using System;
using System.Net;
using System.Xml.Linq;

namespace IziHardGames.Turn.Hub
{
    public class HubForTurnClients
    {
        private List<ClientInfo> clients = new List<ClientInfo>();

        public bool TryFindClientWithName(string name, out ClientInfo? result)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client.name == name)
                    {
                        result = client;
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }
        public ClientInfo FindClientWithName(string name)
        {
            foreach (var client in clients)
            {
                if (client.name == name) return client;
            }
            throw new ArgumentOutOfRangeException($"No CLient with name {name}");
        }

        public void Regist(ConnectionForTurn con)
        {
            var reader = con.StunMessageReader;

            if (con.IsAllocated)
            {
                lock (clients)
                {
                    Logger.LogTrace();

                    clients.Add(new ClientInfo()
                    {
                        name = con.TurnClient.name,

                        adressMapped = con.addressMappedOnBind,
                        adressMappedIp = con.addressMappedOnBindIp,

                        adressMappedXor = con.addressMappedXorOnBind,
                        adressMappedXorIp = con.addressMappedXorOnBindIp,

                        addressRelayedXor = con.addressRelayedXor,
                        addressRelayedXorIp = con.addressRelayedXorIp,
                    });
                }
            }
            else
            {
                throw new ArgumentException($"Connection is not properly initilized");
            }
        }
        public void Regist(string name, StunAddress addressMapped, StunAddress addressRelayedXor)
        {

        }
    }

    public class ClientInfo
    {
        public string? name;

        public StunAddress adressMapped;
        public IPAddress? adressMappedIp;

        public StunAddress adressMappedXor;
        public IPAddress? adressMappedXorIp;

        public StunAddress addressRelayedXor;
        public IPAddress? addressRelayedXorIp;
    }
}
