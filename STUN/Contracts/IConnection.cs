using System;

namespace IziHardGames.STUN.Contracts
{
    public interface IConnection : IDisposable
    {
        public void Send(ReadOnlySpan<byte> data);
        public Span<byte> Recieve();
    }
}
