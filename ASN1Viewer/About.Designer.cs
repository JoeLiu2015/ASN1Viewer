namespace ASN1Viewer {
  partial class About {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.lblProdName = new System.Windows.Forms.Label();
      this.lblCopyRight = new System.Windows.Forms.Label();
      this.lblBuild = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblProdName
      // 
      this.lblProdName.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblProdName.Location = new System.Drawing.Point(12, 18);
      this.lblProdName.Name = "lblProdName";
      this.lblProdName.Size = new System.Drawing.Size(338, 45);
      this.lblProdName.TabIndex = 0;
      this.lblProdName.Text = "ASN.1 Viewer";
      this.lblProdName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblCopyRight
      // 
      this.lblCopyRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCopyRight.Location = new System.Drawing.Point(12, 78);
      this.lblCopyRight.Name = "lblCopyRight";
      this.lblCopyRight.Size = new System.Drawing.Size(336, 23);
      this.lblCopyRight.TabIndex = 1;
      this.lblCopyRight.Text = "Copyright © 2020-2021 ASN.1 Viewer Ltd";
      this.lblCopyRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblBuild
      // 
      this.lblBuild.Location = new System.Drawing.Point(12, 101);
      this.lblBuild.Name = "lblBuild";
      this.lblBuild.Size = new System.Drawing.Size(336, 23);
      this.lblBuild.TabIndex = 1;
      this.lblBuild.Text = "Version 0.9.0 Build 300";
      this.lblBuild.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // About
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(362, 124);
      this.Controls.Add(this.lblBuild);
      this.Controls.Add(this.lblCopyRight);
      this.Controls.Add(this.lblProdName);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "About";
      this.Padding = new System.Windows.Forms.Padding(9);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "About";
      this.Load += new System.EventHandler(this.About_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblProdName;
    private System.Windows.Forms.Label lblCopyRight;
    private System.Windows.Forms.Label lblBuild;
  }
}
