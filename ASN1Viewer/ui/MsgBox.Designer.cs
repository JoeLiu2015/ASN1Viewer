
namespace ASN1Viewer.ui {
  partial class MsgBox {
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
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.lblMsg = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(288, 148);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(155, 57);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "&Yes";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(482, 148);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(155, 57);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "&No";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // lblMsg
      // 
      this.lblMsg.Location = new System.Drawing.Point(28, 24);
      this.lblMsg.Name = "lblMsg";
      this.lblMsg.Size = new System.Drawing.Size(609, 104);
      this.lblMsg.TabIndex = 0;
      this.lblMsg.Text = "label1";
      // 
      // MsgBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(670, 233);
      this.Controls.Add(this.lblMsg);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MsgBox";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "MsgBox";
      this.TopMost = true;
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblMsg;
  }
}