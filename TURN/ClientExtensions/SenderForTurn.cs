using IziHardGames.Networking.IANA;
using IziHardGames.STUN;
using IziHardGames.STUN.Attributes;
using System;
using System.Net;
using System.Reflection;

namespace IziHardGames.TURN
{
    public class SenderForTurn : SenderForStun
    {
        private static object locker = new object();
        private ConnectionForTurn connectionForTurn;
        public SenderForTurn(ConnectionForTurn connection) : base(connection)
        {
            this.connectionForTurn = connection;
        }

        public int PutAttributeOfAddress(ushort type, StunAddress peerAddress, AttributeDataForIPv4 data)
        {
            var offset = length;
            ushort lengthData = AttributeDataForIPv4.SIZE_WHOLE;
            EnsureBufferSize(offset, lengthData);
            this.length = AttributeForStun.PutToBuffer(bufferSend, offset, type, peerAddress, data);

            return StunAddress.SIZE;
        }
        public int PutAttributeOfAddress(ushort type, IPAddress iPAddress, int port)
        {
            PutAttributeOfAddress(type, StunAddress.Xor(iPAddress, port), AttributeDataForIPv4.Xor(iPAddress));
            return StunAddress.SIZE;
        }

        public void DoMethodSend(ConnectionForTurn con, TurnPeer peer, string message)
        {
            lock (locker)
            {
                Logger.LogTrace();

                var header = this.headerForSender;

                AttributesClear();
                header.SetMessageClassToIndication();
                header.SetHeaderMethod(ConstantsForTurn.METHOD_SEND);

                //PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressMapped, peer.addressMappedDataV4);
                //PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressMappedXor, peer.addressMappedXorDataV4);
                PutAttributeOfAddress(ConstantsForTurn.ATTR_XOR_Peer_Address, peer.addressRelayedXor, peer.addressRelayedXorDataV4);

                PutAttribute(length, ConstantsForTurn.ATTR_Data, message.ToByteSpan());

                header.SetMessageLength(length - StunHeader.SIZE);

                header.NewMessage();
                Send();
            }
        }
    }
}
