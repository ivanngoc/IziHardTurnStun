using IziHardGames.Networking.IANA;
using IziHardGames.STUN.Attributes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static IziHardGames.STUN.StunHeader;

namespace IziHardGames.STUN
{
    public class ConnectionForStun : IDisposable
    {
        public const int TCP = 1;
        public const int UDP = 2;
        public int Protocol => config.Protocol;

        protected IConnection connection;
        public IConnection Connection => connection;
        protected readonly StunConnectionConfig config;

        protected bool isBinded;
        protected string serverSoftware;

        public SenderForStun StunMessageSender => stunMessageSender;
        public ReaderForStun StunMessageReader => stunMessageReader;

        protected SenderForStun stunMessageSender;
        protected ReaderForStun stunMessageReader;

        private Task taskForRead;
        public Task TaskForRead => taskForRead;

        public readonly StunClient stunClient;


        public StunAddress addressMappedOnBind;
        public AttributeDataForIPv4 addressMappedOnBindData;
        public IPAddress addressMappedOnBindIp;


        public StunAddress addressMappedXorOnBind;
        public AttributeDataForIPv4 addressMappedXorOnBindData;
        public IPAddress addressMappedXorOnBindIp;


        public ConnectionForStun(StunClient stunClient, StunConnectionConfig config)
        {
            this.stunClient = stunClient;
            this.config = config;

            switch (config.Protocol)
            {
                case TCP: connection = new ConnectionTcp(config.Host, config.Port); break;
                case UDP: connection = new ConnectionUdp(config.Host, config.Port, config.PortListen); break;
                default: throw new ArgumentOutOfRangeException(config.Protocol.ToString());
            }
        }

        public void EnsureReadedAndSender()
        {
            if (stunMessageSender == default)
            {
                stunMessageSender = new SenderForStun(this);
            }
            if (stunMessageReader == default)
            {
                stunMessageReader = new ReaderForStun(this);
            }
        }

        public virtual void Initilize()
        {
            var stunMessageSender = StunMessageSender;
            var stunMessageReader = StunMessageReader;
            StunMessageSender.config = config;

            if (!string.IsNullOrEmpty(config.Password)) StunMessageSender.SetPassword(config.Password);
            StunMessageSender.realmConfig = config.Realm;
            //coturnConnection.Configure();
            if (config.AuthType == EAuthType.LongTermCred)
            {
                StunMessageSender.hmacKeyFromConfig = StunHeader.GenerateHashKey(stunMessageSender.userName, stunMessageSender.realmConfig, stunMessageSender.password);
            }

            taskForRead = Task.Factory.StartNew(stunMessageReader.Run);
        }
        public void Dispose()
        {
            connection.Dispose();
        }

