using System;

namespace IziHardGames.STUN
{
    public interface IConnection : IDisposable
    {
        public void Send(ReadOnlySpan<byte> data);
        public Span<byte> Recieve();
    }
}
