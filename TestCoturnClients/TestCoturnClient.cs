using IziHardGames.ConfigurationTools;
using IziHardGames.STUN;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IziHardGames;
using IziHardGames.Networking.IANA;
using IziHardGames.Turn.Hub;
using System.Linq;
using IziHardGames.TURN;
using System.Net;
using IziHardGames.STUN.Attributes;
using System.Buffers.Binary;
using System.Threading;

namespace TestCoturnClients
{
    public class TestCoturnClient
    {
        public int peerCount = 5;
        public List<CoturnClient> coturnClients = new List<CoturnClient>();
        public List<Task> inits = new List<Task>();
        private HubForTurnClients hubForTurnClients;

        public StunConnectionConfig configDocker = new StunConnectionConfig()
        {
            Host = "127.0.0.1",//"172.17.0.4",//172.17.0.4
            Port = 3478,// tls 5349
            PortListen = 40801,// tls 5349
            Realm = "izihard",
            Protocol = ConnectionForStun.UDP,
            User = "User1",
            Software = "TestClientIziHardGames.v0.01",
            AuthTypeValue = (int)EAuthType.NoAuth,
        };


        public void Execute1()
        {
            TurnInit.Initilize();

            hubForTurnClients = new HubForTurnClients();
            var configDockerCopy = configDocker.DeepCopyWithFieldsAndProps() as StunConnectionConfig;
            configDockerCopy.PortListen = 40888;

            Console.WriteLine(Convert.ToBase64String(BitConverter.GetBytes(StunHeader.MAGIC_COOKIE_FOR_CLIENT)));

            CoturnClient client0 = new CoturnClient("Client0");
            CoturnClient client1 = new CoturnClient("Client1");

            client0.logger.SetColor(ConsoleColor.Yellow);
            client1.logger.SetColor(ConsoleColor.Green);
            //client0.logger.Disable();
            //client1.logger.Disable();

            ReaderForTurn.OnAnyAllocationEvent += hubForTurnClients.Regist;


            Task.Factory.StartNew(() => client0.DevRun(configDocker));
            Task.Factory.StartNew(() => client1.DevRun(configDockerCopy));

            MonitorForTurnClient monitorForTurnClient = new MonitorForTurnClient(client0);

            while (true)
            {
                client0.Update();
                client1.Update();

                monitorForTurnClient.Update();

                if (client0.connections.Count > 0 && client1.connections.Count > 0)
                {
                    lock (client0.connections)
                    {
                        lock (client1.connections)
                        {
                            if (client0.connections.Any(x => x.IsAllocated) && client1.connections.Any(y => y.IsAllocated))
                            {
                                if (hubForTurnClients.TryFindClientWithName(client0.name, out ClientInfo info0))
                                {
                                    if (hubForTurnClients.TryFindClientWithName(client1.name, out ClientInfo info1))
                                    {
                                        var peer1 = client0.AddPeer(client1.connections.First().CreatePeer());
                                        var peer0 = client1.AddPeer(client0.connections.First().CreatePeer());

                                        client0.connections.First().OnPermissionSuccessEvent += () =>
                                        {
                                            client0.SendToPeer(info1.name, "This is greating message from client 0");
                                            client0.SendToPeer(info1.name, "This is greating message from client 0 123ssssssssssssssssssssssssssssssssssssssssssssss");
                                            client0.SendToPeer(info1.name, "This is greating message from client 0 555");
                                        };

                                        // 
                                        client1.ConnectTo(peer0);
                                        Thread.Sleep(5000);
                                        client0.ConnectTo(peer1);

                                        //client0.ConnectTo(client1.name, IPAddress.Parse(info0.mappedAddress), info0.mappedPort);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ConsoleWrap.Cyan("Complete. Exited");

            while (true)
            {

            }
        }
        public void Execute()
        {
            Configuration.ImportConfig(Directory.GetCurrentDirectory(), "config.json");
            StunConnectionConfig coturnConnection = new StunConnectionConfig();
            Configuration.configurationShared.GetSection("CoturnConnections:VirtualBox").Bind(coturnConnection);

            inits.Clear();

            for (int i = 0; i < peerCount; i++)
            {
                StunConnectionConfig conectionCopy = coturnConnection.ShallowCopy();
                CoturnClient coturnClient = new CoturnClient($"Client_For_{i}");
                coturnClients.Add(coturnClient);
                Console.WriteLine($"Peer created {i}");
                coturnClient.id = i;

                Action<Task> next = default;
                if (i < 1)
                {
                    next = (x) => ExecuteMasterPeer(coturnClient);
                }
                else
                {
                    next = (x) =>
                    {
                        while (true)
                        {

                        }
                    };
                }

                var task = Task.Factory.StartNew(() =>
                   {
                       coturnClient.DevRun(conectionCopy).ContinueWith(next);
                   });

                inits.Add(task);
            }

            Task.WaitAll(inits.ToArray());

            Console.WriteLine($"Init Complete");

            for (int i = 0; i < coturnClients.Count; i++)
            {
                var client = coturnClients[i];
                var sender = client.connections.First().StunMessageReader;
                var reader = client.connections.First().StunMessageReader;
                //Console.WriteLine($"CLient Info: Id {client.id} XOR {reader.xorAddressMapped.ToStringInfo()} | XOR-RELAYED-ADDRESS: {sender.xorAddressRelayed.ToStringInfo()}");
            }

            while (true)
            {

            }
        }
        public void ExecuteMasterPeer(CoturnClient coturnClient)
        {
            coturnClient.GetPeers();
        }
    }
}