        #region STUN - Authentication, Format providing
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-10.2
        /// </summary>
        protected void AuthenticateWithLongTermCredentialMechanism(TcpClient tcpClient)
        {
            var bufferSend = stunMessageSender.bufferSend;
            StunHeader header = stunMessageSender.headerForSender;
            var stream = tcpClient.GetStream();
            /// первая отправка для получения realm аттрибута от сервера. получение ошибки 401
            /// https://datatracker.ietf.org/doc/html/rfc5766#section-16 3 абзац
            //Allocate(stream);
            ConsoleWrap.DarkRed($"FIRST MSG SEND");

            ////await first answer to get nonce and realm
            //stunMessageReader.ReadOnce(tcpClient.GetStream());
            /// вторая отправка для аутентификации на сервере
            /// https://datatracker.ietf.org/doc/html/rfc5766#section-16 5 абзац
            header.SetHeaderMethodToAllocate();
            stunMessageSender.SetRealm(stunMessageReader.realm);
            stunMessageSender.SetNonce(stunMessageReader.nonce);
            stunMessageSender.MakeAuthenticationMessageLongTerm();
#if DEBUG
            //validate
            stunMessageReader.ReadFromBuffer(stunMessageSender.bufferSend);
#endif

            //ConsoleWrap.Green($"Send authentication");
            //ConsoleWrap.DarkRed($"SECOND MSG SEND");
            //stunMessageSender.stunHeader.SetTransactionIdRandomly();
            header.NewMessage();
            stunMessageSender.Send();
            /// если после меседж интегрити будет фингер принт, то корректируется <see cref="DataStunHeader.length"/> на размер полного сообщения. См. в документации
            //stream.Write(bufferSend, 0, position);
            //ConsoleWrap.Green($"Next");

            //var reader2 = new StunMessageReader()
            //{
            //	sendHeader = header,
            //};
            //reader2.ReadOnce(stream);
        }
        /// <remarks>
        /// Not supported by https://github.com/coturn/coturn <br/>
        /// It is only for ICE.
        /// https://github.com/coturn/coturn/issues/334<br/>
        /// </summary>
        /// <inheritdoc cref="SenderForStun.MakeAuthenticationMessageShortTerm"/>
        /// <param name="tcpClient"></param>
        [Obsolete("Not Supported By Coturn")]
        protected void AuthenticateWithShortTermCredentialMechanism(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            StunHeader header = stunMessageSender.headerForSender;

            stunMessageSender.SetRealm(config.Realm);

            header.SetHeaderMethodToAllocate();
            header.SetMessageClassToRequest();

            stunMessageSender.MakeAuthenticationMessageShortTerm();
#if DEBUG
            //validate
            stunMessageReader.ReadFromBuffer(stunMessageSender.bufferSend);
#endif
            header.NewMessage();
            stunMessageSender.Send();

            var reader2 = new ReaderForStun(this)
            {

            };
            reader2.ReadTcpOnce(stream);

            while (true)
            {
                var b2 = new byte[1];
                stream.Read(b2, 0, 1);
                Console.WriteLine(BitConverter.ToString(b2));
            }
        }

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.1
        /// </summary>
        private void FormingRequestOrIndicationOverTcp()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-7.2
        /// </summary>
        private void SendingRequestOrIndicationTcp()
        {
            throw new NotImplementedException();
        }
        public void GetLongTermCredential()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region STUN Methods
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-3
        /// </summary>
        /// Response:
        /// <see cref="ConstantsForStun.ATTR_XOR_Mapped_Address"/>
        /// <see cref="ConstantsForStun.ATTR_Mapped_Address"/>
        /// <see cref="ConstantsForStun.RESPONSE_ORIGIN"/>
        /// <see cref="ConstantsForStun.ATTR_Software"/>
        public void DoMethodBind()
        {
            var header = stunMessageSender.headerForSender;
            var reader = stunMessageReader;
            var sender = stunMessageSender;

            header.SetMessageClassToRequest();
            header.SetHeaderMethodToBinding();

            sender.AttributesClear();
            reader.QueueResponse(sender);
            header.SetMessageLength(0);
            header.NewMessage();
            sender.Send();
        }
        #endregion

        /// <summary>
        /// Next, the agent uses STUN or TURN to obtain additional candidates.
        /// These come in two flavors: translated addresses on the public side of
        /// a NAT(SERVER REFLEXIVE CANDIDATES) and addresses on TURN servers
        /// (RELAYED CANDIDATES). <br/>
        /// https://datatracker.ietf.org/doc/html/rfc5245#section-2.1 <br/>
        /// </summary>
        /// <param name="networkStream"></param>
        private void SendingRequestGatherPeers(NetworkStream networkStream)
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            Logger.LogTrace();
            //ConsoleWrap.Green($"{DateTime.Now}	{method.DeclaringType.FullName.ToUpper()}.{method.Name.ToUpper()}()");

            var header = stunMessageSender.headerForSender;
            var sender = stunMessageSender;

            header.SetHeaderMethodToAllocate();
            header.SetHeaderMethodToBinding();
            sender.AttributesClear();

            sender.length = 20;
            sender.PutAttributesForAuthentication(20);
            sender.PutMessageIntegrity();
#if DEBUG
            stunMessageReader.ReadFromBuffer(sender.bufferSend);
#endif
            header.NewMessage();
            sender.Send();
        }

        internal void SetServerSoftware(string software)
        {
            this.serverSoftware = software;
        }
    }
}
