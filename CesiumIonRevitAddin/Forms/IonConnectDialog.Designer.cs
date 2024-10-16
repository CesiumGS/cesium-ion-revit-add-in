namespace CesiumIonRevitAddin.Forms
{
    partial class IonConnectDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IonConnectDialog));
            this.connectBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.saasRadioBtn = new System.Windows.Forms.RadioButton();
            this.selfHostedRadioBtn = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.apiLabel = new System.Windows.Forms.Label();
            this.redirectLabel = new System.Windows.Forms.Label();
            this.serverLabel = new System.Windows.Forms.Label();
            this.apiUrlText = new System.Windows.Forms.TextBox();
            this.clientLabel = new System.Windows.Forms.Label();
            this.ionUrlText = new System.Windows.Forms.TextBox();
            this.clientIDText = new System.Windows.Forms.TextBox();
            this.redirectUrlText = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectBtn
            // 
            this.connectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.connectBtn.AutoSize = true;
            this.connectBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.connectBtn.Location = new System.Drawing.Point(287, 210);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(57, 23);
            this.connectBtn.TabIndex = 1;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.AutoSize = true;
            this.cancelBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cancelBtn.Location = new System.Drawing.Point(350, 210);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(50, 23);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // saasRadioBtn
            // 
            this.saasRadioBtn.AutoSize = true;
            this.saasRadioBtn.Checked = true;
            this.saasRadioBtn.Location = new System.Drawing.Point(6, 19);
            this.saasRadioBtn.Name = "saasRadioBtn";
            this.saasRadioBtn.Size = new System.Drawing.Size(76, 17);
            this.saasRadioBtn.TabIndex = 3;
            this.saasRadioBtn.TabStop = true;
            this.saasRadioBtn.Text = "Cesium ion";
            this.saasRadioBtn.UseVisualStyleBackColor = true;
            // 
            // selfHostedRadioBtn
            // 
            this.selfHostedRadioBtn.AutoSize = true;
            this.selfHostedRadioBtn.Location = new System.Drawing.Point(6, 42);
            this.selfHostedRadioBtn.Name = "selfHostedRadioBtn";
            this.selfHostedRadioBtn.Size = new System.Drawing.Size(134, 17);
            this.selfHostedRadioBtn.TabIndex = 4;
            this.selfHostedRadioBtn.Text = "Cesium ion Self Hosted";
            this.selfHostedRadioBtn.UseVisualStyleBackColor = true;
            this.selfHostedRadioBtn.CheckedChanged += new System.EventHandler(this.selfHostedRadioBtn_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.apiLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.redirectLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.serverLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.apiUrlText, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.clientLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.ionUrlText, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.clientIDText, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.redirectUrlText, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 65);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(376, 104);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // apiLabel
            // 
            this.apiLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.apiLabel.AutoSize = true;
            this.apiLabel.Location = new System.Drawing.Point(0, 26);
            this.apiLabel.Margin = new System.Windows.Forms.Padding(0);
            this.apiLabel.Name = "apiLabel";
            this.apiLabel.Size = new System.Drawing.Size(40, 26);
            this.apiLabel.TabIndex = 6;
            this.apiLabel.Text = "API Url";
            // 
            // redirectLabel
            // 
            this.redirectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.redirectLabel.AutoSize = true;
            this.redirectLabel.Location = new System.Drawing.Point(0, 78);
            this.redirectLabel.Margin = new System.Windows.Forms.Padding(0);
            this.redirectLabel.Name = "redirectLabel";
            this.redirectLabel.Size = new System.Drawing.Size(72, 26);
            this.redirectLabel.TabIndex = 2;
            this.redirectLabel.Text = "Redirect URL";
            // 
            // serverLabel
            // 
            this.serverLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(0, 0);
            this.serverLabel.Margin = new System.Windows.Forms.Padding(0);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(54, 26);
            this.serverLabel.TabIndex = 0;
            this.serverLabel.Text = "Server Url";
            // 
            // apiUrlText
            // 
            this.apiUrlText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.apiUrlText.Location = new System.Drawing.Point(75, 29);
            this.apiUrlText.Name = "apiUrlText";
            this.apiUrlText.Size = new System.Drawing.Size(298, 20);
            this.apiUrlText.TabIndex = 6;
            // 
            // clientLabel
            // 
            this.clientLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clientLabel.AutoSize = true;
            this.clientLabel.Location = new System.Drawing.Point(0, 52);
            this.clientLabel.Margin = new System.Windows.Forms.Padding(0);
            this.clientLabel.Name = "clientLabel";
            this.clientLabel.Size = new System.Drawing.Size(47, 26);
            this.clientLabel.TabIndex = 1;
            this.clientLabel.Text = "Client ID";
            // 
            // ionUrlText
            // 
            this.ionUrlText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ionUrlText.Location = new System.Drawing.Point(75, 3);
            this.ionUrlText.Name = "ionUrlText";
            this.ionUrlText.Size = new System.Drawing.Size(298, 20);
            this.ionUrlText.TabIndex = 3;
            // 
            // clientIDText
            // 
            this.clientIDText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientIDText.Location = new System.Drawing.Point(75, 55);
            this.clientIDText.Name = "clientIDText";
            this.clientIDText.Size = new System.Drawing.Size(298, 20);
            this.clientIDText.TabIndex = 4;
            // 
            // redirectUrlText
            // 
            this.redirectUrlText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.redirectUrlText.Location = new System.Drawing.Point(75, 81);
            this.redirectUrlText.Name = "redirectUrlText";
            this.redirectUrlText.Size = new System.Drawing.Size(298, 20);
            this.redirectUrlText.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.saasRadioBtn);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.selfHostedRadioBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(388, 188);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cesium ion Server";
            // 
            // IonConnectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 245);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.connectBtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IonConnectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to Cesium ion";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IonConnectDialog_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.RadioButton saasRadioBtn;
        private System.Windows.Forms.RadioButton selfHostedRadioBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Label redirectLabel;
        private System.Windows.Forms.Label clientLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox ionUrlText;
        private System.Windows.Forms.TextBox clientIDText;
        private System.Windows.Forms.TextBox redirectUrlText;
        private System.Windows.Forms.Label apiLabel;
        private System.Windows.Forms.TextBox apiUrlText;
    }
}