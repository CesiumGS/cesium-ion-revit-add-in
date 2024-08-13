using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CesiumIonRevitAddin.Forms
{
    public partial class ExportDialog : Form
    {
        public ExportDialog(ref Preferences preferences)
        {
            InitializeComponent();
            this.preferences = preferences;

            // Set the default UI state to match preferences
            if (this.preferences.SharedCoordinates)
                sharedCoordinates.Checked = true;
            else
                internalOrigin.Checked = true;

            instancing.Checked = this.preferences.Instancing;
        }

        Preferences preferences;

        private void exportButton_Click(object sender, EventArgs e)
        {
            
            this.preferences.SharedCoordinates = sharedCoordinates.Checked;
            this.preferences.TrueNorth = sharedCoordinates.Checked; // For now, true north only be used with shared coordinates
            this.preferences.Instancing = instancing.Checked;


            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
