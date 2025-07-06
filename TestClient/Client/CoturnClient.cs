using IziHardGames.STUN;
using IziHardGames.STUN.Attributes;
using IziHardGames.TURN;
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
        public CoturnClient(string name) : base()
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

            ConnectionForTurn connection = new ConnectionForTurn(this, config);
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

        #region Static
#if DEBUG
        public static void Test(StunConnectionConfig config)
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine($"{DateTime.Now}	{method.DeclaringType.FullName}.{method.Name}()");
            if (false)
            {
                SenderForStun.TestPreset();
                SenderForStun.Test0();
                SenderForStun.Test1();
                Console.WriteLine($"Completed");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                //Console.ReadLine();
            }

            CoturnClient turnClient = new CoturnClient("TestClient555");

            Task.Factory.StartNew(() => turnClient.DevRun(config));
            //turnClient.Run().RunSynchronously();

            while (true)
            {

            }
        }
#endif

        #endregion

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
