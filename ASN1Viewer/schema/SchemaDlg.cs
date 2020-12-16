using System;
using System.IO;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  public partial class SchemaDlg : Form {
    public SchemaDlg() {
      InitializeComponent();
    }

    private bool m_Loaded = false;
    private void SchemaDlg_Load(object sender, EventArgs e) {
      LoadLang();
      LoadAsn1Modules();
    }

    private void LoadLang() {
      this.Text = Lang.T["MODULES_TITLE"];
    }

    private void LoadAsn1Modules() {
      if (m_Loaded) return;
      m_Loaded = true;
      if (Directory.Exists(".\\schemas")) {
        string[] files = Directory.GetFiles(".\\schemas");
        for (int i = 0; i < files.Length; i++) {
          FileInfo fi = new FileInfo(files[i]);
          try {
            SchemaFile sf = SchemaFile.ParseFrom(fi.FullName);
            this.treeView1.Nodes.Add(sf.ExportToTreeNode());
          } catch (Exception ex) {
            this.treeView1.Nodes.Add(fi.Name + String.Format("({0}: {1}", Lang.T["MSG_ERROR"], ex.Message));
          }
        }
      } else {
        MessageBox.Show(String.Format(Lang.T["MSG_NOMODULES"], ".\\schemas"), Lang.T["PROD_NAME"], MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}
