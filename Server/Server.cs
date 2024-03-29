﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Server
    {
        private System.Net.IPAddress _address;
        private int _port;

        private System.Net.Sockets.TcpListener _tcpListener;

        private List<Client> _clients;
        private List<Game> _games;

        private List<String> _gameNames;

        private String _newPlayerInfo = "";

        private String _welcomeMessage = "Copyright Fergus Griggs - https://fergusgriggs.co.uk\n\n<00AA00Welcome to the server!>\n\n";

        private String _ruleMessage = "<FF00FFRules>\n\n" +
            "<FF44001.)> Thou shall not be impolite.\n" +
            "<FF44002.)> Thou shall not hack.\n" +
            "<FF44003.)> Thou shall view my YouTube channel.\n" +
            "<FF44004.)> Thou shall subscribe to my YouTube channel.\n" +
            "<FF44005.)> Thou shall comment and like my videos.\n" +
            "\n";

        private String _commandMessage = "<FF00FFCommands>\n\n" +
            "<FF4400/pm username message> - Private message\n" +
            "<FF4400/play username game_name> - Play a game\n" +
            "<FF4400/accept username> - Accepts a game invitation\n" +
            "<FF4400/decline username> - Declines a game invitation\n" +
            "<FF4400/rename new_name> - Renames you\n" +
            "<FF4400/mute username> - Mutes User\n" +
            "<FF4400/unmute username> - Unmutes User\n" +
            "<FF4400/clear> - Clears this box\n" +
            "<FF4400/rules> - Displays rules\n" +
            "<FF4400/help> - Displays commands\n" +
            "<FF4400/users> - Displays connected users\n" +
            "\n";

        //private System.IO.MemoryStream memoryStream;
        //private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter;

        public Server(String ipAddress, int port)
        {
            _port = port;
            _address = System.Net.IPAddress.Parse(ipAddress);
            _tcpListener = new System.Net.Sockets.TcpListener(_address, _port);

            //memoryStream = new System.IO.MemoryStream();
            //binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            _clients = new List<Client>();
            _games = new List<Game>();
            _gameNames = new List<String>();

            _gameNames.Add("rps");
            _gameNames.Add("bman");

            _newPlayerInfo = _welcomeMessage + _ruleMessage + _commandMessage;

        }

        public void SendPacket(PacketData.Packet packet, Client client)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            memoryStream.SetLength(0);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            memoryStream.Position = 0;
            binaryFormatter.Serialize(memoryStream, packet);
            byte[] buffer = memoryStream.GetBuffer();

            client._binaryWriter.Write(buffer.Length);
            client._binaryWriter.Write(buffer);
        }

        public void Start()
        {
            _tcpListener.Start();
            Console.WriteLine("I am a server and am listening... ");
            while (true)
            {
                System.Net.Sockets.Socket socket = _tcpListener.AcceptSocket();
                Client newClient = new Client(socket, _clients.Count);

                newClient._listenerThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ClientMethod));
                newClient._listenerThread.Name = newClient.GetName();
                newClient._listenerThread.Start(newClient);

                _clients.Add(newClient);
            }
        }

        public void Stop()
        {
            _tcpListener.Stop();
            Console.WriteLine("Stopped Listening");
        }

        private void ClientMethod(object clientObj)
        {
            Client client = (Client)clientObj;

            List<int> recievers = new List<int>();
            List<String> messages = new List<String>();

            int numOfIncomingBytes = 0;
            while ((numOfIncomingBytes = client._binaryReader.ReadInt32()) != 0)
            {
                byte[] buffer = client._binaryReader.ReadBytes(numOfIncomingBytes);
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                memoryStream.Position = 0;
                memoryStream.Write(buffer, 0, numOfIncomingBytes);
                memoryStream.Position = 0;
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                PacketData.Packet rawPacket = binaryFormatter.Deserialize(memoryStream) as PacketData.Packet;
                memoryStream.SetLength(0);

                switch (rawPacket.type)
                {
                    case PacketData.PacketType.LOGIN:
                        PacketData.LogInPacket loginPacket = (PacketData.LogInPacket)rawPacket;
                        Console.WriteLine(loginPacket.username + " connected.");

                        client.SetName(loginPacket.username);

                        for (int i = 0; i < _clients.Count; i++)
                        {
                            recievers.Add(i);
                            if (i != client.GetID())
                            {
                                messages.Add("<AAAA00[" + loginPacket.username + "]><777700 has joined the server>");
                            }
                            else
                            {
                                String newPlayerMessage = _newPlayerInfo + GetCurrentUsers(client.GetID());
                                messages.Add(newPlayerMessage);
                            }
                        }

                        for (int i = 0; i < recievers.Count; i++)
                        {
                            PacketData.ChatMessagePacket sendPacket = new PacketData.ChatMessagePacket(messages[i]);
                            SendPacket(sendPacket, _clients[recievers[i]]);
                        }

                        break;
                    case PacketData.PacketType.END_CONNECTION:
                        List<String> commandParams = new List<String>();
                        ProcessEndConnectionCommand(commandParams, client.GetID(), ref recievers, ref messages);
                        break;
                    case PacketData.PacketType.BOMBERMAN_CLIENT_TO_SERVER:
                        PacketData.BombermanClientToServerPacket bombermanClientToServerPacket = (PacketData.BombermanClientToServerPacket)rawPacket;
                        if (client.GetCurrentGame().GetClient1() == client)
                        {
                            SendPacket(new PacketData.BombermanServerToClientPacket(bombermanClientToServerPacket.bombsPlaced, bombermanClientToServerPacket.player), client.GetCurrentGame().GetClient2());
                            //SendPacket(new PacketData.BombermanServerToClientPacket(bombermanClientToServerPacket.player), client.GetCurrentGame().GetClient2());
                        }
                        else
                        {
                            SendPacket(new PacketData.BombermanServerToClientPacket(bombermanClientToServerPacket.bombsPlaced, bombermanClientToServerPacket.player), client.GetCurrentGame().GetClient1());
                            //SendPacket(new PacketData.BombermanServerToClientPacket(bombermanClientToServerPacket.player), client.GetCurrentGame().GetClient1());
                        }
                        break;
                    case PacketData.PacketType.CHAT_MESSAGE:
                        PacketData.ChatMessagePacket packet = (PacketData.ChatMessagePacket)rawPacket;

                        recievers.Clear();
                        messages.Clear();

                        GetRefinedMessage(packet.message, client.GetID(), ref recievers, ref messages);

                        bool sendToAll = false;
                        if (recievers.Count == 0) sendToAll = true;

                        if (sendToAll)
                        {
                            PacketData.ChatMessagePacket sendPacket = new PacketData.ChatMessagePacket(messages[0]);

                            for (int i = 0; i < _clients.Count; i++)
                            {
                                SendPacket(sendPacket, _clients[i]);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < recievers.Count; i++)
                            {
                                PacketData.ChatMessagePacket sendPacket = new PacketData.ChatMessagePacket(messages[i]);
                                SendPacket(sendPacket, _clients[recievers[i]]);
                            }
                        }
                        break;
                }

                if (_clients[client.GetID()].GetShouldTerminate())
                {
                    System.Threading.Thread listenerThread = _clients[client.GetID()]._listenerThread;
                    _clients.Remove(_clients[client.GetID()]);

                    for (int i = client.GetID(); i < _clients.Count; i++)
                    {
                        _clients[i].SetID(_clients[i].GetID() - 1);
                    }

                    listenerThread.Abort();
                }
            }
            client.Close();

        }

        private void SplitString(String stringToSplit, ref List<String> substringList)
        {
            int charsSinceSplitChar = 0;
            for (int i = 0; i < stringToSplit.Length; i++)
            {
                if (stringToSplit[i] == ' ')
                {
                    if (charsSinceSplitChar > 0)
                    {
                        substringList.Add(stringToSplit.Substring(i - charsSinceSplitChar, charsSinceSplitChar));
                    }

                    charsSinceSplitChar = -1;
                }
                charsSinceSplitChar++;
            }
            if (charsSinceSplitChar > 0)
            {
                substringList.Add(stringToSplit.Substring(stringToSplit.Length - charsSinceSplitChar, charsSinceSplitChar));
            }
        }

        private string GetRPSFullName(char RPSChar)
        {
            switch (RPSChar)
            {
                case 'R':
                    return "rock";
                case 'P':
                    return "paper";
                case 'S':
                    return "scissors";
                default:
                    return "invalid";
            }
        }
        private void ProcessGameInputRPS(String input, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            char inputChar = input.ToUpper()[0];
            if (inputChar == 'R' || inputChar == 'P' || inputChar == 'S')
            {
                bool alreadyPlayed = false;
                bool isClient1 = false;
                Game clientGame = _clients[clientID].GetCurrentGame();
                if (clientGame.GetClient1() == _clients[clientID])
                {
                    isClient1 = true;
                    if (!clientGame.GetClient1Inputted())
                    {
                        clientGame.SetClient1Input(inputChar.ToString());
                    }
                    else
                    {
                        alreadyPlayed = true;
                    }
                }
                else
                {
                    if (!clientGame.GetClient2Inputted())
                    {
                        clientGame.SetClient2Input(inputChar.ToString());
                    }
                    else
                    {
                        alreadyPlayed = true;
                    }
                }

                if (alreadyPlayed)
                {
                    recievers.Add(clientID);
                    messages.Add("You've already played num nutz");
                }
                else
                {
                    if (clientGame.GetClient1Inputted() && clientGame.GetClient2Inputted())
                    {
                        String client1Message = "", client2Message = "";

                        clientGame.GetRPSResult(ref client1Message, ref client2Message);

                        if (isClient1)
                        {
                            recievers.Add(clientID);
                            messages.Add("<777700You played " + GetRPSFullName(inputChar) + ">\n" + client1Message);

                            recievers.Add(clientGame.GetClient2().GetID());
                            messages.Add(client2Message);
                        }
                        else
                        {
                            recievers.Add(clientGame.GetClient1().GetID());
                            messages.Add(client1Message);

                            recievers.Add(clientID);
                            messages.Add("<777700You played " + GetRPSFullName(inputChar) + ">\n" + client2Message);
                        }
                        clientGame.NextRound();
                    }
                    else
                    {
                        recievers.Add(clientID);
                        messages.Add("<777700You played " + GetRPSFullName(inputChar) + ">");
                    }
                }
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("Invalid Input my dude");
            }
        }

        private void ProcessRenameCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String previousName = _clients[clientID].GetName();
            String newName = commandParameters[0];

            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].GetName() == newName)
                {
                    newName += "(2)";
                }
            }

            _clients[clientID].SetName(newName);

            messages.Add("<AAAA00[" + previousName + "]><777700 is now ><AAAA00[" + newName + "]>");
            return;
        }

        private String GetCurrentUsers(int clientID)
        {
            String _currentUsers = "<FF00FFCurrent Users>\n\n";
            for (int j = 0; j < _clients.Count; j++)
            {
                if (j != clientID)
                {
                    _currentUsers += "<AAAA00[" + _clients[j].GetName() + "]>\n";
                }
                else
                {
                    _currentUsers += "<AAAA00[" + _clients[j].GetName() + "]><777700(You)\n>";
                }
            }
            _currentUsers += "\n";
            return _currentUsers;
        }
        private void ProcessPlayCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            for (int i = 0; i < _games.Count; i++)
            {
                if (_games[i].GetClient1() == _clients[clientID] || _games[i].GetClient2() == _clients[clientID])
                {
                    if (_games[i].GetState() == 1)
                    {
                        recievers.Add(clientID);
                        messages.Add("<FF4400You're already in a game. Use /exitgame to leave>");
                        return;
                    }
                }
            }

            String gameName = commandParameters[0];
            String opponentName = commandParameters[1];

            bool foundGame = false;
            int gameType = -1;
            int opponentID = -1;

            if (gameName == "rps")
            {
                foundGame = true;
                gameType = 0;
            }
            else if (gameName == "bman")
            {
                foundGame = true;
                gameType = 1;
            }

            if (foundGame)
            {
                bool foundUser = false;

                if (_clients[clientID].GetName() == opponentName)
                {
                    recievers.Add(clientID);
                    messages.Add("<FF4400You cannot play with yourself>");
                    return;
                }

                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i].GetName() == opponentName)
                    {
                        foundUser = true;
                        opponentID = i;
                        recievers.Add(i);
                        messages.Add("<AAAA00[" + _clients[clientID].GetName() + "]><777700 has challenged you to a game of ><FF00FF[" + gameName + "]>\n<777700Use /accept ><AAAA00[" + _clients[clientID].GetName() + "]><777700 to accept\nUse /decline ><AAAA00[" + _clients[clientID].GetName() + "]><777700 to decline>");
                    }
                }

                if (foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("<777700You have challenged ><AAAA00[" + opponentName + "]><777700 to a game of ><FF00FF[" +  gameName + "]>");
                    _games.Add(new Game(gameType, _clients[clientID], _clients[opponentID]));
                    return;
                }
                else
                {
                    recievers.Add(clientID);
                    messages.Add("<FF4400That client does not exist>");
                    return;
                }
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("<FF4400That game does not exist>");
                return;
            }
        }
        private void ProcessAcceptCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String opponentName = commandParameters[0];

            bool foundUser = false;
            int opponentID = -1;

            for (int j = 0; j < _clients.Count; j++)
            {
                if (_clients[j].GetName() == opponentName)
                {
                    foundUser = true;
                    opponentID = j;
                    break;
                }
            }

            if (foundUser)
            {
                bool foundGame = false;
                int gameID = -1;
                String gameName = "";

                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].GetClient1() == _clients[clientID] || _games[i].GetClient2() == _clients[clientID])
                    {

                        if (_games[i].GetClient1() == _clients[opponentID] || _games[i].GetClient2() == _clients[opponentID])
                        {
                            gameID = i;
                            foundGame = true;
                            gameName = _gameNames[_games[gameID].GetGameType()];
                            break;
                        }
                    }
                }

                if (foundGame)
                {
                    _games[gameID].Start();
                    recievers.Add(clientID);
                    messages.Add("<777700You have accepted the ><FF00FF[" + gameName + "]><777700 challenge from ><AAAA00[" + _clients[opponentID].GetName() + "]>");
                    recievers.Add(opponentID);
                    messages.Add("<AAAA00[" + _clients[clientID].GetName() + "]><777700 has accepted your ><FF00FF[" + gameName + "]><777700 challenge>");

                    if (_games[gameID].GetGameType() == 1)
                    {
                        SendPacket(new PacketData.OpenBombermanWindowPacket(true), _clients[clientID]);
                        SendPacket(new PacketData.OpenBombermanWindowPacket(false), _clients[opponentID]);
                    }
                }
                else
                {
                    recievers.Add(clientID);
                    messages.Add("<AAAA00[" + _clients[opponentID].GetName() + "]><FF4400 has not invited you>");
                }
                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("<FF4400That client does not exist>");
                return;
            }
        }
        private void ProcessDeclineCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String opponentName = commandParameters[0];

            bool foundUser = false;
            int opponentID = -1;

            for (int j = 0; j < _clients.Count; j++)
            {
                if (_clients[j].GetName() == opponentName)
                {
                    foundUser = true;
                    opponentID = j;
                    break;
                }
            }

            if (foundUser)
            {
                bool foundGame = false;
                int gameID = -1;
                String gameName = "";

                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].GetClient1() == _clients[clientID] || _games[i].GetClient2() == _clients[clientID])
                    {

                        if (_games[i].GetClient1() == _clients[opponentID] || _games[i].GetClient2() == _clients[opponentID])
                        {
                            gameID = i;
                            foundGame = true;
                            gameName = _gameNames[_games[gameID].GetGameType()];
                            break;
                        }
                    }
                }

                if (foundGame)
                {
                    _games[gameID].Close();
                    _games.RemoveAt(gameID);

                    recievers.Add(clientID);
                    messages.Add("<777700You have declined the ><FF00FF[" +  gameName + "]><777700 challenge from ><AAAA00[" + _clients[opponentID].GetName() + "]>");
                    recievers.Add(opponentID);
                    messages.Add("<AAAA00[" + _clients[clientID].GetName() + "]><777700 has declined your ><FF00FF[" + gameName + "]><777700 challenge>");
                }
                else
                {
                    recievers.Add(clientID);
                    messages.Add("<AAAA00[" + _clients[opponentID].GetName() + "]><FF4400 has not invited you>");
                }
                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("<FF4400That client does not exist>");
                return;
            }
        }
        private void ProcessPrivateMessageCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String recipientName = commandParameters[0];
            String message = "";
            for (int i = 1; i < commandParameters.Count; i++)
            {
                message += commandParameters[i] + " ";
            }

            if (_clients[clientID].GetName() == recipientName)
            {
                recievers.Add(clientID);
                messages.Add("<FF4400You cannot pm yourself>");
                return;
            }

            bool foundUser = false;
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].GetName() == recipientName)
                {
                    foundUser = true;
                    recievers.Add(i);
                    messages.Add("<AAAA00[" + _clients[clientID].GetName() + "]><777700 whispered to you: >" + message);
                }
            }

            if (foundUser)
            {
                recievers.Add(clientID);
                messages.Add("<777700You whispered to ><AAAA00[" + recipientName + "]><777700:> " + message);
                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("<FF4400That client does not exist>");
                return;
            }
        }

        private void ProcessMuteCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String userToMute = commandParameters[0];
            if (userToMute == "all")
            {
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (clientID != i)
                    {
                        _clients[clientID].AddMutedClient(_clients[i]);
                    }
                }

                recievers.Add(clientID);
                messages.Add("<777700You have muted all users (Why are you even here?)>");
                return;
            }
            else
            {
                bool foundUser = false;
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i].GetName() == userToMute)
                    {
                        foundUser = true;

                        if (_clients[i].GetID() == clientID)
                        {
                            recievers.Add(clientID);
                            messages.Add("<FF4400You cannot mute yourself>");
                            return;
                        }
                        else if (_clients[clientID].AddMutedClient(_clients[i]))
                        {
                            recievers.Add(clientID);
                            messages.Add("<777700You muted ><AAAA00[" +  userToMute + "]>");
                            return;
                        }
                        else
                        {
                            recievers.Add(clientID);
                            messages.Add("<AAAA00[" + userToMute + "]><FF4400 is already muted>");
                            return;
                        }

                    }
                }
                if (!foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("<FF4400That client does not exist>");
                    return;
                }
            }
        }

        private void ProcessUnmuteCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String userToUnmute = commandParameters[0];
            if (userToUnmute == "all")
            {
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (clientID != i)
                    {
                        _clients[clientID].RemoveMutedClient(_clients[i]);
                    }
                }

                recievers.Add(clientID);
                messages.Add("<777700You have unmuted all users>");
                return;
            }
            else
            {
                bool foundUser = false;
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i].GetName() == userToUnmute)
                    {
                        foundUser = true;

                        if (_clients[i].GetID() == clientID)
                        {
                            recievers.Add(clientID);
                            messages.Add("<FF4400You cannot unmute yourself>");
                            return;
                        }
                        else if (_clients[clientID].RemoveMutedClient(_clients[i]))
                        {
                            recievers.Add(clientID);
                            messages.Add("<777700You have unmuted ><AAAA00[" + userToUnmute + "]>");
                            return;
                        }
                        else
                        {
                            recievers.Add(clientID);
                            messages.Add("<AAAA00[" + userToUnmute + "]><FF4400 is not muted>");
                            return;
                        }

                    }
                }
                if (!foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("<FF4400That client does not exist>");
                    return;
                }
            }
        }

        private void ProcessClearCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            SendPacket(new PacketData.Packet(PacketData.PacketType.CLEAR_WINDOW), _clients[clientID]);
            messages.Add("<777700Window Cleared>");
            recievers.Add(clientID);
            return;
        }

        private void ProcessRulesCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            messages.Add(_ruleMessage);
            recievers.Add(clientID);
            return;
        }

        private void ProcessHelpCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            messages.Add(_commandMessage);
            recievers.Add(clientID);
            return;
        }

        private void ProcessUsersCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            messages.Add(GetCurrentUsers(clientID));
            recievers.Add(clientID);
            return;
        }

        private void ProcessExitGameCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            for (int i = 0; i < _games.Count; i++)
            {
                if (_games[i].GetClient1() == _clients[clientID] || _games[i].GetClient2() == _clients[clientID])
                {
                    if (_games[i].GetClient1() == _clients[clientID])
                    {
                        messages.Add("<FF4400You have left the game. ><777700You won " + _games[i].GetClient1Score() + " game(s). ><AAAA00[" + _games[i].GetClient2().GetName() + "]><777700 won " + _games[i].GetClient2Score() + " game(s)>");
                        recievers.Add(_games[i].GetClient1().GetID());

                        messages.Add("<AAAA00[" + _games[i].GetClient1().GetName() + "]><FF4400 has left the game. ><777700You won " + _games[i].GetClient2Score() + " game(s). ><AAAA00[" + _games[i].GetClient1().GetName() + "]><777700 won " + _games[i].GetClient1Score() + " game(s)>");
                        recievers.Add(_games[i].GetClient2().GetID());
                    }
                    else
                    {
                        messages.Add("<FF4400You have left the game. ><777700You won " + _games[i].GetClient2Score() + " game(s). ><AAAA00[" + _games[i].GetClient1().GetName() + "]><777700 won " + _games[i].GetClient1Score() + " game(s)>");
                        recievers.Add(_games[i].GetClient2().GetID());

                        messages.Add("<AAAA00["+_games[i].GetClient2().GetName()+ "]><FF4400 has left the game. ><777700You won " + _games[i].GetClient1Score() + " game(s). ><AAAA00[" + _games[i].GetClient2().GetName() + "]><777700 won " + _games[i].GetClient2Score() + " game(s)>");
                        recievers.Add(_games[i].GetClient1().GetID());
                    }
                    _games[i].Close();
                    _games.RemoveAt(i);
                    return;
                }
            }
        }

        private void ProcessEndConnectionCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String name = _clients[clientID].GetName();
            SendPacket(new PacketData.Packet(PacketData.PacketType.TERMINATE_CLIENT), _clients[clientID]);

            Console.WriteLine( name + " disconnected.");

            for (int i = 0; i < _games.Count; i++)
            {
                if (_games[i].GetClient1().GetID() == clientID || _games[i].GetClient2().GetID() == clientID)
                {
                    _games[i].Close();
                    _games.RemoveAt(i);
                    break;
                }
            }

            _clients[clientID].SetShouldTerminate(true);

            PacketData.ChatMessagePacket sendPacket = new PacketData.ChatMessagePacket("<AAAA00[" + name + "]><777700 left the server>");

            for (int i = 0; i < _clients.Count; i++)
            {
                SendPacket(sendPacket, _clients[i]);
            }

            return;
        }
        private void GetRefinedMessage(String rawMessage, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            if (rawMessage.Length > 0)
            {
                if (rawMessage.Substring(0, 1) == "/")
                {
                    String commandFull = rawMessage.Substring(1);
                    List<String> commandParameters = new List<String>();

                    SplitString(commandFull, ref commandParameters);

                    if (commandParameters.Count == 0)
                    {
                        messages.Add("<FF4400No command type given>");
                        recievers.Add(clientID);
                        return;
                    }

                    String commandType = commandParameters[0];
                    commandParameters.RemoveAt(0);

                    if (commandType == "rename")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessRenameCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No name given. Correct syntax is: /rename new_name>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /rename new_name>");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "play")
                    {
                        if (commandParameters.Count == 2)
                        {
                            ProcessPlayCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No parameters given. Correct syntax is: /play game_name opponent_name>");
                        }
                        else if (commandParameters.Count == 1)
                        {
                            messages.Add("<FF4400No opponent given. Correct syntax is: /play game_name opponent_name>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /play game_name opponent_name>");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "pm")
                    {
                        if (commandParameters.Count > 1)
                        {
                            ProcessPrivateMessageCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No parameters given. Correct syntax is: /pm recipient_name message>");
                        }
                        else if (commandParameters.Count == 1)
                        {
                            messages.Add("<FF4400No message given. Correct syntax is: /pm recipient_name message>");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "mute")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessMuteCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No name given. Correct syntax is: /mute username>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /mute username>");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "unmute")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessUnmuteCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No name given. Correct syntax is: /unmute username>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /unmute username>");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "accept")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessAcceptCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No name given. Correct syntax is: /accept username>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /accept username>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "decline")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessDeclineCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("<FF4400No name given. Correct syntax is: /decline username>");
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /decline username>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "clear")
                    {
                        if (commandParameters.Count == 0)
                        {
                            ProcessClearCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /clear>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "rules")
                    {
                        if (commandParameters.Count == 0)
                        {
                            ProcessRulesCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /clear>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "help")
                    {
                        if (commandParameters.Count == 0)
                        {
                            ProcessHelpCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /clear>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "users")
                    {
                        if (commandParameters.Count == 0)
                        {
                            ProcessUsersCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /clear>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandType == "exitgame")
                    {
                        if (commandParameters.Count == 0)
                        {
                            ProcessExitGameCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else
                        {
                            messages.Add("<FF4400Too many parameters given. Correct syntax is: /exitgame>");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else
                    {
                        recievers.Add(clientID);
                        messages.Add("<FF4400Unknown Command: " + commandFull + ">");
                        return;
                    }
                }
                else if (_clients[clientID].GetCurrentGame() != null)
                {
                    switch (_clients[clientID].GetCurrentGame().GetGameType())
                    {
                        case 0:
                            ProcessGameInputRPS(rawMessage, clientID, ref recievers, ref messages);
                            return;
                    }
                }
                
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (!_clients[i].GetMuted(_clients[clientID]))
                    {
                        recievers.Add(i);
                        messages.Add("<AAAA00[" + _clients[clientID].GetName() + "]><777700:> " + rawMessage);
                    }
                }

                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("Please refrain from sending nothing");
                return;
            }
        }
    }
}
