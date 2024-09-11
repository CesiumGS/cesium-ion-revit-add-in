namespace CesiumIonRevitAddin.Forms
{
    partial class ExportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportDialog));
            this.Georeferencing = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.crsInput = new System.Windows.Forms.TextBox();
            this.internalOrigin = new System.Windows.Forms.RadioButton();
            this.sharedCoordinates = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.maxTextureSizeLabel = new System.Windows.Forms.Label();
            this.maxTextureSize = new System.Windows.Forms.ComboBox();
            this.links = new System.Windows.Forms.CheckBox();
            this.textures = new System.Windows.Forms.CheckBox();
            this.materials = new System.Windows.Forms.CheckBox();
            this.instancing = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.Georeferencing.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Georeferencing
            // 
            this.Georeferencing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Georeferencing.Controls.Add(this.label1);
            this.Georeferencing.Controls.Add(this.crsInput);
            this.Georeferencing.Controls.Add(this.internalOrigin);
            this.Georeferencing.Controls.Add(this.sharedCoordinates);
            this.Georeferencing.Location = new System.Drawing.Point(12, 12);
            this.Georeferencing.Name = "Georeferencing";
            this.Georeferencing.Size = new System.Drawing.Size(309, 80);
            this.Georeferencing.TabIndex = 0;
            this.Georeferencing.TabStop = false;
            this.Georeferencing.Text = "Georeferencing";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(161, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "EPSG";
            // 
            // crsInput
            // 
            this.crsInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.crsInput.Location = new System.Drawing.Point(203, 19);
            this.crsInput.Name = "crsInput";
            this.crsInput.Size = new System.Drawing.Size(100, 20);
            this.crsInput.TabIndex = 2;
            this.crsInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CrsInput_KeyPress);
            // 
            // internalOrigin
            // 
            this.internalOrigin.AutoSize = true;
            this.internalOrigin.Location = new System.Drawing.Point(7, 44);
            this.internalOrigin.Name = "internalOrigin";
            this.internalOrigin.Size = new System.Drawing.Size(90, 17);
            this.internalOrigin.TabIndex = 1;
            this.internalOrigin.TabStop = true;
            this.internalOrigin.Text = "Internal Origin";
            this.internalOrigin.UseVisualStyleBackColor = true;
            // 
            // sharedCoordinates
            // 
            this.sharedCoordinates.AutoSize = true;
            this.sharedCoordinates.Location = new System.Drawing.Point(7, 20);
            this.sharedCoordinates.Name = "sharedCoordinates";
            this.sharedCoordinates.Size = new System.Drawing.Size(118, 17);
            this.sharedCoordinates.TabIndex = 0;
            this.sharedCoordinates.TabStop = true;
            this.sharedCoordinates.Text = "Shared Coordinates";
            this.sharedCoordinates.UseVisualStyleBackColor = true;
            this.sharedCoordinates.CheckedChanged += new System.EventHandler(this.SharedCoordinates_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.maxTextureSizeLabel);
            this.groupBox1.Controls.Add(this.maxTextureSize);
            this.groupBox1.Controls.Add(this.links);
            this.groupBox1.Controls.Add(this.textures);
            this.groupBox1.Controls.Add(this.materials);
            this.groupBox1.Controls.Add(this.instancing);
            this.groupBox1.Location = new System.Drawing.Point(12, 98);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(309, 111);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // maxTextureSizeLabel
            // 
            this.maxTextureSizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxTextureSizeLabel.AutoSize = true;
            this.maxTextureSizeLabel.Location = new System.Drawing.Point(147, 66);
            this.maxTextureSizeLabel.Name = "maxTextureSizeLabel";
            this.maxTextureSizeLabel.Size = new System.Drawing.Size(50, 13);
            this.maxTextureSizeLabel.TabIndex = 5;
            this.maxTextureSizeLabel.Text = "Max Size";
            // 
            // maxTextureSize
            // 
            this.maxTextureSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxTextureSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxTextureSize.FormattingEnabled = true;
            this.maxTextureSize.Items.AddRange(new object[] {
            "4096",
            "2048",
            "1024"});
            this.maxTextureSize.Location = new System.Drawing.Point(203, 63);
            this.maxTextureSize.Name = "maxTextureSize";
            this.maxTextureSize.Size = new System.Drawing.Size(100, 21);
            this.maxTextureSize.TabIndex = 4;
            // 
            // links
            // 
            this.links.AutoSize = true;
            this.links.Location = new System.Drawing.Point(6, 88);
            this.links.Name = "links";
            this.links.Size = new System.Drawing.Size(79, 17);
            this.links.TabIndex = 3;
            this.links.Text = "Revit Links";
            this.links.UseVisualStyleBackColor = true;
            // 
            // textures
            // 
            this.textures.AutoSize = true;
            this.textures.Location = new System.Drawing.Point(6, 65);
            this.textures.Name = "textures";
            this.textures.Size = new System.Drawing.Size(67, 17);
            this.textures.TabIndex = 2;
            this.textures.Text = "Textures";
            this.textures.UseVisualStyleBackColor = true;
            this.textures.CheckedChanged += new System.EventHandler(this.Textures_CheckedChanged);
            // 
            // materials
            // 
            this.materials.AutoSize = true;
            this.materials.Location = new System.Drawing.Point(6, 42);
            this.materials.Name = "materials";
            this.materials.Size = new System.Drawing.Size(68, 17);
            this.materials.TabIndex = 1;
            this.materials.Text = "Materials";
            this.materials.UseVisualStyleBackColor = true;
            this.materials.CheckedChanged += new System.EventHandler(this.Materials_CheckedChanged);
            // 
            // instancing
            // 
            this.instancing.AutoSize = true;
            this.instancing.Location = new System.Drawing.Point(6, 19);
            this.instancing.Name = "instancing";
            this.instancing.Size = new System.Drawing.Size(75, 17);
            this.instancing.TabIndex = 0;
            this.instancing.Text = "Instancing";
            this.instancing.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(246, 225);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Location = new System.Drawing.Point(165, 225);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 3;
            this.exportButton.Text = "Save";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ExportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 260);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Georeferencing);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportDialog";
            this.Text = "Export to 3D Tiles";
            this.Georeferencing.ResumeLayout(false);
            this.Georeferencing.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox Georeferencing;
        private System.Windows.Forms.RadioButton internalOrigin;
        private System.Windows.Forms.RadioButton sharedCoordinates;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox instancing;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox crsInput;
        private System.Windows.Forms.CheckBox textures;
        private System.Windows.Forms.CheckBox materials;
        private System.Windows.Forms.CheckBox links;
        private System.Windows.Forms.Label maxTextureSizeLabel;
        private System.Windows.Forms.ComboBox maxTextureSize;
    }
}