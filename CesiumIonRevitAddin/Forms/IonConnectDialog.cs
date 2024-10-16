using CesiumIonRevitAddin.CesiumIonClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Autodesk.Revit.UI;
using System.Threading;

namespace CesiumIonRevitAddin.Forms
{
    public partial class IonConnectDialog : Form
    {
        private CancellationTokenSource cts;
        private int port;

        public IonConnectDialog()
        {
            InitializeComponent();
            setTextFieldsEnabled(selfHostedRadioBtn.Checked);

            // Use a randomly available port for the redirect URI
            TcpListener portListener = new TcpListener(IPAddress.Loopback, 0);
            portListener.Start();
            port = ((IPEndPoint)portListener.LocalEndpoint).Port;
            portListener.Stop();
        }

        private async void connectBtn_Click(object sender, EventArgs e)
        {
            // Standard Cesium ion OAuth2 parameters
            string ionServerUrl = "https://ion.cesium.com/";
            string apiServerUrl = "https://api.cesium.com/";
            string responseType = "code";
            string clientID = "847";
            string redirectUrl = "http://127.0.0.1/cesium-ion-revit-addin/oauth2/callback";
            string scope = "assets:write";

            if (selfHostedRadioBtn.Checked)
            {
                ionServerUrl = ionUrlText.Text;
                apiServerUrl = apiUrlText.Text;
                clientID = clientIDText.Text;
                redirectUrl = redirectUrlText.Text;
            }

            UriBuilder uriBuilder = new UriBuilder(new Uri(redirectUrl))
            {
                Port = port
            };
            redirectUrl = uriBuilder.Uri.ToString();

            // Cancel the existing connection if another is started
            if (cts != null)
                cts.Cancel();

            cts = new CancellationTokenSource();

            ConnectionResult result = await Connection.ConnectToIon(ionServerUrl, apiServerUrl, responseType, clientID, redirectUrl, scope, cts.Token);

            if (result.Status == ConnectionStatus.Success)
            {
                TaskDialog.Show("Connected", result.Message);
                this.Close();
            }
            else if (result.Status == ConnectionStatus.Failure)
            {
                TaskDialog.Show("Connection Failed", result.Message);
            }
            else if (result.Status == ConnectionStatus.Cancelled)
            {
                // Do nothing
            }
            
            
        }

        private void selfHostedRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            setTextFieldsEnabled(selfHostedRadioBtn.Checked);
        }

        private void setTextFieldsEnabled(bool enabled)
        {
            ionUrlText.Enabled = enabled;
            apiUrlText.Enabled = enabled;
            clientIDText.Enabled = enabled;
            redirectUrlText.Enabled = enabled;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void IonConnectDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel any existing connection task that hasn't completed
            if (cts != null)
                cts.Cancel();
        }
    }
}
