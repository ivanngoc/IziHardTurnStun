using IziHardGames.STUN;
using IziHardGames.STUN.Attributes;
using IziHardGames.STUN.Domain.Headers;
using IziHardGames.STUN.STUN;
using IziHardGames.TURN;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IziHardGames
{
    /// <summary>
    /// Клиент TURN сервера с имплементацией STUN
    /// Server: Version Coturn-4.5.1.1 'dan Eider'
    /// </summary>
    public class CoturnClient : TurnClient, IDisposable
    {
        private StunConnectionConfig config;
        public int id;
        protected bool isConnected;
        public CoturnClient(string name, ILogger<CoturnClient> logger) : base(logger)
        {
            this.name = name;
        }
        public override async Task RunAsync(StunConnectionConfig config)
        {
            await base.RunAsync(config);

            var t1 = ConnectAsync(config);
            await t1.ConfigureAwait(false);
        }

        public async Task ConnectAsync(StunConnectionConfig config)
        {
            await Task.CompletedTask;
        }
        public void Connect(StunConnectionConfig config)
        {
            this.config = config;

            ConnectionForTurn connection = new ConnectionForTurn(this, config, logger);
            connection.EnsureReadedAndSender();
            connection.DoMethodBind();

            lock (connections)
            {
                connections.Add(connection);
            }
            isConnected = true;
        }
        protected virtual void InitilizeConnection(ConnectionForTurn con)
        {
            base.InitilizeConnection((ConnectionForStun)con);
            con.Initilize();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var con in connections)
            {
                con.Dispose();
            }
            connections.Clear();
        }


        #region Test
        public Task DevRun(StunConnectionConfig configDocker)
        {
            this.config = configDocker;

            var method = System.Reflection.MethodBase.GetCurrentMethod();
            //Console.WriteLine($"{DateTime.Now}	{method.DeclaringType.FullName}.{method.Name}()");
            Logger.LogTrace();

            Connect(configDocker);
            var con = connections.First();
            StunHeader header = con.StunMessageSender.headerForSender;
            var reader = con.StunMessageReader;

            var buffer = con.StunMessageSender.bufferSend;

            InitilizeConnection(con);

            Logger.LogTrace();
            //ConsoleWrap.Green($"{DateTime.Now}	{method.DeclaringType.FullName.ToUpper()}.{method.Name.ToUpper()}() Complete initilization");
            while (isConnected)
            {
                foreach (var connect in connections)
                {
                    if (connect.TaskForRead != null && connect.TaskForRead.IsFaulted)
                    {
                        throw connect.TaskForRead.Exception;
                    }
                }
            }

            Dispose();
            return Task.CompletedTask;
        }

        public void Update()
        {

        }
        #endregion
    }
}
