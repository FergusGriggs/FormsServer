using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Client
    {
        private System.Net.Sockets.Socket _socket;
        private System.Net.Sockets.NetworkStream _stream;
        //public System.IO.StreamReader _reader;
        //public System.IO.StreamWriter _writer;

        public System.IO.BinaryReader _binaryReader;
        public System.IO.BinaryWriter _binaryWriter;

        private int _ID;
        private String _name;
        private bool _renamedBefore;
        private Game _currentGame;
        private List<Client> _mutedUsers;

        public System.Threading.Thread _listenerThread;

        public Client(System.Net.Sockets.Socket socket, int id)
        {
            _ID = id;
            _name = _ID.ToString();
            _socket = socket;
            _stream = new System.Net.Sockets.NetworkStream(_socket);

            //_reader = new System.IO.StreamReader(_stream);
            //_writer = new System.IO.StreamWriter(_stream);

            _binaryReader = new System.IO.BinaryReader(_stream);
            _binaryWriter = new System.IO.BinaryWriter(_stream);

            _renamedBefore = false;

            _currentGame = null;

            _mutedUsers = new List<Client>();
        }

        public void Close()
        {
            _socket.Close();
        }

        public int GetID()
        {
            return _ID;
        }

        public void SetID(int ID)
        {
            _ID = ID;
        }
        public String GetName()
        {
            return _name;
        }

        public void SetName(String newName)
        {
            _name = newName;
        }

        public bool RenamedBefore()
        {
            return _renamedBefore;
        }

        public void Renamed()
        {
            _renamedBefore = true;
        }

        public void SetCurrentGame(Game game)
        {
            _currentGame = game;
        }

        public Game GetCurrentGame()
        {
            return _currentGame;
        }

        public bool AddMutedClient(Client client)
        {
            for (int i = 0; i < _mutedUsers.Count; i++)
            {
                if (_mutedUsers[i] == client)
                {
                    return false;
                }
            }
            _mutedUsers.Add(client);
            return true;
        }

        public bool RemoveMutedClient(Client client)
        {
            for (int i = 0; i < _mutedUsers.Count; i++)
            {
                if (_mutedUsers[i] == client)
                {
                    _mutedUsers.Remove(client);
                    return true;
                }
            }
            return false;
        }

        public bool GetMuted(Client client)
        {
            for (int i = 0; i < _mutedUsers.Count; i++)
            {
                if (_mutedUsers[i] == client)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
