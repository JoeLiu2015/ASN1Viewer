using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer.ui {
  public partial class EditASN1Node : Form {
    public EditASN1Node() {
      InitializeComponent();
      this.Font = new System.Drawing.Font("YouYuan", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      if (this.Font.Name != "YouYuan") {
        this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      }
      LoadLang();
    }

    private TreeNode m_Node = null;

    public String Title = "";
    public EventHandler OnNodeChanged;

    public ASNNode ASNNode {
      get { return m_Node.Tag as ASNNode; }
    }

    public TreeNode Node {
      set { m_Node = value; load(); }
    }
    private void LoadLang() {
      this.Title                = Lang.T["PROD_NAME"];
      this.gpEditHex.Text       = Lang.T["TXT_EDITHEX"];
    }

    private void load() {
      StringBuilder sb = new StringBuilder();
      ASNNode aNode = ASNNode;
      byte[] d = aNode.Data;
      int start = aNode.ContentStart;
      int end = aNode.ContentEnd;
      int col = 0;
      for (int i = start; i < end; i++) {
        sb.Append(Utils.ByteToHex(d[i]) + " ");
        col++;
        if (col == 16) {
          col = 0;
          sb.Append("\r\n");
        }
      }

      this.txtHexEdit.Text = sb.ToString();
    }

    private void txtHexEdit_TextChanged(object sender, EventArgs e) {
      byte[] b = Utils.HexDecode(this.txtHexEdit.Text);
      if (b != null) {
        if (ASNNode.SetContent(b)) {
          if (OnNodeChanged != null) { OnNodeChanged(m_Node, EventArgs.Empty);}
        }
      }
    }
  }
}
