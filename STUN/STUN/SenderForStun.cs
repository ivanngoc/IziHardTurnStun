#pragma warning disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using IziHardGames;
using IziHardGames.STUN.Contracts;
using IziHardGames.STUN.Domain.Headers;
using IziHardGames.STUN.STUN;
using StringPrep;

namespace IziHardGames.STUN
{
    /// <summary>
    /// Mapped data struct of Whole Stun message. Including Attributes and theris values
    /// </summary>
    /// public-stun-list.txt https://gist.github.com/mondain/b0ec1cf5f60ae726202e
    public class SenderForStun
    {
        public const int SIZE_STUN_MESSAGE_HEADER = 20;

        public readonly StunHeader headerForSender;

        public string nonce;
        public string realm;

        public string userName;
        public string realmConfig;

        public string hmacKeyConfigDecoded;
        public string hmacKeyConfigEncoded;
        public string hmacKeyDecoded;
        public string password;

        public byte[] ha1;
        public byte[] hmacKeyFromConfig;

        public byte[] bufferSend;
        public byte[] bufferedUsername;
        public byte[] bufferedRealm;
        public byte[] bufferedNonce;
        public byte[] bufferedPassword;

        /// <summary>
        /// 4 мибибайта ~ 4 мегабайта
        /// </summary>
        public const int MIB4 = 4194304;

        /// <summary>
        /// Current index of <see cref="bufferSend"/> to write from
        /// </summary>
        public int length = StunHeader.SIZE;
        public int positionAfterAuthenticationData;
        /// <summary>
        /// Длина аттрибутов авторизации (realm, username, nonce)
        /// </summary>
        public int lengthAuthentication;
        public int countSends;
        /// <summary>
        /// Key - <see cref="AttributeForStun.Type"/><br/>
        /// value - index in <see cref="bufferSend"/>attribute start from
        /// </summary>
        public Dictionary<int, int> offsetsInBufferByAttributeType = new Dictionary<int, int>(64);
        public StunConnectionConfig config;
        private readonly IConnection connection;
        private readonly ConnectionForStun connectionForStun;

        public SenderForStun(ConnectionForStun connection)
        {
            this.connectionForStun = connection;
            this.connection = connection.Connection;

            bufferSend = new byte[MIB4];
            headerForSender = new StunHeader(bufferSend);
        }

        #region Header
        public void HeaderCopy(byte[] stunMessageHeaderAsBytes)
        {
            Buffer.BlockCopy(stunMessageHeaderAsBytes, 0, bufferSend, 0, SIZE_STUN_MESSAGE_HEADER);
        }
        /// <summary>
        /// Copy block from filed <see cref="headerForSender"/> to <see cref="bufferSend"/>
        /// </summary>
        public void HeaderUpdate()
        {
            throw new NotImplementedException();
            //Buffer.BlockCopy(stunHeader.Buffer, 0, bufferSend, 0, SIZE_STUN_MESSAGE_HEADER);
        }
        #endregion

        #region Write
        public void Send()
        {
            //countSends++;
            //var method = System.Reflection.MethodBase.GetCurrentMethod();
            //ConsoleWrap.Green($"{DateTime.Now}	{method.DeclaringType.FullName.ToUpper()}.{method.Name.ToUpper()}() #{countSends} position: {length} TransID: {headerForSender.ToStringTransactionIdAsHex()}{Environment.NewLine}{BitConverter.ToString(bufferSend.Take(length).ToArray())}");
            connection.Send(new ReadOnlySpan<byte>(bufferSend, 0, length));
        }
        #endregion

        #region Read

        #endregion

