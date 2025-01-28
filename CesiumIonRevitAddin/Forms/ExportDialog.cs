using System;
using System.Text.RegularExpressions;
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

#if REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022
            instancing.Checked = false;
            instancing.Enabled = false;
#else
            instancing.Checked = this.preferences.IonInstancing;
#endif

            materials.Checked = this.preferences.Materials;
            normals.Checked = this.preferences.Normals;
            textures.Checked = this.preferences.Textures;
            links.Checked = this.preferences.Links;
            metadata.Checked = this.preferences.ExportMetadata;

            maxTextureSize.Text = this.preferences.MaxTextureSize.ToString();
            maxTextureSize.Enabled = textures.Checked && materials.Checked;

            textures.Enabled = materials.Checked;
        }

        private readonly Preferences preferences;

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.crsInput.Text) && !ValidateInputCRS(this.crsInput.Text))
            {
                Autodesk.Revit.UI.TaskDialog.Show("Invalid EPSG Code", "Please enter a valid 4- or 5-digit EPSG code in the range 1024 to 32767.  For combined CRS, use the format 'EPSG+EPSG', where both codes are within the valid range.");
                return;
            }

            this.preferences.SharedCoordinates = sharedCoordinates.Checked;
            this.preferences.TrueNorth = sharedCoordinates.Checked; // For now, true north only be used with shared coordinates
            this.preferences.IonInstancing = instancing.Checked;
            this.preferences.EpsgCode = crsInput.Text;
            this.preferences.Materials = materials.Checked;
            this.preferences.Normals = normals.Checked;
            this.preferences.Textures = textures.Checked;
            this.preferences.Links = links.Checked;
            this.preferences.ExportMetadata = metadata.Checked;
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

        private static bool ValidateInputCRS(string inputCRS)
        {
            var regex = new Regex(@"^(\d{4,5})(\+(\d{4,5}))?$");
            var match = regex.Match(inputCRS);

            if (!match.Success) return false;

            int horizontal = int.Parse(match.Groups[1].Value);
            int vertical = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : -1;

            bool isHorizontalValid = horizontal >= 1024 && horizontal <= 32767;
            bool isVerticalValid = vertical == -1 || (vertical >= 1024 && vertical <= 32767);

            return isHorizontalValid && isVerticalValid;    
        }
    }
}
