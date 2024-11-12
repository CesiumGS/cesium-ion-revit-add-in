namespace CesiumIonRevitAddin.Forms
{
    partial class IonUploadDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IonUploadDialog));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.openAssetBtn = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.progressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 38);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(310, 28);
            this.progressBar.TabIndex = 0;
            // 
            // openAssetBtn
            // 
            this.openAssetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openAssetBtn.AutoSize = true;
            this.openAssetBtn.Location = new System.Drawing.Point(133, 79);
            this.openAssetBtn.Name = "openAssetBtn";
            this.openAssetBtn.Size = new System.Drawing.Size(108, 23);
            this.openAssetBtn.TabIndex = 1;
            this.openAssetBtn.Text = "Open in Cesium ion";
            this.openAssetBtn.UseVisualStyleBackColor = true;
            this.openAssetBtn.Click += new System.EventHandler(this.openAsset_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.AutoSize = true;
            this.closeBtn.Location = new System.Drawing.Point(247, 79);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 2;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // progressLabel
            // 
            this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressLabel.Location = new System.Drawing.Point(12, 9);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(310, 26);
            this.progressLabel.TabIndex = 3;
            this.progressLabel.Text = "Preparing upload...";
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // IonUploadDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 114);
            this.ControlBox = false;
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.openAssetBtn);
            this.Controls.Add(this.progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IonUploadDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uploading to Cesium ion";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button openAssetBtn;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Label progressLabel;
    }
}