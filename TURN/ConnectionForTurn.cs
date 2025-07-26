using IziHardGames.Networking.IANA;
using IziHardGames.STUN;
using IziHardGames.STUN.Attributes;
using IziHardGames.STUN.Domain.Headers;
using IziHardGames.STUN.STUN;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace IziHardGames.TURN
{
    public class ConnectionForTurn : ConnectionForStun
    {
        protected ReaderForTurn readerForTurn;
        protected SenderForTurn senderForTurn;

        public SenderForTurn SenderForTurn => senderForTurn;
        public ReaderForTurn ReaderForTurn => readerForTurn;

        public bool isAddressRelayedXorRecived;
        private bool isTransportRequested;
        /// <summary>
        /// Recived external adress how server is identify host.
        /// </summary>
        private bool isReflected;

        /// <summary>
        /// <see cref="ConstantsForTurn.ATTR_Lifetime"/>
        /// </summary>
        public uint Lifetime;

        public TurnRequestedTransport turnRequestedTransport;

        // https://datatracker.ietf.org/doc/html/rfc5766#section-14.5
        // https://datatracker.ietf.org/doc/html/rfc5766#section-5
        public StunAddress addressRelayedXor;
        public AttributeDataForIPv4 addressRelayedXorData;
        public IPAddress addressRelayedXorIp;

        public bool IsAllocated { get; set; }
        public bool IsRelayedAddressRecieved => isAddressRelayedXorRecived;
        public bool IsTransportRecieved => isTransportRequested;

        private TurnClient turnClient;
        public TurnClient TurnClient => turnClient;

        public event Action OnPermissionSuccessEvent;

        public ConnectionForTurn(TurnClient turnClient, StunConnectionConfig config, ILogger logger) : base(turnClient, config, logger)
        {
            this.turnClient = turnClient;

            this.readerForTurn = new ReaderForTurn(this);
            this.stunMessageReader = readerForTurn;

            this.senderForTurn = new SenderForTurn(this);
            this.stunMessageSender = senderForTurn;
        }

        public override void Initilize()
        {
            base.Initilize();

            switch (config.AuthType)
            {
                case EAuthType.None: throw new ArgumentOutOfRangeException();
                case EAuthType.NoAuth:
                    {
                        DoMethodAllocate();
                        break;
                    }
                case EAuthType.LongTermCred:
                    {
                        DoMethodAllocate();
                        AuthenticateWithLongTermCredentialMechanism(null);
                        break;
                    }
                case EAuthType.ShortTermSecret:
                    {
                        throw new NotSupportedException("coturn is not supported shortm term auth");
                    }
                default: break;
            }
        }

        internal void SetTransport(TurnRequestedTransport value)
        {
            isTransportRequested = true;
            this.turnRequestedTransport = value;
        }

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-2.2 - overview
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-6 - Creating an Allocation (UDP onlyy)
        /// https://datatracker.ietf.org/doc/html/rfc6062#section-4.1 - Creating an Allocation (TCP only)
        /// </summary>
        /// Response:
        /// <see cref="ConstantsForTurn.ATTR_XOR_Relayed_Address"/>
        /// <see cref="ConstantsForTurn.ATTR_Lifetime"/>
        /// <see cref="ConstantsForStun.ATTR_XOR_Mapped_Address"/>
        /// <see cref="ConstantsForStun.ATTR_Software"/>
        private void DoMethodAllocate()
        {
            var sender = stunMessageSender;
            var header = sender.headerForSender;
            var reader = stunMessageReader;

            //if (Protocol == TCP)
            {
                header.SetHeaderMethodToAllocate();
                header.SetMessageClassToRequest();

                byte protocol = 0;

                if (Protocol == TCP) protocol = ProtocolNumber.TCP;
                if (Protocol == UDP) protocol = ProtocolNumber.UDP;

                /// The client must always include a REQUESTED-TRANSPORT attribute in an Allocate request 
                /// https://datatracker.ietf.org/doc/html/rfc5766#section-16 - UDP
                /// https://datatracker.ietf.org/doc/html/rfc6062#section-4.1 - TCP
                sender.PutAttribute(sender.length, ConstantsForTurn.ATTR_Requested_Transport, new TurnRequestedTransport(protocol));
                sender.PutAttribute(sender.length, ConstantsForStun.ATTR_Realm, config.Realm.ToByteSpan());
                header.SetMessageLength(sender.length - StunHeader.SIZE);

                reader.QueueResponse(sender);
                header.NewMessage();
                stunMessageSender.Send();
            }

            //header.NewMessage();
            //header.SetMessageLength(sender.length - StunHeader.SIZE);
            //stunMessageSender.Send();

            //else if (Protocol == UDP)
            //{
            //    throw new NotImplementedException();
            //}
            //else
            //{
            //    throw new ArgumentOutOfRangeException($"Protocol:{Protocol}");
            //}
        }

        #region TURN - Allocation, Sending, Recieving, Creating/Receiving Permissions  ETC.

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5766#section-16 8 абзац
        /// </summary>
        public void DoMethodCreatePermissionToPeer(TurnPeer peer)
        {
            var header = senderForTurn.headerForSender;
            var sender = senderForTurn;

            header.SetMessageClassToRequest();
            header.SetHeaderMethod(ConstantsForTurn.METHOD_CREATE_PERMISSION);

            sender.AttributesClear();

            if (peer.addressMappedXorIp.AddressFamily == AddressFamily.InterNetwork)
            {
                //sender.PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressMapped, peer.addressMappedDataV4);
                //sender.PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressMappedXor, peer.addressMappedXorDataV4);
                sender.PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressRelayedXor, peer.addressRelayedXorDataV4);
            }
            else if (peer.addressMappedXorIp.AddressFamily == AddressFamily.InterNetworkV6)
            {
                throw new System.NotImplementedException();
            }

            header.SetMessageLength(sender.length - StunHeader.SIZE);
            senderForTurn.Send();
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc6062#section-4.3
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="relayPort"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ConnectTo(IPAddress adress, int relayPort)
        {
            var header = senderForTurn.headerForSender;
            var sender = senderForTurn;

            header.SetMessageClassToRequest();
            header.SetHeaderMethod(ConstantsForTurn.METHOD_CONNECT);

            sender.AttributesClear();
            sender.PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, adress, relayPort);
            header.SetMessageLength(sender.length - StunHeader.SIZE);

            senderForTurn.Send();
        }
        #endregion
        internal void ReportPermissionGranted()
        {
            OnPermissionSuccessEvent?.Invoke();
        }

        public TurnPeer CreatePeer()
        {
            return new TurnPeer()
            {
                name = stunClient.name,

                addressMapped = this.addressMappedOnBind,
                addressMappedIp = this.addressMappedOnBindIp,
                addressMappedDataV4 = this.addressMappedOnBindData,
                addressMappedDataV6 = default,


                addressMappedXor = this.addressMappedXorOnBind,
                addressMappedXorIp = this.addressMappedXorOnBindIp,
                addressMappedXorDataV4 = this.addressMappedXorOnBindData,
                addressMappedXorDataV6 = default,


                addressRelayedXor = this.addressRelayedXor,
                addressRelayedXorIp = this.addressRelayedXorIp,
                addressRelayedXorDataV4 = this.addressRelayedXorData,
                addressRelayedXorDataV6 = default,
            };
        }
    }
}
