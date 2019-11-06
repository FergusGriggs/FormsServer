using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Game
    {
        private int _type;
        private int _state;

        private Client _client1;
        private Client _client2;

        private String _clientInput1;
        private String _clientInput2;

        private int _clientScore1;
        private int _clientScore2;

        public Game(int type, Client inviteSender, Client inviteReciever)
        {
            _type = type;
            _state = 0;
            _clientScore1 = 0;
            _clientScore2 = 0;
            _client1 = inviteSender;
            _client2 = inviteReciever;
        }

        public int GetState()
        {
            return _state;
        }
        public int GetGameType()
        {
            return _type;
        }

        public Client GetClient1()
        {
            return _client1;
        }

        public Client GetClient2()
        {
            return _client2;
        }

        public void Close()
        {
            _client1.SetCurrentGame(null);
            _client2.SetCurrentGame(null);
        }

        public void Start()
        {
            _state = 1;
        }
    }
}