        #region Make Predefined Message (Preset)
        /// <remarks>
        /// Doesn't affect first 20 bytes of header, except length field
        /// </remarks>
        public void MakeAuthenticationMessageLongTerm()
        {
            CreateCredentials();
            PutAttributesForAuthentication(20);
            PutMessageIntegrity();
        }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5389#section-10.1
        /// </summary>
        public void MakeAuthenticationMessageShortTerm()
        {
            int positionLocal = PutAttributeOfUsername(20);

            positionLocal = PutAttributeOfRealm(positionLocal, bufferedRealm);
            Console.WriteLine($"Position {positionLocal}");

            int indexMessageIntegrity = positionLocal;
            int offsetMessageIntegrity = positionLocal + 4;
            positionLocal = PutMessageIntegrity();
            //stunHeader.SetMessageLength((UInt16)(positionLocal - 20));
            //hmacKey = StunHeader.GenerateHashKey(userName, realm, password);
            //ConsoleWrap.Green($"MD5: {BitConverter.ToString(hmacKey)}");

            //using
            //byte[] hmac = StunHeader.GenerateHMACSHA1(bufferedPassword, bufferSend, 0, indexMessageIntegrity);//positionLocal-24

            //byte[] hmac1 = StunHeader.GenerateHMACSHA1(password, bufferSend.Take(positionLocal).ToArray());
            //ConsoleWrap.Green($"HMAC0: {BitConverter.ToString(hmac)}. PositionLocal {positionLocal}. PositionMSG {position}. indexMessageIntegrity {indexMessageIntegrity}");
            //ConsoleWrap.Green($"HMAC1: {BitConverter.ToString(hmac1)}");

            //ToSendBufferPutMessageIntegrity(offsetMessageIntegrity, hmac);

            Console.WriteLine(BitConverter.ToString(bufferedUsername));
            Console.WriteLine(BitConverter.ToString(bufferedRealm));
            Console.WriteLine(BitConverter.ToString(bufferedPassword));

            throw new NotImplementedException();
        }

        public void CreateCredentials()
        {
            var Secret = config.Secret;
            var User = config.User;
            var unixTimestamp = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds();
            var usercombo = config.usercombo = $"{unixTimestamp}:{User}";
            var secretAsBytes = Secret.GetBytes();
            var usercomboAsBytes = usercombo.GetBytes();
            SetUserName(usercombo, usercomboAsBytes);

            using (HMACSHA1 sha1 = new HMACSHA1(secretAsBytes))
            {
                var passwordComboAsBytes = sha1.ComputeHash(usercomboAsBytes);
                var passwordCombo = Convert.ToBase64String(passwordComboAsBytes);
                var md5 = MD5.Create();
                ha1 = md5.ComputeHash($"{usercombo}:{this.realm}:{passwordCombo}".GetBytes());
            }
        }
        #endregion

