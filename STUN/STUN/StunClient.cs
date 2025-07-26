using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IziHardGames.STUN.STUN
{
    public class StunClient : ISTUNClient, IDisposable
    {
        public string name;
        public readonly ILogger logger;
        protected CancellationTokenSource cts;

        public StunClient(ILogger logger)
        {
            this.logger = logger;
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
