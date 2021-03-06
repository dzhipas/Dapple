namespace Dapple.Extract
{
   partial class HyperXYZ
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
			this.c_lArcMapNote = new System.Windows.Forms.Label();
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
			// c_lArcMapNote
			// 
			this.c_lArcMapNote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_lArcMapNote.Location = new System.Drawing.Point(0, 26);
			this.c_lArcMapNote.Name = "c_lArcMapNote";
			this.c_lArcMapNote.Size = new System.Drawing.Size(500, 174);
			this.c_lArcMapNote.TabIndex = 2;
			this.c_lArcMapNote.Text = "Note: \'Point\' datasets are always extracted using the dataset\'s native projection" +
				 " when using ArcMap.";
			this.c_lArcMapNote.Visible = false;
			// 
			// HyperXYZ
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_lArcMapNote);
			this.Controls.Add(this.tbFilename);
			this.Controls.Add(this.lFileName);
			this.Name = "HyperXYZ";
			this.Size = new System.Drawing.Size(500, 200);
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
		private System.Windows.Forms.Label c_lArcMapNote;
   }
}
