using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

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
      Dictionary<string, SchemaFile> files = SchemaFile.Schemas;
      foreach (KeyValuePair<string, SchemaFile> kv in files) {
        this.treeView1.Nodes.Add(kv.Value.ExportToTreeNode());
      }
    }
  }
}
