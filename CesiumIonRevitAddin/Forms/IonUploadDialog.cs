using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CesiumIonRevitAddin.CesiumIonClient;
using Autodesk.Revit.UI;


namespace CesiumIonRevitAddin.Forms
{
    public partial class IonUploadDialog : Form
    {
        string assetUrl;
        readonly string zipPath;
        readonly string assetName;
        readonly string assetDesc;
        readonly string inputCrs;

        public IonUploadDialog(string zipPath, string assetName, string assetDesc, string inputCrs)
        {
            InitializeComponent();
            this.zipPath = zipPath; 
            this.assetName = assetName;
            this.assetDesc = assetDesc;
            this.inputCrs = inputCrs;

            // Disable buttons until the upload is complete or fails
            openAssetBtn.Enabled = false;
            closeBtn.Enabled = false;

            // Start the upload process
            StartUpload();
        }

        private void StartUpload()
        {
            // Create a progress handler that will report progress to UpdateProgress
            var progressHandler = new Progress<double>(percent =>
            {
                UpdateProgress(percent);
            });

            // Start the upload task in a separate thread
            Task.Run(async () =>
            {
                var result = await Connection.Upload(
                    this.zipPath,
                    this.assetName,
                    this.assetDesc,
                    "",
                    "3DTILES",
                    "BIM_CAD",
                    this.inputCrs,
                    progressHandler);

                // Invoke UI updates on the main thread
                this.Invoke(new Action(() =>
                {
                    if (result.Status == ConnectionStatus.Success)
                    {
                        assetUrl = result.Message; // Assuming Message contains the asset URL
                        SetUploadComplete();
                    }
                    else
                    {
                        TaskDialog.Show("Upload failed", result.Message); // Now called on the UI thread
                        this.Close();
                    }
                }));
            });
        }


        private void openAsset_Click(object sender, EventArgs e)
        {
            // Open asset URL in the default browser (if assetUrl is valid)
            if (!string.IsNullOrEmpty(assetUrl))
            {
                Connection.OpenBrowser(assetUrl);
            }
        }

        // Method to update the progress bar
        public void UpdateProgress(double percent)
        {
            if (progressBar.InvokeRequired)
            {
                // Ensure that the UI update happens on the main thread
                progressBar.Invoke(new Action(() => progressBar.Value = (int)percent));
            }
            else
            {
                progressBar.Value = (int)percent;
            }

            string uploadText = progressLabel.Text = $"Uploading to Cesium ion... {(int)percent}%";

            if (progressLabel.InvokeRequired)
            {                 
                progressLabel.Invoke(new Action(() => progressLabel.Text = uploadText));
            }
            else
            {
                progressLabel.Text = uploadText;
            }
        }

        public void SetUploadComplete()
        {
            closeBtn.Enabled = true;

            // Enable the open asset button if the asset URL is valid
            openAssetBtn.Enabled = !string.IsNullOrEmpty(assetUrl);

            progressLabel.Text = "Upload complete!";
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
