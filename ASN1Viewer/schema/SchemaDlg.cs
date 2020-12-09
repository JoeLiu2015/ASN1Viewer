using System;
using System.IO;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  public partial class SchemaDlg : Form {
    public SchemaDlg() {
      InitializeComponent();
    }

    private void SchemaDlg_Load(object sender, EventArgs e) {
      if (Directory.Exists(".\\schemas")) {
        string[] files = Directory.GetFiles(".\\schemas");
        for (int i = 0; i < files.Length; i++) {
          FileInfo fi = new FileInfo(files[i]);
          try {
            SchemaFile sf = SchemaFile.ParseFrom(fi.FullName);
            this.treeView1.Nodes.Add(sf.ExportToTreeNode());
          } catch (Exception ex) {
            this.treeView1.Nodes.Add(fi.Name + "(ERROR: " + ex.Message + ")");
          }
        }

      } else {
        MessageBox.Show("Can not find folder 'schemas'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}
