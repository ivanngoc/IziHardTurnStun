using System;
using System.Net;
using System.Net.Sockets;
using IziHardGames.STUN.Contracts;
using IziHardGames.STUN.Domain.Headers;
using static IziHardGames.STUN.Domain.Headers.StunHeader;

namespace IziHardGames.STUN
{
    public class ConnectionUdp : IConnection
    {
        private object lockForSend = new object();
        private object lockForRead = new object();
        public readonly UdpClient udpClient;
        private IPEndPoint endpoint;
        private readonly IPAddress address;
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.2.1
        /// </summary>
        public int RetransmissionTimeOut;
        public ConnectionUdp(string host, int port, int portListen)
        {
            if (host == "localhost")
            {
                host = "127.0.0.1";
            }
            address = IPAddress.Parse(host);
            endpoint = new IPEndPoint(address, port);
            udpClient = new UdpClient(portListen);
            udpClient.Connect(endpoint);
        }
        public void Dispose()
        {
            udpClient.Dispose();
        }
        public void Send(ReadOnlySpan<byte> data)
        {
            var header = data.ToStruct<DataStunHeader>();
            Logger.LogTrace();

            //lock (lockForSend)
            {
                udpClient.Send(data);
            }
        }
        public Span<byte> Recieve()
        {
            IPEndPoint localEndpoint = null;

            //lock (lockForRead)
            {
                byte[] data = udpClient.Receive(ref localEndpoint);
                return new Span<byte>(data);
            }
        }
    }
}
