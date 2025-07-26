using IziHardGames.STUN.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static IziHardGames.STUN.STUN.ReaderForStun;
using static IziHardGames.STUN.Domain.Headers.StunHeader;
using TData = System.ReadOnlySpan<byte>;
using static IziHardGames.STUN.ConstantsForStun;
using System.Security.Cryptography;
using IziHardGames.STUN.Domain.Headers;
using Microsoft.Extensions.Logging;


namespace IziHardGames.STUN.STUN
{
    public class ReaderForStun
    {
        public delegate void RouteAttributeData(TData span);

        protected readonly RouteAttributeData dummyAttrReader;
        protected readonly Action dummyMethodHandler;

        private readonly byte[] bufferAttributeResponse = new byte[4];
        private readonly byte[] bufferForResponseHeader = new byte[20];
        private readonly Queue<TransactionID> transactionIDs = new Queue<TransactionID>(128);

        /// <summary>
        /// Common handlers for each attributes
        /// </summary>
        protected readonly Dictionary<int, RouteAttributeData> actionsPerAttributeType;
        /// <summary>
        /// Unique per each method handlers for attributes
        /// </summary>
        protected readonly Dictionary<int, SetPerMethod> attributeHanndlersPerMethod = new Dictionary<int, SetPerMethod>();

        protected Action[] methodHandlers;

        public string nonce;
        public string realm;
        public string errorMessage;
        public string username;

        public readonly StunHeader headerForRead;
        public Encoding encoding = Encoding.UTF8;

        private bool isConnected;

        public int countReads;



        public DataStunHeader lastHeaderForStun;
        public AttributeForStun lastHeaderForAttr;

        public event Action<StunAddress> OnSelfReflixiveAddressRecivedEvent;
        public event Action<StunAddress> OnMappedAdressEvent;

        protected ConnectionForStun connectionForStun;
        protected readonly StunClient client;

        public ReaderForStun(ConnectionForStun connectionForStun)
        {
            dummyAttrReader = HandleAttrDummy;
            dummyMethodHandler = DummyMethodHandler;

            client = connectionForStun.stunClient;

            InitilizeMethodHandlers();

            this.connectionForStun = connectionForStun;
            headerForRead = new StunHeader(bufferForResponseHeader);

            actionsPerAttributeType = new Dictionary<int, RouteAttributeData>()
            {
                //STUN
                [ATTR_Mapped_Address] = HandleAttrMappedAddress,
                [ATTR_XOR_Mapped_Address] = dummyAttrReader,
                [ATTR_Username] = HandleAttrUserName,
                [ATTR_MessageIntegrity] = dummyAttrReader,
                [ATTR_Fingerprint] = dummyAttrReader,
                [ATTR_Error_Code] = HandleAttrError,
                [ATTR_Realm] = HandleAttrRealm,
                [ATTR_Nonce] = HandleAttrNonce,
                [ATTR_Unknown_Attributes] = dummyAttrReader,
                [ATTR_Software] = HandleAttrSoftware,
                [ATTR_Alternate_Server] = dummyAttrReader,

                //ICE
                [ATTR_PRIORITY] = dummyAttrReader,
                [ATTR_USE_CANDIDATE] = dummyAttrReader,
                [ATTR_ICE_CONTROLLED] = dummyAttrReader,
                [ATTR_ICE_CONTROLLING] = dummyAttrReader,

                //NAT 
                [ATTR_CHANGE_REQUEST] = (x) => throw new NotImplementedException(encoding.GetString(x)),
                [ATTR_RESPONSE_PORT] = (x) => throw new NotImplementedException(encoding.GetString(x)),
                [ATTR_PADDING] = (x) => throw new NotImplementedException(encoding.GetString(x)),
                [ATTR_CACHE_TIMEOUT] = (x) => throw new NotImplementedException(encoding.GetString(x)),
                [RESPONSE_ORIGIN] = HandleAttrRESPONSE_ORIGIN,
                [OTHER_ADDRESS] = HandleAttrOTHER_ADDRESS,
            };

            var setForBinding = new SetPerMethod(METHOD_BINDING);
            attributeHanndlersPerMethod.Add(METHOD_BINDING, setForBinding);

            setForBinding.AddHandler(CLASS_SUCCESS_RESPONSE, ATTR_Mapped_Address, HandleAtMethodBindAttrMappedAddress);
            setForBinding.AddHandler(CLASS_SUCCESS_RESPONSE, ATTR_XOR_Mapped_Address, HandleAtMethodBindAttrMappedAddressXor);
        }

