using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IziHardGames.Binary;
using IziHardGames.STUN.Domain.Headers;
using Microsoft.AspNetCore.Mvc;
using static IziHardGames.STUN.ConstantsForStun;

namespace IziHardGames.TurnClient.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StunController : ControllerBase
    {
        /// <summary>
        /// <seealso cref="DataStunHeader"/>
        /// </summary>
        static readonly byte[] StunBindingRequest = new byte[]
        {
                0x00, 0x01,             // Binding Request
                0x00, 0x00,             // Message Length
                0x21, 0x12, 0xA4, 0x42, // Magic Cookie
                0x63, 0xC7, 0x11, 0x8B, 0xA0, 0xE1, 0x0A, 0x43, 0xC1, 0xF3, 0x17, 0xA7 // Transaction ID
        };
        string stunServer = "localhost"; // Or your CoTURN server address
        int port = 3478; // Common STUN port

        [HttpGet]
        public async Task<IActionResult> Connect()
        {
            using var udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Connect(endpoint);

            var sended = await udpClient.SendAsync(StunBindingRequest, CancellationToken.None);
            var response = await udpClient.ReceiveAsync(CancellationToken.None);
            var res = BitConverter.ToString(response.Buffer);
            ReadOnlySpan<byte> span = response.Buffer.AsSpan();
            var str = BufferReader.ToStructConsume<DataStunHeader>(ref span);
            var length = str.Length;
            IPAddress? ip = null;
            int? portXor = null;
            int atrCount = default;
            while (length > 0)
            {
                atrCount++;
                var atr = BufferReader.ToStructConsume<AttributeForStun>(ref span);
                var val = BufferReader.Consume(atr.Length, ref span);
                if (atr.GetAttributeName() == nameof(ATTR_XOR_Mapped_Address))
                {
                    var xor = BufferReader.ToStructConsume<StunAddress>(ref val);
                    portXor = xor.PortXor;
                    var addrss = BufferReader.Consume(xor.LengthFollowed, ref val);
                    //Reverse(addrss);
                    ip = new IPAddress(addrss);
                }

                length -= AttributeForStun.SIZE;
                length -= atr.Length;
            }

            await Task.CompletedTask;

            return Ok(new
            {
                MethodName = str.messageType.GetStunMethodName(),
                Length = str.Length,
                AttrCount = atrCount,
                //Attr = atr.GetAttributeName(),
                Ip = ip?.ToString(),
                Port = portXor,
            });
        }

        public static void Reverse(ReadOnlySpan<byte> readOnlySpan)
        {
            Span<byte> bytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(readOnlySpan), readOnlySpan.Length);
            var last = bytes.Length - 1;
            var swaps = bytes.Length >> 1;
            for (int i = 0; i < swaps; i++)
            {
                var temp = bytes[i];
                int j = last - i;
                bytes[i] = bytes[j];
                bytes[j] = temp;
            }
        }
    }
}
