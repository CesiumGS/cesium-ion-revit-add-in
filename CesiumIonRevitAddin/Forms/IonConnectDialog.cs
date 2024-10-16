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

namespace CesiumIonRevitAddin.Forms
{
    public partial class IonConnectDialog : Form
    {
        public IonConnectDialog()
        {
            InitializeComponent();
            setTextFieldsEnabled(selfHostedRadioBtn.Checked);
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

            // TODO: Handle use cases of multiple connection button clicks and unfinished tasks
            bool success = await Connection.ConnectToIon(ionServerUrl, apiServerUrl, responseType, clientID, redirectUrl, scope);

            if (success)
                TaskDialog.Show("Connected", "Successfully connected to Cesium ion");
            else
                TaskDialog.Show("Connection Failed", "Could not connect to Cesium ion");

            this.Close();
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
    }
}
