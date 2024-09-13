using System;
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
            {
                sharedCoordinates.Checked = true;
            }
            else
            {
                internalOrigin.Checked = true;
            }

            crsInput.Enabled = sharedCoordinates.Checked;
            crsInput.Text = this.preferences.EpsgCode;

            instancing.Checked = this.preferences.Instancing;
            materials.Checked = this.preferences.Materials;
            textures.Checked = this.preferences.Textures;
            links.Checked = this.preferences.Links;

            maxTextureSize.Text = this.preferences.MaxTextureSize.ToString();
            maxTextureSize.Enabled = textures.Checked && materials.Checked;

            textures.Enabled = materials.Checked;
        }

        private readonly Preferences preferences;

        private void ExportButton_Click(object sender, EventArgs e)
        {
            this.preferences.SharedCoordinates = sharedCoordinates.Checked;
            this.preferences.TrueNorth = sharedCoordinates.Checked; // For now, true north only be used with shared coordinates
            this.preferences.Instancing = instancing.Checked;
            this.preferences.EpsgCode = crsInput.Text;
            this.preferences.Materials = materials.Checked;
            this.preferences.Textures = textures.Checked;
            this.preferences.Links = links.Checked;
            this.preferences.MaxTextureSize = int.Parse(maxTextureSize.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SharedCoordinates_CheckedChanged(object sender, EventArgs e)
        {
            crsInput.Enabled = sharedCoordinates.Checked;
        }

        private void CrsInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '+')
            {
                e.Handled = true;
            }
        }

        private void Textures_CheckedChanged(object sender, EventArgs e)
        {
            maxTextureSize.Enabled = textures.Checked;
        }

        private void Materials_CheckedChanged(object sender, EventArgs e)
        {
            textures.Enabled = materials.Checked;
            maxTextureSize.Enabled = textures.Checked && materials.Checked;
        }
    }
}
