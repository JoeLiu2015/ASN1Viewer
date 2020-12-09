namespace ASN1Viewer.schema {
  partial class SchemaDlg {
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
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.treeView1.Location = new System.Drawing.Point(4, 4);
      this.treeView1.Name = "treeView1";
      this.treeView1.Size = new System.Drawing.Size(931, 564);
      this.treeView1.TabIndex = 0;
      // 
      // SchemaDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(938, 571);
      this.Controls.Add(this.treeView1);
      this.Name = "SchemaDlg";
      this.Text = "Schema Viewer";
      this.Load += new System.EventHandler(this.SchemaDlg_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TreeView treeView1;
  }
}