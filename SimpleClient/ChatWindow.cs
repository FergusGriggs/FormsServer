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

        private delegate void ClearChatWindowDelegate();
        private ClearChatWindowDelegate _clearChatWindowDelegate;

        private SimpleClient _client;
        private Color _defaultColour = Color.FromArgb(170, 170, 170);

        public ChatWindow(SimpleClient client)
        {
            InitializeComponent();

            _updateChatWindowDelegate = new UpdateChatWindowDelegate(UpdateChatWindow);
            _clearChatWindowDelegate = new ClearChatWindowDelegate(ClearChatWindow);

            _client = client;
            if (messageBox.CanSelect) {
                messageBox.Select();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            _client.TCPSendPacket(new PacketData.ChatMessagePacket(messageBox.Text));
            //_client.SendMessage(messageBox.Text);
            messageBox.Text = "";
        }

        public static Color GetColourFromHex(String hexstring)
        {
            int r, g, b;

            if (int.TryParse(hexstring.Substring(0, 2), System.Globalization.NumberStyles.HexNumber,new System.Globalization.CultureInfo("en-US"), out r) &&
                int.TryParse(hexstring.Substring(2, 2), System.Globalization.NumberStyles.HexNumber,new System.Globalization.CultureInfo("en-US"), out g) &&
                int.TryParse(hexstring.Substring(4, 2), System.Globalization.NumberStyles.HexNumber,new System.Globalization.CultureInfo("en-US"), out b)
                )
            {
                return Color.FromArgb(r, g, b);
            }
            return Color.White;
        }

        public void UpdateChatWindow(String message)
        {
            try
            {
                if (chatWindowBox.InvokeRequired)
                {
                    Invoke(_updateChatWindowDelegate, message);
                }
                else
                {
                    for (int i = 0; i < message.Length; i++)
                    {
                        if (message[i] == '<' && i < message.Length - 7)
                        {
                            Color textColour = GetColourFromHex(message.Substring(i + 1, 6));
                            
                            if (textColour == Color.White)
                            {
                                chatWindowBox.AppendText(message.Substring(i, 1), _defaultColour);
                                continue;
                            }

                            bool foundClose = false;

                            for (int j = i + 7; j < message.Length; j++)
                            {
                                if (message[j] == '>')
                                {
                                    foundClose = true;
                                    String ColouredMessage = message.Substring(i + 7, j - (i + 7));
                                    chatWindowBox.AppendText(ColouredMessage, textColour);
                                    i = j;
                                    break;
                                }
                            }

                            if (!foundClose)
                            {
                                chatWindowBox.AppendText(message.Substring(i, 1), _defaultColour);
                            }
                        }
                        else
                        {
                            chatWindowBox.AppendText(message.Substring(i, 1), _defaultColour);
                        }
                    }

                    chatWindowBox.AppendText("\n");
                    
                    chatWindowBox.SelectionStart = chatWindowBox.Text.Length;
                    chatWindowBox.ScrollToCaret();
                }
            }
            catch (System.InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ClearChatWindow()
        {
            try
            {
                if (chatWindowBox.InvokeRequired)
                {
                    Invoke(_clearChatWindowDelegate);
                }
                else
                {
                    chatWindowBox.Text = "";

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
                _client.TCPSendPacket(new PacketData.ChatMessagePacket(messageBox.Text.Substring(0, messageBox.Text.Length - 1)));
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

        private void chatWindowBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }
    }
}
