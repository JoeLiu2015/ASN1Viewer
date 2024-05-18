namespace ASN1Viewer.ui {
  partial class EditASN1Node {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
      this.txtHexEdit = new System.Windows.Forms.TextBox();
      this.gpEditHex = new System.Windows.Forms.GroupBox();
      this.gpEditHex.SuspendLayout();
      this.SuspendLayout();
      // 
      // txtHexEdit
      // 
      this.txtHexEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtHexEdit.Location = new System.Drawing.Point(6, 19);
      this.txtHexEdit.Multiline = true;
      this.txtHexEdit.Name = "txtHexEdit";
      this.txtHexEdit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtHexEdit.Size = new System.Drawing.Size(507, 403);
      this.txtHexEdit.TabIndex = 0;
      this.txtHexEdit.TextChanged += new System.EventHandler(this.txtHexEdit_TextChanged);
      // 
      // gpEditHex
      // 
      this.gpEditHex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gpEditHex.Controls.Add(this.txtHexEdit);
      this.gpEditHex.Location = new System.Drawing.Point(3, 12);
      this.gpEditHex.Name = "gpEditHex";
      this.gpEditHex.Size = new System.Drawing.Size(519, 428);
      this.gpEditHex.TabIndex = 2;
      this.gpEditHex.TabStop = false;
      this.gpEditHex.Text = "Edit Hex Bytes";
      // 
      // EditASN1Node
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(525, 444);
      this.Controls.Add(this.gpEditHex);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "EditASN1Node";
      this.Text = "EditASN1Node";
      this.gpEditHex.ResumeLayout(false);
      this.gpEditHex.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TextBox txtHexEdit;
    private System.Windows.Forms.GroupBox gpEditHex;
  }
}