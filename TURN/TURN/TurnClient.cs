using IziHardGames.STUN.Attributes;
using IziHardGames.TURN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IziHardGames.STUN
{
    public class TurnClient : StunClient
    {
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.2.2
        /// page 15, last paragraph
        /// </summary>
        protected int DEFAULT_INITIAL_RTO_TIMEOUT_MILLISECONDS = 39500;
        public List<ConnectionForTurn> connections = new List<ConnectionForTurn>();
        public List<TurnPeer> peers = new List<TurnPeer>();


        #region ICE - Get Peers
        public void GetPeers()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            //ConsoleWrap.Green($"{DateTime.Now}	{method.DeclaringType.FullName.ToUpper()}.{method.Name.ToUpper()}()");
            Logger.LogTrace();
        }
        #endregion
        public void ConnectTo(TurnPeer peer)
        {
            // find client
            var con = connections.First();

            //con.ConnectTo(adress, relayPort);
            con.DoMethodCreatePermissionToPeer(peer);

            // connect
            // create permissions
            // messaging
        }
        /// <summary>
        /// incoming packet SEND processed, error 403: Send cannot be used with TCP relay
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public void SendToPeer(string name, string message)
        {
            var con = connections.First();
            TurnPeer peer = peers.FirstOrDefault(x => x.name == name);

            if (peer != null && con.Protocol == ConnectionForStun.UDP)
            {
                logger.Log($"Client:{name} SendTo:{name}. {message}. Host:{peer.SendingHost} Port:{peer.SendingPort}");
                con.SenderForTurn.DoMethodSend(con, peer, message);
            }
            else
            {
                throw new NotSupportedException($"error 403: Send cannot be used with TCP relay");
            }
        }

        public TurnPeer AddPeer(TurnPeer turnPeer)
        {
            peers.Add(turnPeer);
            return turnPeer;
        }
        public TurnPeer AddPeer(string name, StunAddress adressMapped, IPAddress adressMappedIp, StunAddress addressRelayedXor, IPAddress addressRelayedXorIp, StunAddress adressMappedXor, IPAddress adressMappedXorIp)
        {
            TurnPeer turnPeer = new TurnPeer()
            {
                name = name,
                addressMapped = adressMapped,
                addressMappedIp = adressMappedIp,

                addressMappedXor = adressMappedXor,
                addressMappedXorIp = adressMappedXorIp,

                addressRelayedXor = addressRelayedXor,
                addressRelayedXorIp = addressRelayedXorIp,
            };

            peers.Add(turnPeer);
            return turnPeer;
        }
    }
}
