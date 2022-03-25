using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ASN1Viewer.schema {
  public partial class SchemaDlg : Form {
    public SchemaDlg() {
      InitializeComponent();
      this.StartPosition = FormStartPosition.CenterParent;
      this.ShowInTaskbar = false;
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
      Dictionary<string, SchemaFile> files = SchemaFile.Schemas;
      foreach (KeyValuePair<string, SchemaFile> kv in files) {
        try {
          this.treeView1.Nodes.Add(kv.Value.ExportToTreeNode());
        } catch (Exception ex) {
          MessageBox.Show("Failed to load ASN1 module '" + kv.Key + "'.", "Error", MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
      }
    }
  }
}
