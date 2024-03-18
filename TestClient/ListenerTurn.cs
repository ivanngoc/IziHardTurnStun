using IziHardGames.STUN;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace IziHardGames
{

    class ListenerTurn
    {
        public struct Message
        {
            public string nonce;
            public string realm;
            public string address;
            public byte[] con_id;
        }
        public Queue<Message> messages;
        public Queue<CoturnClient> clients;
        public ListenerTurn()
        {
            messages = new Queue<Message>();
            clients = new Queue<CoturnClient>();
        }

        public async void Listen(CoturnClient turnClient)
        {
            throw new NotImplementedException();
        }
    }
}
