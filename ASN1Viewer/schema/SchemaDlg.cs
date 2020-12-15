using System;
using System.IO;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  public partial class SchemaDlg : Form {
    public SchemaDlg() {
      InitializeComponent();
    }

    private void SchemaDlg_Load(object sender, EventArgs e) {
      LoadLang();
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

    private void LoadLang() {
      this.Text = Lang.T["MODULES_TITLE"];
      
    }
  }
}
