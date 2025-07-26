using System;
using System.Net.Sockets;
using IziHardGames.STUN.Contracts;

namespace IziHardGames.STUN
{
    public class ConnectionTcp : IConnection
    {
        public readonly TcpClient tcpClient;
        private string host;
        private int port;

        public ConnectionTcp(string host, int port)
        {
            tcpClient = new TcpClient(host, port);
            this.host = host;
            this.port = port;
        }

        public void Dispose()
        {
            tcpClient.Dispose();
        }

        public NetworkStream GetStream()
        {
            return tcpClient.GetStream();
        }

        public void Send(ReadOnlySpan<byte> data)
        {
            lock (tcpClient)
            {
                tcpClient.GetStream().Write(data);
            }
        }

        public Span<byte> Recieve()
        {
            throw new NotImplementedException();
        }
    }
}
