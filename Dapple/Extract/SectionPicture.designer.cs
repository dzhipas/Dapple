namespace Dapple.Extract
{
   partial class SectionPicture
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
			this.lFileName = new System.Windows.Forms.Label();
			this.tbFilename = new System.Windows.Forms.TextBox();
			this.oResolution = new Dapple.Extract.Resolution();
			this.lDisplayOptions = new System.Windows.Forms.Label();
			this.cbDisplayOptions = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// lFileName
			// 
			this.lFileName.AutoSize = true;
			this.lFileName.Location = new System.Drawing.Point(3, 6);
			this.lFileName.Name = "lFileName";
			this.lFileName.Size = new System.Drawing.Size(55, 13);
			this.lFileName.TabIndex = 0;
			this.lFileName.Text = "File name:";
			// 
			// tbFilename
			// 
			this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.tbFilename.Location = new System.Drawing.Point(104, 3);
			this.tbFilename.Name = "tbFilename";
			this.tbFilename.Size = new System.Drawing.Size(373, 20);
			this.tbFilename.TabIndex = 1;
			this.tbFilename.Validating += new System.ComponentModel.CancelEventHandler(this.tbFilename_Validating);
			// 
			// oResolution
			// 
			this.oResolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.oResolution.Location = new System.Drawing.Point(0, 29);
			this.oResolution.Name = "oResolution";
			this.oResolution.Size = new System.Drawing.Size(480, 105);
			this.oResolution.TabIndex = 2;
			// 
			// lDisplayOptions
			// 
			this.lDisplayOptions.AutoSize = true;
			this.lDisplayOptions.Location = new System.Drawing.Point(3, 143);
			this.lDisplayOptions.Name = "lDisplayOptions";
			this.lDisplayOptions.Size = new System.Drawing.Size(81, 13);
			this.lDisplayOptions.TabIndex = 3;
			this.lDisplayOptions.Text = "Display options:";
			// 
			// cbDisplayOptions
			// 
			this.cbDisplayOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDisplayOptions.FormattingEnabled = true;
			this.cbDisplayOptions.Items.AddRange(new object[] {
            "Shaded colour image",
            "Colour image",
            "Do not display"});
			this.cbDisplayOptions.Location = new System.Drawing.Point(104, 140);
			this.cbDisplayOptions.Name = "cbDisplayOptions";
			this.cbDisplayOptions.Size = new System.Drawing.Size(250, 21);
			this.cbDisplayOptions.TabIndex = 4;
			// 
			// SectionPicture
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cbDisplayOptions);
			this.Controls.Add(this.lDisplayOptions);
			this.Controls.Add(this.oResolution);
			this.Controls.Add(this.tbFilename);
			this.Controls.Add(this.lFileName);
			this.Name = "SectionPicture";
			this.Size = new System.Drawing.Size(500, 200);
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
      private Resolution oResolution;
      private System.Windows.Forms.Label lDisplayOptions;
      private System.Windows.Forms.ComboBox cbDisplayOptions;
   }
}