        #region Buffer
        public int PutAttribute(int offset, ushort attributeType, ReadOnlySpan<byte> data)
        {
            ushort lengthData = (ushort)data.Length;
            ushort lengthDataAligned = lengthData.AlignToBoundry(4);
            EnsureBufferSize(offset, lengthDataAligned);
            this.length = AttributeForStun.PutToBuffer(bufferSend, offset, attributeType, data, (ushort)lengthDataAligned);
            return AttributeForStun.SIZE + lengthDataAligned;
        }
        public int PutAttribute(int offset, ushort attributeType, Memory<byte> data)
        {
            return PutAttribute(offset, attributeType, data.Span);
        }
        public int PutAttribute<T>(int offset, ushort attributeType, T data) where T : unmanaged
        {
#if DEBUG
            if ((Marshal.SizeOf<T>() % 4) != 0) throw new NotSupportedException($"Структура должна быть выровнена на 4 байта согласно документации");
#endif
            ushort length = (ushort)Marshal.SizeOf<T>();
            EnsureBufferSize(offset, length);
            attributeType.WriteToBufferBigEndian(bufferSend, offset);
            length.WriteToBufferBigEndian(bufferSend, offset + 2);
            this.length = data.CopyToBuffer(bufferSend, offset + 4);
            return AttributeForStun.SIZE + length;
        }
        public int PutAttributeOfUsername(int offset)
        {
            int lengthData = bufferedUsername.Length;
            int lengthDataAligned = lengthData.AlignToBoundry(4);
            EnsureBufferSize(offset, lengthDataAligned);
            this.length = AttributeForStun.PutToBuffer(bufferSend, offset, ConstantsForStun.ATTR_Username, lengthData, bufferedUsername, (ushort)lengthDataAligned);
            return 4 + lengthDataAligned;
        }
        public int PutAttributeOfRealm(int offset, byte[] realm)
        {
            int lengthData = realm.Length;
            int lengthDataAligned = lengthData.AlignToBoundry(4);
            EnsureBufferSize(offset, lengthDataAligned);
            this.length = AttributeForStun.PutToBuffer(bufferSend, offset, ConstantsForStun.ATTR_Realm, lengthData, realm, (ushort)lengthDataAligned);
            return 4 + lengthDataAligned;
        }
        public int PutAttributeOfNonce(int offset, byte[] data)
        {
            int lengthData = data.Length;
            int lengthDataAligned = lengthData.AlignToBoundry(4);
            EnsureBufferSize(offset, lengthDataAligned);
            this.length += 4 + lengthDataAligned;
            return AttributeForStun.PutToBuffer(bufferSend, offset, ConstantsForStun.ATTR_Nonce, lengthData, data, (ushort)lengthDataAligned);
        }
        public int PutMessageIntegrity()
        {
            var offset = this.length;
            int indexMessageIntegrity = this.length;
            ushort lengthData = 20;
            EnsureBufferSize(offset, lengthData);
            this.length += 24;
            headerForSender.SetMessageLength((UInt16)(length - 20));
            byte[] hmac = StunHeader.GenerateHMACSHA1(ha1, bufferSend, 0, indexMessageIntegrity);
            return AttributeForStun.PutToBuffer(bufferSend, offset, ConstantsForStun.ATTR_MessageIntegrity, lengthData, hmac, lengthData);
        }
        public void PutAttributesForAuthentication(int offset)
        {
            lengthAuthentication = default;
            lengthAuthentication += PutAttributeOfUsername(offset);
            lengthAuthentication += PutAttributeOfRealm(this.length, bufferedRealm);
            lengthAuthentication += PutAttributeOfNonce(this.length, bufferedNonce);
            throw new NotImplementedException();
            /// lengthAuthentication += ToSendBufferPutAttribute(this.position, StunAttribute.Requested_Transport, new TurnRequestedTransport(ProtocolNumber.TCP));

            headerForSender.SetMessageLength(lengthAuthentication);
            positionAfterAuthenticationData = this.length;
        }
        protected void EnsureBufferSize(int offset, int lengthData)
        {
            if ((offset + lengthData) >= bufferSend.Length)
            {
                var temp = new byte[bufferSend.Length << 1];
                Buffer.BlockCopy(bufferSend, 0, temp, 0, bufferSend.Length);
                bufferSend = temp;
            }
        }
        #endregion
        public void SetUserName(string username, byte[] buffer = default)
        {
            this.userName = new SASLprep().Prepare(username);

            if (buffer == default)
            {
                bufferedUsername = this.userName.GetBytes();
            }
            else
            {
                bufferedUsername = buffer;
            }
        }
        public void SetPassword(string psw)
        {
            //this.password = new SASLprep().Prepare(psw);
            this.password = psw;
            bufferedPassword = this.password.GetBytes();
        }
        public void SetRealm(string realm)
        {
            this.realm = realm;
            bufferedRealm = realm.GetBytes();
        }
        public void SetNonce(string nonce)
        {
            this.nonce = nonce;
            bufferedNonce = nonce.GetBytes();
        }

