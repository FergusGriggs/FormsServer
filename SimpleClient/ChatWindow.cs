using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleClient
{
    public partial class ChatWindow : Form
    {
        private delegate void UpdateChatWindowDelegate(String message);
        private UpdateChatWindowDelegate _updateChatWindowDelegate;
        private SimpleClient _client;

        public ChatWindow(SimpleClient client)
        {
            InitializeComponent();
            _updateChatWindowDelegate = new UpdateChatWindowDelegate(UpdateChatWindow);
            _client = client;
            if (messageBox.CanSelect) {
                messageBox.Select();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            _client.SendPacket(new PacketData.ChatMessagePacket(messageBox.Text));
            //_client.SendMessage(messageBox.Text);
            messageBox.Text = "";
        }

        public void UpdateChatWindow(String message)
        {
            try
            {
                if (chatWindowBox.InvokeRequired)
                {
                    //Invoke(_updateChatWindowDelegate, new object[] { message });
                    Invoke(_updateChatWindowDelegate, message);
                }
                else
                {
                    int boundCheck = 11;
                    if (message.Length < boundCheck)
                    {
                        boundCheck = message.Length;
                    }
                    int nameLength = 0;
                    for (int i = 0; i < boundCheck; i++)
                    {
                        if (message[i] == ':')
                        {
                            nameLength = i;
                        }
                    }

                    if (nameLength > 0)
                    {
                        chatWindowBox.AppendText("[" + message.Substring(0, nameLength) + "]", Color.FromArgb(170, 170, 0));
                        chatWindowBox.AppendText(message.Substring(nameLength) + "\n", Color.FromArgb(170, 170, 170));
                    }
                    else
                    {
                        chatWindowBox.AppendText(message + "\n", Color.FromArgb(170, 170, 170));
                    }
                    
                    chatWindowBox.SelectionStart = chatWindowBox.Text.Length;
                    chatWindowBox.ScrollToCaret();
                }
            }
            catch (System.InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _client.Close();
        }

        private void messageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _client.SendPacket(new PacketData.ChatMessagePacket(messageBox.Text.Substring(0, messageBox.Text.Length - 1)));
                //_client.SendMessage(messageBox.Text.Substring(0, messageBox.Text.Length - 1));
                messageBox.Text = "";
            }
        }

        private void ChatWindow_Load(object sender, EventArgs e)
        {
            _client.StartListener();
        }

        private void ChatWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_client._connected)
            {
                _client.Close();
            }
            
        }
    }
}
