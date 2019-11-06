using System;
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
        private String _newPlayerInfo;

        private System.IO.MemoryStream _memoryStream;
        private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _binaryFormatter;

        public Server(String ipAddress, int port)
        {
            _port = port;
            _address = System.Net.IPAddress.Parse(ipAddress);
            _tcpListener = new System.Net.Sockets.TcpListener(_address, _port);

            _memoryStream = new System.IO.MemoryStream();
            _binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            _clients = new List<Client>();
            _games = new List<Game>();
            _gameNames = new List<String>();
            _gameNames.Add("rps");

            _newPlayerInfo = "Welcome to the server!\n\nRules\n\n1.) Be nice you little shit.\n\nCommands\n\n/pm username message - Private message\n/play username game_name - Play a game\n/rename new_name - Renames you\n/mute username - Mutes User\n/unmute username - Unmutes User\n\nOnline Clients\n\n";

        }

        public void SendPacket(PacketData.Packet packet, Client client)
        {
            _memoryStream.SetLength(0);
            _binaryFormatter.Serialize(_memoryStream, packet);
            byte[] buffer = _memoryStream.GetBuffer();
            _memoryStream.SetLength(0);

            client._binaryWriter.Write(buffer.Length);
            client._binaryWriter.Write(buffer);
            client._binaryWriter.Flush();
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
                _memoryStream.Write(buffer, 0, numOfIncomingBytes);
                _memoryStream.Position = 0;
                PacketData.Packet rawPacket = _binaryFormatter.Deserialize(_memoryStream) as PacketData.Packet;
                _memoryStream.SetLength(0);

                switch (rawPacket.type)
                {
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
                                SendPacket(sendPacket, _clients[i]);
                            }
                        }
                        break;
                }
            }
            //while ((recievedMessage = client._reader.ReadLine()) != null)
            //{
            //    recievers.Clear();
            //    messages.Clear();

            //    GetRefinedMessage(recievedMessage, client.GetID(), ref recievers, ref messages);

            //    bool sendToAll = false;
            //    if (recievers.Count == 0) sendToAll = true;

            //    if (sendToAll)
            //    {
            //        String returnMessage = messages[0];

            //        for (int i = 0; i < _clients.Count; i++)
            //        {
            //            _clients[i]._writer.WriteLine(returnMessage);
            //            _clients[i]._writer.Flush();
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < recievers.Count; i++)
            //        {
            //            String returnMessage = messages[i];

            //            _clients[recievers[i]]._writer.WriteLine(returnMessage);
            //            _clients[recievers[i]]._writer.Flush();
            //        }
            //    }


            //    if (recievedMessage == "end") break;
            //}

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
            if (!_clients[clientID].RenamedBefore())
            {
                _clients[clientID].Renamed();

                Console.WriteLine(newName + " connected.");

                for (int i = 0; i < _clients.Count; i++)
                {
                    recievers.Add(i);
                    if (i != clientID)
                    {
                        messages.Add(newName + " has joined the server.");
                    }
                    else
                    {
                        String newPlayerMessage = _newPlayerInfo;
                        for (int j = 0; j < _clients.Count; j++)
                        {
                            if (j != clientID)
                            {
                                newPlayerMessage += _clients[j].GetName() + "\n";
                            }
                            else
                            {
                                newPlayerMessage += _clients[j].GetName() + "(You)";
                            }
                        }
                        newPlayerMessage += "\n";
                        messages.Add(newPlayerMessage);
                    }

                }
                return;
            }
            else
            {
                messages.Add("Client '" + previousName + "' is now called '" + newName + "'.");
                return;
            }
        }
        private void ProcessPlayCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String gameName = commandParameters[0];
            String opponentName = commandParameters[1];

            bool foundGame = false;
            int gameID = -1;
            int opponentID = -1;

            if (gameName == "rps")
            {
                foundGame = true;
                gameID = 0;
            }

            if (foundGame)
            {
                bool foundUser = false;

                if (_clients[clientID].GetName() == opponentName)
                {
                    recievers.Add(clientID);
                    messages.Add("You cannot play with yourself.");
                    return;
                }

                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i].GetName() == opponentName)
                    {
                        foundUser = true;
                        opponentID = i;
                        recievers.Add(i);
                        messages.Add(_clients[clientID].GetName() + " has challenged you to a game of " + gameName + ".\nUse /accept " + _clients[clientID].GetName() + " to accept.");
                    }
                }

                if (foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("You have challenged " + opponentName + " to a game of " + gameName + ".");
                    _games.Add(new Game(gameID, _clients[clientID], _clients[opponentID]));
                    return;
                }
                else
                {
                    recievers.Add(clientID);
                    messages.Add("That client does not exist.");
                    return;
                }
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("That game does not exist.");
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
                    messages.Add("You have accepted the " + gameName + " challenge from " + _clients[opponentID].GetName() + "!");
                    recievers.Add(opponentID);
                    messages.Add(_clients[clientID].GetName() + " has accepted your " + gameName + " challenge!");
                }
                else
                {
                    recievers.Add(clientID);
                    messages.Add("That client has not invited you.");
                }
                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("That client does not exist.");
                return;
            }
        }
        private void ProcessPrivateMessageCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String recipientName = commandParameters[0];
            String message = commandParameters[1];

            if (_clients[clientID].GetName() == recipientName)
            {
                recievers.Add(clientID);
                messages.Add("You cannot pm yourself.");
                return;
            }

            bool foundUser = false;
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].GetName() == recipientName)
                {
                    foundUser = true;
                    recievers.Add(i);
                    messages.Add(_clients[clientID].GetName() + " whispered to you: " + message);
                }
            }

            if (foundUser)
            {
                recievers.Add(clientID);
                messages.Add("You whispered to " + recipientName + ": " + message);
                return;
            }
            else
            {
                recievers.Add(clientID);
                messages.Add("That client does not exist.");
                return;
            }
        }

        private void ProcessMuteCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String userToMute = commandParameters[0];
            if (userToMute == "*")
            {
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (clientID != i)
                    {
                        _clients[clientID].AddMutedClient(_clients[i]);
                    }
                }

                recievers.Add(clientID);
                messages.Add("You have muted all users. (Why are you even here?)");
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
                            messages.Add("You cannot mute yourself.");
                            return;
                        }
                        else if (_clients[clientID].AddMutedClient(_clients[i]))
                        {
                            recievers.Add(clientID);
                            messages.Add("You muted " + userToMute + ".");
                            return;
                        }
                        else
                        {
                            recievers.Add(clientID);
                            messages.Add(userToMute + " is already muted.");
                            return;
                        }

                    }
                }
                if (!foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("That client does not exist.");
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
                messages.Add("You have unmuted all users.");
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
                            messages.Add("You cannot unmute yourself.");
                            return;
                        }
                        else if (_clients[clientID].RemoveMutedClient(_clients[i]))
                        {
                            recievers.Add(clientID);
                            messages.Add("You have unmuted " + userToUnmute + ".");
                            return;
                        }
                        else
                        {
                            recievers.Add(clientID);
                            messages.Add(userToUnmute + " is not muted.");
                            return;
                        }

                    }
                }
                if (!foundUser)
                {
                    recievers.Add(clientID);
                    messages.Add("That client does not exist.");
                    return;
                }
            }
        }

        private void ProcessEndConnectionCommand(List<String> commandParameters, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            String name = _clients[clientID].GetName();
            SendPacket(new PacketData.ChatMessagePacket("TERMINATE"), _clients[clientID]);

            Console.WriteLine(name + " disconnected.");

            for (int i = 0; i < _games.Count; i++)
            {
                if (_games[i].GetClient1().GetID() == clientID || _games[i].GetClient2().GetID() == clientID)
                {
                    _games[i].Close();
                    _games.RemoveAt(i);
                    break;
                }
            }

            System.Threading.Thread clientListenerThread = _clients[clientID]._listenerThread;

            _clients.Remove(_clients[clientID]);

            for (int i = clientID; i < _clients.Count; i++)
            {
                _clients[i].SetID(_clients[i].GetID() - 1);
            }

            messages.Add(name + " left the server.");

            clientListenerThread.Abort();

            return;
        }
        private void GetRefinedMessage(String rawMessage, int clientID, ref List<int> recievers, ref List<String> messages)
        {
            if (rawMessage.Length > 0)
            {
                if (rawMessage.Substring(0, 1) == "/")
                {
                    String commandFull = rawMessage.Substring(1);

                    String commandType = "";
                    String commandContent = "";

                    for (int i = 0; i < commandFull.Length; i++)
                    {
                        if (commandFull[i] == ' ')
                        {
                            commandType = commandFull.Substring(0, i);
                            commandContent = commandFull.Substring(i + 1);
                            break;
                        }
                    }

                    List<String> commandParameters = new List<String>();

                    SplitString(commandContent, ref commandParameters);

                    if (commandType == "rename")
                    {
                        if (commandParameters.Count == 1)
                        {
                            ProcessRenameCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("No name given. Correct syntax is: /rename new_name");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /rename new_name");
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
                            messages.Add("No parameters given. Correct syntax is: /play game_name opponent_name");
                        }
                        else if (commandParameters.Count == 1)
                        {
                            messages.Add("No opponent given. Correct syntax is: /play game_name opponent_name");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /play game_name opponent_name");
                        }
                        recievers.Add(clientID);
                        return;
                    }
                    else if (commandType == "pm")
                    {
                        if (commandParameters.Count == 2)
                        {
                            ProcessPrivateMessageCommand(commandParameters, clientID, ref recievers, ref messages);
                            return;
                        }
                        else if (commandParameters.Count == 0)
                        {
                            messages.Add("No parameters given. Correct syntax is: /pm recipient_name message");
                        }
                        else if (commandParameters.Count == 1)
                        {
                            messages.Add("No message given. Correct syntax is: /pm recipient_name message");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /pm recipient_name message");
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
                            messages.Add("No name given. Correct syntax is: /mute username");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /mute username");
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
                            messages.Add("No name given. Correct syntax is: /unmute username");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /unmute username");
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
                            messages.Add("No name given. Correct syntax is: /accept username");
                        }
                        else
                        {
                            messages.Add("Too many parameters given. Correct syntax is: /accept username");
                        }
                        recievers.Add(clientID);
                        return;

                    }
                    else if (commandFull == "endconnection")
                    {
                        ProcessEndConnectionCommand(commandParameters, clientID, ref recievers, ref messages);
                        return;
                    }
                    else
                    {
                        recievers.Add(clientID);
                        messages.Add("Unknown Command: " + commandFull);
                        return;
                    }
                }
                else
                {
                    for (int i = 0; i < _clients.Count; i++)
                    {
                        if (!_clients[i].GetMuted(_clients[clientID]))
                        {
                            recievers.Add(i);
                            messages.Add(_clients[clientID].GetName() + ": " + rawMessage);
                        }
                    }

                    return;
                }
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
