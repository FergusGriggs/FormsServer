using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SimpleClient
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }

    public class SimpleClient
    {
        private System.Net.Sockets.TcpClient _tcpClient;
        private System.Net.Sockets.NetworkStream _stream;

        //private System.IO.StreamReader _reader;
        //private System.IO.StreamWriter _writer;

        private System.IO.BinaryReader _binaryReader;
        private System.IO.BinaryWriter _binaryWriter;

        private System.IO.MemoryStream _memoryStream;
        private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _binaryFormatter;

        private System.Threading.Thread _listenerThread;
        private ServerConnect _serverConnectForm;
        private ChatWindow _chatWindow;

        private String _name = "Fergus";
        private String _IP = "127.0.0.1";
        private int _port = 4444;

        private bool _connectSucessful;
        public bool _appRunning;
        public bool _connected;
        public SimpleClient()
        {
            _memoryStream = new System.IO.MemoryStream();
            _binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            _connectSucessful = false;

            _appRunning = true;

            _connected = false;

            while (_appRunning)
            {
                _tcpClient = new System.Net.Sockets.TcpClient();

                _serverConnectForm = new ServerConnect(this);

                Application.Run(_serverConnectForm);

                if (_connectSucessful)
                {
                    _connected = true;

                    _chatWindow = new ChatWindow(this);

                    Application.Run(_chatWindow);
                }
                else
                {
                    break;
                }
            }
        }

        public void SendPacket(PacketData.Packet packet)
        {
            _binaryFormatter.Serialize(_memoryStream, packet);
            _memoryStream.Flush();
            byte[] buffer = _memoryStream.GetBuffer();
            _memoryStream.SetLength(0);

            _binaryWriter.Write(buffer.Length);
            _binaryWriter.Write(buffer);
            _binaryWriter.Flush();
        }

        public bool Connect()
        {
            try
            {
                _tcpClient.Connect(_IP, _port);
                _stream = _tcpClient.GetStream();

                //_reader = new System.IO.StreamReader(_stream);
                //_writer = new System.IO.StreamWriter(_stream);

                _binaryReader = new System.IO.BinaryReader(_stream);
                _binaryWriter = new System.IO.BinaryWriter(_stream);

                return true;

            }
            catch (Exception e)
            {
                _serverConnectForm.UpdateErrorLogWindow("Exception: " + e.Message);

                return false;
            }
        }

        public void StartListener()
        {
            //SendMessage("/rename " + _name);
            SendPacket(new PacketData.ChatMessagePacket("/rename " + _name));
            _listenerThread = new System.Threading.Thread(ListenerProgram);
            _listenerThread.Start();
        }

        public void ListenerProgram()
        {
            while (true) {
                ProcessServerResponse();
            }
        }

        public void SendMessage(String message)
        {
            //_writer.WriteLine(message);
            //_writer.Flush();
        }


        public void Close()
        {
            SendPacket(new PacketData.ChatMessagePacket("/endconnection"));
            //SendMessage("/endconnection");

            while (_listenerThread.IsAlive)
            {

            }
            FullyClose();
        }

        public void FullyClose()
        {
            _connected = false;

            _tcpClient.Close();

            _chatWindow.Close();

            _stream.Close();

            //_reader.Close();

            //_writer.Close();
        }

        private void ProcessServerResponse()
        {
            int numOfIncomingBytes = 0;
            while((numOfIncomingBytes = _binaryReader.ReadInt32()) != 0)
            {
                byte[] buffer = _binaryReader.ReadBytes(numOfIncomingBytes);
                _memoryStream.Write(buffer, 0, numOfIncomingBytes);
                _memoryStream.Position = 0;
                PacketData.Packet rawPacket = _binaryFormatter.Deserialize(_memoryStream) as PacketData.Packet;
                _memoryStream.SetLength(0);

                switch (rawPacket.type) {
                    case PacketData.PacketType.CHAT_MESSAGE:
                        PacketData.ChatMessagePacket packet = (PacketData.ChatMessagePacket)rawPacket;
                        if (packet.message == "TERMINATE")
                        {
                            _listenerThread.Abort();
                        }
                        else if (packet.message == "CLEAR")
                        {
                            _chatWindow.ClearChatWindow();
                        }

                        else
                        {
                            _chatWindow.UpdateChatWindow(packet.message);
                        }
                       
                        break;
                }
            }

            //String messageRecieved = _reader.ReadLine();
        }

        public String GetName()
        {
            return _name;
        }

        public String GetIP()
        {
            return _IP;
        }

        public int GetPort()
        {
            return _port;
        }

        public void SetUsername(String username)
        {
            _name = username;
        }
        public void SetIP(String IP)
        {
            _IP = IP;
        }
        public void SetPort(int port)
        {
            _port = port;
        }

        public void SetConnectionSucessful(bool connectionSucessful)
        {
            _connectSucessful = connectionSucessful;
        }

        //private string GetGameResult()
        //{
        //    switch (_myAnswer)
        //    {
        //        case "R":
        //            switch (_otherPlayerInput)
        //            {
        //                case "R":
        //                    return "DRAW";
        //                case "P":
        //                    return "YOU LOSE";
        //                case "S":
        //                    return "YOU WIN";
        //                default:
        //                    return "Other Player doesn't know how to play...";
        //            }
        //        case "P":
        //            switch (_otherPlayerInput)
        //            {
        //                case "R":
        //                    return "YOU WIN";
        //                case "P":
        //                    return "DRAW";
        //                case "S":
        //                    return "YOU LOSE";
        //                default:
        //                    return "Other Player doesn't know how to play...";
        //            }
        //        case "S":
        //            switch (_otherPlayerInput)
        //            {
        //                case "R":
        //                    return "YOU LOSE";
        //                case "P":
        //                    return "YOU WIN";
        //                case "S":
        //                    return "DRAW";
        //                default:
        //                    return "Other Player doesn't know how to play...";
        //            }
        //        default:
        //            return "Wrong input dummy";
        //    }
        //}
    }
}
