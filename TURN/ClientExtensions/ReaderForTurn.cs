using IziHardGames.STUN;
using IziHardGames.STUN.Attributes;
using IziHardGames.STUN.Domain.Headers;
using IziHardGames.STUN.STUN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using static IziHardGames.STUN.Domain.Headers.StunHeader;
using TData = System.ReadOnlySpan<byte>;

namespace IziHardGames.TURN
{
    /// <summary>
    /// Read and route result to handler
    /// </summary>
    public class ReaderForTurn : ReaderForStun
    {
        private ConnectionForTurn connectionForTurn;

        public event Action<ConnectionForTurn> OnAllocatedEvent;
        public static event Action<ConnectionForTurn> OnAnyAllocationEvent;

        public ReaderForTurn(ConnectionForTurn connection) : base(connection)
        {
            this.connectionForTurn = connection;

            var handleDummy = dummyAttrReader;
            //TURN
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Channel_Number, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Lifetime, HandleAttrLifetime);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Data, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_XOR_Peer_Address, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_XOR_Relayed_Address, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Even_Port, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Requested_Transport, HandleAttrRequestedTransport);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Dont_Fragment, handleDummy);
            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_Reservation_Token, handleDummy);

            actionsPerAttributeType.Add(ConstantsForTurn.ATTR_CONNECTION_ID, handleDummy);

            var setForAllocate = new SetPerMethod(ConstantsForTurn.METHOD_ALLOCATE);
            var setForCreatePerm = new SetPerMethod(ConstantsForTurn.METHOD_CREATE_PERMISSION);
            var setForData = new SetPerMethod(ConstantsForTurn.METHOD_DATA);

            attributeHanndlersPerMethod.Add(ConstantsForTurn.METHOD_ALLOCATE, setForAllocate);
            attributeHanndlersPerMethod.Add(ConstantsForTurn.METHOD_CREATE_PERMISSION, setForCreatePerm);
            attributeHanndlersPerMethod.Add(ConstantsForTurn.METHOD_DATA, setForData);

            setForAllocate.AddHandler(ConstantsForStun.CLASS_SUCCESS_RESPONSE, ConstantsForTurn.ATTR_XOR_Relayed_Address, HandleAttrXorRelayedAddress);
            setForAllocate.AddHandler(ConstantsForStun.CLASS_SUCCESS_RESPONSE, ConstantsForStun.ATTR_XOR_Mapped_Address, HandleAtMethodBindAttrMappedAddressXor);

            setForData.AddHandler(ConstantsForStun.CLASS_INDICATION, ConstantsForTurn.ATTR_Data, HandleAtMethodDataAttrData);
        }

        public void HandleAtMethodDataAttrData(TData memory)
        {
            //client.logger.Log($"{nameof(HandleAtMethodDataAttrData)}    " + memory.ToUtf16());
        }

        /// <see cref="ConstantsForTurn.ATTR_Requested_Transport"/>
        public void HandleAttrRequestedTransport(TData memory)
        {
            TurnRequestedTransport val = TurnRequestedTransport.FromBuffer(memory);
            //Console.WriteLine($"{nameof(HandleRequestedTransport)} {val.ToStringInfo()}");
            //client.logger.LogAttributeInterpetation(val.ToStringInfo());
            connectionForTurn.SetTransport(val);
        }

        /// <summary>
        /// <see cref="ConstantsForTurn.ATTR_Lifetime"/>
        /// </summary>
        /// <param name="memory"></param>
        private void HandleAttrLifetime(TData memory)
        {
            connectionForTurn.Lifetime = memory.ToStruct<uint>();
            //client.logger.LogAttributeInterpetation($"Lifetime:{connectionForTurn.Lifetime}");
        }

        /// <see cref="ConstantsForTurn.ATTR_XOR_Relayed_Address"/>
        private void HandleAttrXorRelayedAddress(TData memory)
        {
            StunAddress value = StunAddress.FromMemory(memory);
            connectionForTurn.addressRelayedXor = value;

            IPAddress iPAddress;

            if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv4 data = AttributeDataForIPv4.FromMemory(memory);
                iPAddress = data.IPAddressXor;
                connectionForTurn.isAddressRelayedXorRecived = true;
                connectionForTurn.addressRelayedXorData = data;
                connectionForTurn.addressRelayedXorIp = data.IPAddressXor;

                //client.logger.LogAttributeInterpetation($"{nameof(HandleAttrXorRelayedAddress)}" + value.ToStringInfoXor() + data.ToStringInfoXor());
            }
            else if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                AttributeDataForIPv6 data = AttributeDataForIPv6.FromMemory(memory);
                //client.logger.LogAttributeInterpetation($"{nameof(HandleAttrXorRelayedAddress)}" + value.ToStringInfoXor() + data.ToStringInfoXor());
            }
            else
            {
                throw new FormatException(memory.ToHex());
            }
        }

        protected override void InitilizeMethodHandlers()
        {
            methodHandlers = new Action[10];

            for (int i = 0; i < methodHandlers.Length; i++)
            {
                methodHandlers[i] = dummyMethodHandler;
            }

            FillMethodsWithStunSpec();
            FillMethodsWithTurnSpec();
        }

        protected void FillMethodsWithTurnSpec()
        {
            methodHandlers[ConstantsForTurn.METHOD_ALLOCATE] = ReactOnMethodAllocate;
            methodHandlers[ConstantsForTurn.METHOD_REFRASH] = dummyMethodHandler;
            methodHandlers[ConstantsForTurn.METHOD_SEND] = dummyMethodHandler;
            methodHandlers[ConstantsForTurn.METHOD_DATA] = dummyMethodHandler;
            methodHandlers[ConstantsForTurn.METHOD_CREATE_PERMISSION] = ReactOnMethodPermission;
            methodHandlers[ConstantsForTurn.METHOD_CHANNEL_BIND] = dummyMethodHandler;
        }

        private void ReactOnMethodAllocate()
        {
            connectionForTurn.IsAllocated = lastHeaderForStun.messageType.GetStunClassValue() == ConstantsForStun.CLASS_SUCCESS_RESPONSE;

            Console.WriteLine($"{nameof(ConstantsForTurn.METHOD_ALLOCATE)}:{lastHeaderForStun.messageType.GetStunMethodValue()}");

            OnAllocatedEvent?.Invoke(connectionForTurn);
            OnAnyAllocationEvent?.Invoke(connectionForTurn);

            //var turnPeer = connectionForTurn.CreatePeer();
            //connectionForTurn.DoMethodCreatePermissionToPeer(turnPeer);
        }
        private void ReactOnMethodPermission()
        {
            //client.logger.LogHandler(nameof(ConstantsForTurn.METHOD_CREATE_PERMISSION));
            connectionForTurn.ReportPermissionGranted();
        }
    }
}
