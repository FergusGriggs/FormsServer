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
                            _client.SetConnectionSucessful(true);
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
    }
}
