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

    public enum WindowCloseAction
    {
        OPEN_CHAT_WINDOW,
        OPEN_SERVER_CONNECT,
        CLOSE_APP
    }

    public class SimpleClient
    {
        private System.Net.Sockets.TcpClient _tcpClient;
        private System.Net.Sockets.NetworkStream _stream;

        private System.IO.BinaryReader _binaryReader;
        private System.IO.BinaryWriter _binaryWriter;

        //private System.IO.MemoryStream memoryStream;
        //private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter;

        private System.Threading.Thread _listenerThread;
        private System.Threading.Thread _gameWindowThread;

        private ServerConnect _serverConnectForm;
        private ChatWindow _chatWindow;
        private GameWindow _gameWindow;

        private String _name = "Fergus";
        private String _IP = "127.0.0.1";
        private int _port = 4444;

        Random RNG = new Random();

        public bool _connected;

        WindowCloseAction windowCloseAction;

        public SimpleClient()
        {
            //memoryStream = new System.IO.MemoryStream();
            //binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            _connected = false;

            windowCloseAction = WindowCloseAction.OPEN_SERVER_CONNECT;

            while (true)
            {
                if (windowCloseAction == WindowCloseAction.OPEN_SERVER_CONNECT)
                {
                    _tcpClient = new System.Net.Sockets.TcpClient();

                    _serverConnectForm = new ServerConnect(this);

                    Application.Run(_serverConnectForm);
                }
                else if (windowCloseAction == WindowCloseAction.OPEN_CHAT_WINDOW)
                {
                    _connected = true;

                    _chatWindow = new ChatWindow(this);

                    Application.Run(_chatWindow);
                }
                else if (windowCloseAction == WindowCloseAction.CLOSE_APP)
                {
                    break;
                }
            }
        }

        public void TCPSendPacket(PacketData.Packet packet)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            memoryStream.Position = 0;
            binaryFormatter.Serialize(memoryStream, packet);
            byte[] buffer = memoryStream.GetBuffer();

            _binaryWriter.Write(buffer.Length);
            _binaryWriter.Write(buffer);
        }

        public bool TCPConnect()
        {
            try
            {
                _tcpClient.Connect(_IP, _port);
                _stream = _tcpClient.GetStream();

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
            TCPSendPacket(new PacketData.LogInPacket(_name));
            _listenerThread = new System.Threading.Thread(ListenerProgram);
            _listenerThread.Start();
        }

        public void ListenerProgram()
        {
            while (true) {
                ProcessServerResponse();
            }
        }

        public void Close()
        {
            TCPSendPacket(new PacketData.Packet(PacketData.PacketType.END_CONNECTION));

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

            _binaryReader.Close();

            _binaryWriter.Close();
        }

        private void ProcessServerResponse()
        {
            int numOfIncomingBytes = 0;
            while((numOfIncomingBytes = _binaryReader.ReadInt32()) != 0)
            {
                byte[] buffer = _binaryReader.ReadBytes(numOfIncomingBytes);
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                memoryStream.Position = 0;
                memoryStream.Write(buffer, 0, numOfIncomingBytes);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                memoryStream.Position = 0;
                PacketData.Packet rawPacket = binaryFormatter.Deserialize(memoryStream) as PacketData.Packet;

                switch (rawPacket.type) {
                    case PacketData.PacketType.CHAT_MESSAGE:
                        PacketData.ChatMessagePacket chatPacket = (PacketData.ChatMessagePacket)rawPacket;
                        _chatWindow.UpdateChatWindow(chatPacket.message);
                        break;
                    case PacketData.PacketType.CLEAR_WINDOW:
                        _chatWindow.ClearChatWindow();
                        break;
                    case PacketData.PacketType.TERMINATE_CLIENT:
                        _listenerThread.Abort();
                        break;
                    case PacketData.PacketType.BOMBERMAN_SERVER_TO_CLIENT:
                        PacketData.BombermanServerToClientPacket bombermanServerToClientPacket = (PacketData.BombermanServerToClientPacket)rawPacket;
                        _gameWindow.UpdateOtherPlayerWithPacket(bombermanServerToClientPacket.otherPlayer);
                        _gameWindow.AddOtherPlayersBombs(bombermanServerToClientPacket.otherPlayerBombs);
                        break;
                    case PacketData.PacketType.OPEN_BOMBERMAN_WINDOW:
                        PacketData.OpenBombermanWindowPacket openBomberManWindowPacket = (PacketData.OpenBombermanWindowPacket)rawPacket;
                        _gameWindowThread = new System.Threading.Thread(() => RunBombermanGame(openBomberManWindowPacket.isPlayerOne));
                        _gameWindowThread.Start();
                        break;
                }
            }
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

        public void SetWindowCloseAction(WindowCloseAction windowCloseAction)
        {
            this.windowCloseAction = windowCloseAction;
        }

        private void RunBombermanGame(bool isPLayerOne)
        {
            _gameWindow = new GameWindow(this, isPLayerOne);
            _gameWindow.Show();

            while (_gameWindow.Visible)
            {
                Application.DoEvents();

                _gameWindow.UpdateGame();
                
                _gameWindow.Refresh();//Force a repaint
            }
        }
    }
}