        public void ClearExceptHeader()
        {
            for (int i = 20; i < bufferSend.Length; i++)
            {
                bufferSend[i] = default;
            }
            length = 20;
        }
        public void SetPositionToAuthentication()
        {
            headerForSender.SetMessageLength(lengthAuthentication);
            this.length = lengthAuthentication + StunHeader.SIZE;
        }
        public void AttributesClear()
        {
            headerForSender.SetMessageLength(0);
            length = StunHeader.SIZE;
        }

#if DEBUG
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5769#section-2.1
        /// </summary>
        public static void Test0()
        {
            var md5 = MD5.Create();

            string userName = "evtj:h6vY";
            string password = new SASLprep().Prepare("VOkJxbRl1RmTxUk/WvJxBt");
            byte[] passwordBytes = password.GetBytes();
            string complex = $"{userName}::{password}";

            byte[] complexKey = md5.ComputeHash(complex.GetBytes());

            byte[] byteArray = new byte[]
            {
                    0x00, 0x01, 0x00, 0x58,
                    0x21, 0x12, 0xa4, 0x42,
                    0xb7, 0xe7, 0xa7, 0x01,
                    0xbc, 0x34, 0xd6, 0x86,
                    0xfa, 0x87, 0xdf, 0xae,
                    0x80, 0x22, 0x00, 0x10,
                    0x53, 0x54, 0x55, 0x4e,
                    0x20, 0x74, 0x65, 0x73,
                    0x74, 0x20, 0x63, 0x6c,
                    0x69, 0x65, 0x6e, 0x74,
                    0x00, 0x24, 0x00, 0x04,
                    0x6e, 0x00, 0x01, 0xff,
                    0x80, 0x29, 0x00, 0x08,
                    0x93, 0x2f, 0xf9, 0xb1,
                    0x51, 0x26, 0x3b, 0x36,
                    0x00, 0x06, 0x00, 0x09,
                    0x65, 0x76, 0x74, 0x6a,
                    0x3a, 0x68, 0x36, 0x76,
                    0x59, 0x20, 0x20, 0x20,
                    0x00, 0x08, 0x00, 0x14,
                    0x9a, 0xea, 0xa7, 0x0c,
                    0xbf, 0xd8, 0xcb, 0x56,
                    0x78, 0x1e, 0xf2, 0xb5,
                    0xb2, 0xd3, 0xf2, 0x49,
                    0xc1, 0xb5, 0x71, 0xa2,
                    0x80, 0x28, 0x00, 0x04,
                    0xe5, 0x7a, 0x3b, 0xcf,
            };
            Console.WriteLine($"Total Length {byteArray.Length}");

            int indexWithoutFingerPrint = 80;
            byte[] lengthBytes = BitConverter.GetBytes(indexWithoutFingerPrint);

            ReaderForStun stunMessageReader = new ReaderForStun(null);
            stunMessageReader.ReadFromBuffer(byteArray);
            byteArray[2] = lengthBytes[1];
            byteArray[3] = lengthBytes[0];
            //stunMessageReader.sendHeader.SetMessageLength(indexWithoutFingerPrint);
            stunMessageReader.ReadFromBuffer(byteArray);

            byte[] copy = byteArray.Take(100).ToArray();

            for (int i = 80; i < 100; i++)
            {
                copy[i] = default;
            }
            copy[2] = lengthBytes[1];
            copy[3] = lengthBytes[0];


            var hash0 = StunHeader.GenerateHMACSHA1(password, copy);
            var hash1 = StunHeader.GenerateHMACSHA1(new SASLprep().Prepare(password), copy);
            var hash2 = StunHeader.GenerateHMACSHA1($"{userName}:{password}", copy);
            var hash3 = StunHeader.GenerateHMACSHA1(passwordBytes, copy, 0, 100);
            var hash4 = StunHeader.GenerateHMACSHA1(complexKey, copy, 0, 100);
            var hash5 = StunHeader.GenerateHMACSHA1(passwordBytes, copy, 0, indexWithoutFingerPrint - 4);

            Console.WriteLine(BitConverter.ToString(hash0));
            Console.WriteLine(BitConverter.ToString(hash1));
            Console.WriteLine(BitConverter.ToString(hash2));
            Console.WriteLine(BitConverter.ToString(hash3));
            Console.WriteLine(BitConverter.ToString(hash4));

            //winner 
            using (HMACSHA1 sha1 = new HMACSHA1(passwordBytes))
            {
                var v = sha1.ComputeHash(copy);

                Console.WriteLine(BitConverter.ToString(v));
            }
            using (HMACSHA1 sha1 = new HMACSHA1(passwordBytes))
            {
                var v = sha1.ComputeHash(copy.Take(76).ToArray());

                Console.WriteLine(BitConverter.ToString(v) + "	Right");
            }
            Console.WriteLine(BitConverter.ToString(hash5) + "	Right");

            using (HMACSHA1 sha1 = new HMACSHA1(passwordBytes))
            {
                var v = sha1.ComputeHash(copy.Take(80).ToArray());

                Console.WriteLine(BitConverter.ToString(v));
            }
            Console.WriteLine($"Test0 Complete");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc5769#section-2.4
        /// </summary>
        public static void Test1()
        {
            int length = 116;
            int indexMI = 116 - 20 - 1;

            string username = "<U+30DE><U+30C8><U+30EA><U+30C3><U+30AF><U+30B9>";
            string password = "The<U+00AD>M<U+00AA>tr<U+2168>";
            string passwordCorrect = "TheMatrIX";
            string passwordPrep = new SASLprep().Prepare(password);

            byte[] raw = new byte[]
            {
                0x00, 0x01, 0x00, 0x60,
                0x21, 0x12, 0xa4, 0x42,
                0x78, 0xad, 0x34, 0x33,
                0xc6, 0xad, 0x72, 0xc0,
                0x29, 0xda, 0x41, 0x2e,
                0x00, 0x06, 0x00, 0x12,
                0xe3, 0x83, 0x9e, 0xe3,
                0x83, 0x88, 0xe3, 0x83,
                0xaa, 0xe3, 0x83, 0x83,
                0xe3, 0x82, 0xaf, 0xe3,
                0x82, 0xb9, 0x00, 0x00,
                0x00, 0x15, 0x00, 0x1c,
                0x66, 0x2f, 0x2f, 0x34,
                0x39, 0x39, 0x6b, 0x39,
                0x35, 0x34, 0x64, 0x36,
                0x4f, 0x4c, 0x33, 0x34,
                0x6f, 0x4c, 0x39, 0x46,
                0x53, 0x54, 0x76, 0x79,
                0x36, 0x34, 0x73, 0x41,
                0x00, 0x14, 0x00, 0x0b,
                0x65, 0x78, 0x61, 0x6d,
                0x70, 0x6c, 0x65, 0x2e,
                0x6f, 0x72, 0x67, 0x00,
                0x00, 0x08, 0x00, 0x14,
                0xf6, 0x70, 0x24, 0x65,
                0x6d, 0xd6, 0x4a, 0x3e,
                0x02, 0xb8, 0xe0, 0x71,
                0x2e, 0x85, 0xc9, 0xa2,
                0x8c, 0xa8, 0x96, 0x66,
            };

            byte[] originMI = raw.Skip(length - 20).Take(20).ToArray();
            Console.WriteLine(BitConverter.ToString(originMI));

            ReaderForStun stunMessageReader = new ReaderForStun(null);
            stunMessageReader.ReadFromBuffer(raw);
            Console.WriteLine($"Test1 Complete");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        public static void TestPreset()
        {
            string username = "user";
            string paassword = "pass";
            byte[] passwordBytes = paassword.GetBytes();

            byte[] raw = new byte[]
            {
                0x00, 0x03, 0x00, 0x2C,	//0x003 = allocate | 0x2C=44
				0x21, 0x12, 0xA4, 0x42,
				//TrID
				0x1A, 0x56, 0x28, 0xF0,
                0x36, 0xB0, 0xAD, 0x56,
                0xF6, 0x2C, 0x72, 0xE3,

                0x00, 0x06, 0x00, 0x04,
                0x75, 0x73, 0x65, 0x72,	//username value

				0x00, 0x14, 0x00, 0x05,	// 0x14=Realm
				0x72, 0x65, 0x61, 0x6C,
                0x6D, 0x00, 0x00, 0x00,

                0x00, 0x08, 0x00, 0x14, //Message Integrity atr
				0xCE, 0xF7, 0x71, 0x6A,
                0xDE, 0xFA, 0x0F, 0xCF,
                0x05, 0x65, 0x88, 0x63,
                0x97, 0x10, 0x1D, 0xDE,
                0x99, 0x8F, 0xBC, 0xFC
            };

            int indexME = 40;
            var copyMi = raw.Skip(indexME + 4).Take(20).ToArray();

            for (int i = indexME + 4; i < raw.Length; i++)
            {
                raw[i] = default;
            }

            using (HMACSHA1 sha1 = new HMACSHA1(passwordBytes))
            {
                var v = sha1.ComputeHash(raw.Take(indexME).ToArray());

                Console.WriteLine(BitConverter.ToString(v));
                Console.WriteLine(BitConverter.ToString(copyMi));
            }

            Console.WriteLine($"{nameof(TestPreset)} Completed");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
#endif
    }
}
