using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.STUN
{
    public class StunClient : ISTUNClient, IDisposable
    {
        public string name;
        public readonly Logger logger;
        protected CancellationTokenSource cts;

        public StunClient()
        {
            logger = new Logger(this);
        }
        protected virtual void InitilizeConnection(ConnectionForStun con)
        {

        }

        public virtual async Task RunAsync(StunConnectionConfig config)
        {
            await Task.CompletedTask;
        }

        public virtual void Dispose()
        {

        }
    }
}