        #region Read Handlers Origin
        private void HandleAttrOTHER_ADDRESS(TData memory)
        {
            throw new NotImplementedException();
        }
        private void HandleAttrRESPONSE_ORIGIN(TData memory)
        {
            StunAddress stunMappedAddress = StunAddress.FromMemory(memory);
            IPAddress iPAddress;

            if (stunMappedAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv4 data = AttributeDataForIPv4.FromMemory(memory);
                iPAddress = data.IPAddressXor;
                //client.logger.LogAttributeInterpetation($"{nameof(HandleAttrRESPONSE_ORIGIN)}" + stunMappedAddress.ToStringInfo() + data.ToStringInfo());
            }
            else if (stunMappedAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv6 data = AttributeDataForIPv6.FromMemory(memory);
                //client.logger.LogAttributeInterpetation($"{nameof(HandleAttrRESPONSE_ORIGIN)}" + stunMappedAddress.ToStringInfo() + data.ToStringInfo());
            }
            else
            {
                throw new FormatException(memory.ToHex());
            }
        }
        private void HandleAttrNotSet(TData memory)
        {
            Console.WriteLine(encoding.GetString(memory));
        }
        private void HandleAttrNonce(TData memory)
        {
            nonce = encoding.GetString(memory);
        }
        private void HandleAttrSoftware(TData memory)
        {
            var software = encoding.GetString(memory);
            connectionForStun.SetServerSoftware(software);
        }
        private void HandleAttrRealm(TData memory)
        {
            realm = encoding.GetString(memory);
        }
        private void HandleAttrError(TData memory)
        {
            var span = memory.Slice(0, 4);
            StunErrorCode stunErrorCode = StunErrorCode.FromBuffer(memory);
            //client.logger.LogError(stunErrorCode, memory);
        }
        private void HandleAttrUserName(TData memory)
        {
            username = encoding.GetString(memory);
            //client.logger.LogAttributeInterpetation($"UserName:{username}");
#if DEBUG
            //Console.WriteLine(new SASLprep().Prepare(username));
#endif
        }
        private void HandleAttrMappedAddress(TData memory)
        {
            StunAddress stunMappedAddress = StunAddress.FromMemory(memory);

            IPAddress iPAddress;

            if (stunMappedAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv4 data = AttributeDataForIPv4.FromMemory(memory);
                iPAddress = data.IPAddress;
                //client.logger.LogAttributeInterpetation(stunMappedAddress.ToStringInfo() + data.ToStringInfo());
                OnMappedAdressEvent?.Invoke(stunMappedAddress);
            }
            else if (stunMappedAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv6 data = AttributeDataForIPv6.FromMemory(memory);
                //client.logger.LogAttributeInterpetation(stunMappedAddress.ToStringInfo() + data.ToStringInfo());
            }
            else
            {
                throw new FormatException(memory.ToHex());
            }
        }
        private void HandleAtMethodBindAttrMappedAddress(TData memory)
        {
            StunAddress value = StunAddress.FromMemory(memory);
            connectionForStun.addressMappedOnBind = value;

            IPAddress iPAddress;

            if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv4 data = AttributeDataForIPv4.FromMemory(memory);
                iPAddress = data.IPAddress;
                connectionForStun.addressMappedOnBindData = data;
                connectionForStun.addressMappedOnBindIp = iPAddress;

                //client.logger.LogAttributeInterpetation($"{nameof(HandleAtMethodBindAttrMappedAddress)}" + value.ToStringInfo() + data.ToStringInfo());
                OnMappedAdressEvent?.Invoke(value);
            }
            else if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv6 data = AttributeDataForIPv6.FromMemory(memory);
                //client.logger.LogAttributeInterpetation($"{nameof(HandleAtMethodBindAttrMappedAddress)}" + value.ToStringInfo() + data.ToStringInfo());
            }
            else
            {
                throw new FormatException(memory.ToHex());
            }
        }
        protected void HandleAtMethodBindAttrMappedAddressXor(TData memory)
        {
            StunAddress value = StunAddress.FromMemory(memory);
            connectionForStun.addressMappedXorOnBind = value;

            IPAddress iPAddress;

            if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv4 data = AttributeDataForIPv4.FromMemory(memory);
                iPAddress = data.IPAddressXor;
                connectionForStun.addressMappedXorOnBindData = data;
                connectionForStun.addressMappedXorOnBindIp = iPAddress;

                //client.logger.LogAttributeInterpetation($"{nameof(HandleAtMethodBindAttrMappedAddressXor)}" + value.ToStringInfoXor() + data.ToStringInfoXor());
                OnMappedAdressEvent?.Invoke(value);
            }
            else if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv6 data = AttributeDataForIPv6.FromMemory(memory);
                //client.logger.LogAttributeInterpetation($"{nameof(HandleAtMethodBindAttrMappedAddressXor)}" + value.ToStringInfoXor() + data.ToStringInfoXor());
            }
            else
            {
                throw new FormatException(memory.ToHex());
            }
        }


        protected void DummyMethodHandler()
        {
            //client.logger.LogStunReadHeaderCompletes($"No Dedicated Handler For Method. Header is read. {lastHeaderForStun.ToStringInfo()}");
        }
        protected void HandleAttrDummy(TData memory)
        {
            //client.logger.LogAttributeInterpetation($"{nameof(HandleAttrDummy)} StunHeader:{lastHeaderForStun.ToStringInfo()}.  AttrHeader:{lastHeaderForAttr.ToStringInfo()}    Data:{memory.ToUtf16()}", ConsoleColor.Red);
        }
        #endregion

        public void ReadUdpOnce()
        {
            ReadUdpDatagram(connectionForStun.Connection.Recieve());
        }

        /// <summary>
        /// Routing: Method => Class => Attribute
        /// </summary>
        /// <param name="dgram"></param>
        /// <exception cref="FormatException"></exception>
        private void ReadUdpDatagram(TData dgram)
        {
            DataStunHeader dsh = lastHeaderForStun = dgram.ToStruct<DataStunHeader>();
            Console.WriteLine(dgram.ToUtf16());
            Console.WriteLine(dgram.ToBase64());
            //client.logger.LogStunHeader(dsh);

            if (dsh.magicCookie != MAGIC_COOKIE_FOR_CLIENT)
            {
                throw new FormatException($"Wrong Header {dsh.ToStringInfo()}");
            }

            int length = dsh.Length;
            int offset = SIZE;

            while (length > 0)
            {
                AttributeForStun attr = lastHeaderForAttr = dgram.Slice(offset, AttributeForStun.SIZE).ToStruct<AttributeForStun>();
                //client.logger.LogAttribute(attr);
                int dataLength = attr.Length;
                int dataLengthAligned = dataLength.AlignToBoundry(4);
                int size = dataLengthAligned + AttributeForStun.SIZE;
                offset += AttributeForStun.SIZE;
                TData attrData = dgram.Slice(offset, dataLengthAligned);
                offset += dataLengthAligned;
                length -= size;
                //client.logger.LogAttributeData(attr, attrData);
                actionsPerAttributeType[attr.Type].Invoke(attrData);
                attributeHanndlersPerMethod[dsh.messageType.Method].Handle(dsh.messageType.Class, attr.Type, attrData);
            }

            methodHandlers[dsh.messageType.GetStunMethodValue()].Invoke();
        }

        /// <cref="https://datatracker.ietf.org/doc/html/rfc5389#section-7.3"/> - describe how to recieve message
        public void ReadTcpOnce(NetworkStream stream)
        {
            countReads++;

            var header = headerForRead;

            int count = stream.Read(bufferForResponseHeader, 0, 20);

            if (count > 0)
            {
                DataStunHeader dsh = lastHeaderForStun = DataStunHeader.FromBuffer(bufferForResponseHeader);
                //client.logger.LogStunHeader(dsh);

                // check magic cookie
                if (header.IsMatchMagicCookie(bufferForResponseHeader))
                {
                    int lengthTotal = dsh.Length;

                    while (lengthTotal > 0)
                    {
                        int read = stream.Read(bufferAttributeResponse, 0, 4);

                        AttributeForStun attr = lastHeaderForAttr = AttributeForStun.FromBuffer(bufferAttributeResponse, 0);
                        //client.logger.LogAttribute(attr);

                        int length = attr.Length;
                        int lengthAligned = length.AlignToBoundry(4);

                        if (length > 0)
                        {
                            var rent = ArrayPool<byte>.Shared.Rent(lengthAligned);
                            var attrData = new Span<byte>(rent, 0, length);
                            int readedData = stream.Read(attrData);

                            //client.logger.LogAttributeData(attr, attrData);

                            actionsPerAttributeType[attr.Type].Invoke(attrData);
                            attributeHanndlersPerMethod[dsh.messageType.Method].Handle(dsh.messageType.Class, attr.Type, attrData);

                            ArrayPool<byte>.Shared.Return(rent);
                        }
                        lengthTotal -= AttributeForStun.SIZE + lengthAligned;
                    }
                }
                else
                {
                    throw new FormatException($"STUN Magic Cookie is not Valid");
                }
                //client.logger.LogStunHeader(dsh);
                methodHandlers[dsh.messageType.GetStunMethodValue()].Invoke();
            }
        }

        public void ReadFromBuffer(byte[] buffer)
        {
            int position = 20;
            DataStunHeader dataStunHeader = DataStunHeader.FromBuffer(buffer);
#if DEBUG
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            Logger.LogTrace();
            //ConsoleWrap.Green($"{DateTime.Now}	{method.DeclaringType.FullName.ToUpper()}.{method.Name.ToUpper()}()");
            //ConsoleWrap.Yellow(dataStunHeader.ToStringInfo());
#endif
            ReadByLengthFromBuffer(dataStunHeader.Length, position, buffer);
        }

        private void ReadByLengthFromBuffer(int lengthTotal, int offset, byte[] buffer)
        {
            int position = offset;
#if DEBUG
            if (lengthTotal == default) throw new Exception($"Length total is 0");
#endif
            while (lengthTotal > 0)
            {
                AttributeForStun stunAttribute = AttributeForStun.FromBuffer(buffer, position);
                //ConsoleWrap.Cyan(stunAttribute.ToStringInfo());
                int length = stunAttribute.Length;
                int lengthAligned = length.AlignToBoundry(4);

                if (length > 0)
                {
                    var memory = new Span<byte>(buffer, position + 4, lengthAligned);
                    //if (stunAttribute.Type == ConstantsForStun.MessageIntegrity)
                    //{
                    //    ConsoleWrap.Cyan(BitConverter.ToString(memory.Span.ToArray()));
                    //}
                    //else
                    //{
                    //    ConsoleWrap.Cyan(encoding.GetString(memory.Span));
                    //}
                    actionsPerAttributeType[stunAttribute.Type].Invoke(memory);
                }
                position += 4 + lengthAligned;
                lengthTotal -= 4 + lengthAligned;
                //Console.WriteLine($"Length Total {lengthTotal}");
            }
        }
        public void Run()
        {
            isConnected = true;

            while (isConnected)
            {
                if (connectionForStun.Protocol == ConnectionForStun.TCP)
                {
                    ReadTcpOnce((connectionForStun.Connection as ConnectionTcp).GetStream());
                    continue;
                }
                if (connectionForStun.Protocol == ConnectionForStun.UDP)
                {
                    ReadUdpOnce();
                    continue;
                }
                throw new NotSupportedException($"Network Protocol Is not set properly");
            }
        }
        public void QueueResponse(SenderForStun sender)
        {
            headerForRead.SetAwaitedTransanctionId(sender.headerForSender);
            //transactionIDs.Enqueue(sender.headerForSender.TransactionId);
        }
        public void Stop()
        {
            isConnected = false;
        }
        protected virtual void InitilizeMethodHandlers()
        {
            methodHandlers = new Action[3];
            FillMethodsWithStunSpec();
        }

        protected virtual void FillMethodsWithStunSpec()
        {
            methodHandlers[METHOD_RESERVED] = dummyMethodHandler;
            methodHandlers[METHOD_BINDING] = ReactOnMethodBinding;
            methodHandlers[METHOD_RESERVER_WAS_SHARED_SECRET] = dummyMethodHandler;
        }

        protected void ReactOnMethodBinding()
        {
            //client.logger.LogHandler(nameof(METHOD_BINDING));
        }

        public T FindAttributeData<T>() where T : unmanaged
        {
            throw new NotImplementedException();
        }

        #region Static 
        public static IPAddress GetIpAdress(Memory<byte> memory)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

    public class SetPerMethod
    {
        public int method;
        public readonly SetPerHeaderClass[] handlers;

        public SetPerMethod(int method)
        {
            this.method = method;

            handlers = new SetPerHeaderClass[4];

            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i] = new SetPerHeaderClass(this, i);
            }
        }
        public void Handle(int headerClass, int attributeType, TData data)
        {
            handlers[headerClass].Handle(attributeType, data);
        }

        public void AddHandler(int headerClass, int attrType, RouteAttributeData action)
        {
            handlers[headerClass].AddHandler(attrType, action);
        }
    }

    public class SetPerHeaderClass
    {
        private readonly int headerClass;
        public readonly Dictionary<int, RouteAttributeData> handlers = new Dictionary<int, RouteAttributeData>();
        private readonly SetPerMethod setPerMethod;
        public SetPerHeaderClass(SetPerMethod setPerMethod, int headerClass)
        {
            this.setPerMethod = setPerMethod;
            this.headerClass = headerClass;
        }

        internal void AddHandler(int attrType, RouteAttributeData action)
        {
            handlers.Add(attrType, action);
        }

        internal void Handle(int attributeType, TData data)
        {
            if (handlers.TryGetValue(attributeType, out RouteAttributeData handler))
            {
                handler.Invoke(data);
            }
            else
            {
                Logger.LogError($"Handler for method:{MessageType.methodsByValueLittleEndian[setPerMethod.method]}, class:{(EStunClass)headerClass},    attributeType:{AttributeForStun.stunAttributeByValue[attributeType]} is not Defined");
            }
        }
    }
}
