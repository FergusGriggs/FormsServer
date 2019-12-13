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
    public partial class ServerConnect : Form
    {
        private delegate void UpdateErrorLogWindowDelegate(String message);
        private UpdateErrorLogWindowDelegate _updateErrorLogWindowDelegate;
        private SimpleClient _client;

        public ServerConnect(SimpleClient client)
        {
            _updateErrorLogWindowDelegate = new UpdateErrorLogWindowDelegate(UpdateErrorLogWindow);
            _client = client;
            InitializeComponent();
            usernameBox.Text = _client.GetName();
            ipBox.Text = _client.GetIP();
            portBox.Text = _client.GetPort().ToString();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            AttemptConnect();
        }

        public void UpdateErrorLogWindow(String message)
        {
            if (errorLogBox.InvokeRequired)
            {
                Invoke(_updateErrorLogWindowDelegate, new object[] { message });
            }
            else
            {
                errorLogBox.Text += message + "\n";
                errorLogBox.SelectionStart = errorLogBox.Text.Length;
                errorLogBox.ScrollToCaret();
            }
        }

        private void usernameBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                usernameBox.Text = usernameBox.Text.Substring(0, usernameBox.Text.Length - 1);
                AttemptConnect();
            }
        }

        private void AttemptConnect()
        {
            if (usernameBox.Text.Length > 2)
            {
                if (ipBox.Text.Length > 0)
                {
                    if (portBox.Text.Length == 4)
                    {
                        UpdateErrorLogWindow("Connecting...");

                        _client.SetUsername(usernameBox.Text);
                        _client.SetIP(ipBox.Text);
                        _client.SetPort(Int32.Parse(portBox.Text));

                        if (_client.TCPConnect())
                        {
                            _client.SetWindowCloseAction(WindowCloseAction.OPEN_CHAT_WINDOW);
                            this.Close();
                        }
                        else
                        {
                            UpdateErrorLogWindow("Connection Failed.");
                        }
                    }
                    else
                    {
                        UpdateErrorLogWindow("Please enter a valid port.");
                    }
                }
                else
                {
                    UpdateErrorLogWindow("Please enter an IP.");
                }
            }
            else
            {
                UpdateErrorLogWindow("Username must be three characters or more.");
            }
        }

        private void ServerConnect_FormClosed(object sender, FormClosedEventArgs e)
        {
            _client.SetWindowCloseAction(WindowCloseAction.CLOSE_APP);
        }
    }
}
