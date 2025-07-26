using IziHardGames.STUN;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IziHardGames.STUN.STUN
{
    /// #Authentication and Message-Integrity Mechanisms
    /// https://datatracker.ietf.org/doc/html/rfc5389#section-10
    public enum EAuthType
    {
        None,
        // 
        NoAuth,
        LongTermCred,
        ShortTermSecret,
    }

    public class StunConnectionConfig
    {
        [JsonPropertyName("Host")] public string Host { get; set; }
        [JsonPropertyName("Port")] public int Port { get; set; }
        [JsonPropertyName("PortUdp")] public int PortListen { get; set; }
        [JsonPropertyName("User")] public string User { get; set; }
        [JsonPropertyName("Password")] public string Password { get; set; }
        [JsonPropertyName("Realm")] public string Realm { get; set; }
        [JsonPropertyName("Secret")] public string Secret { get; set; }
        /// <summary>
        /// <see cref="ConnectionForStun.TCP"/>
        /// <see cref="ConnectionForStun.UDP"/>
        /// </summary>
        [JsonPropertyName("Protocol")] public int Protocol { get; set; }
        [JsonPropertyName("Software")] public string Software { get; set; }
        [JsonPropertyName("AuthType")] public int AuthTypeValue { get; set; }

        public long unixTimestamp;
        public string usercombo;
        public string passwordCombo;
        public byte[] secretAsBytes;
        public byte[] usercomboAsBytes;
        public byte[] passwordComboAsBytes;
        public EAuthType AuthType => (EAuthType)AuthTypeValue;
        //public byte[] HA1;

        public static StunConnectionConfig FromJsonString(string str)
        {
            var v = JsonSerializer.Deserialize<StunConnectionConfig>(str);
            return v;
        }

        public StunConnectionConfig ShallowCopy()
        {
            return MemberwiseClone() as StunConnectionConfig;
        }
    }
}
