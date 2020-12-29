using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer
{
  public partial class MainForm : Form
  {
    private const long SIZE_100MB = 1024 * 1024 * 100L;
    private schema.SchemaDlg m_SchemaDlg = null;
    public MainForm()
    {
      InitializeComponent();
      this.txtInput.Font = this.hexViewer1.Font;
      
    }

    private void MainForm_Load(object sender, EventArgs e) {
      menuChinese_Click(menuChinese, EventArgs.Empty);
      UpdateRecentFiles();
      UpdateTestFiles();
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
      Config.Instance.Save();
    }
    private void MainForm_DragDrop(object sender, DragEventArgs e) {
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0) ParseInputFile(files[0]);
    }
    private void MainForm_DragEnter(object sender, DragEventArgs e) {
      if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Move;
    }

    private void menuExit_Click(object sender, EventArgs e) {
      this.Close();
    }
    private void menuOpen_Click(object sender, EventArgs e) {
      String file = "";
      using (SaveFileDialog dlg = new SaveFileDialog()) {
        dlg.CreatePrompt = true;
        dlg.OverwritePrompt = false;
        dlg.CheckFileExists = false;
        dlg.CheckPathExists = true;
        dlg.DefaultExt = "*.*";
        dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        dlg.FilterIndex = 2;
        DialogResult res = dlg.ShowDialog(this);
        if (res == DialogResult.OK) {
          file = dlg.FileName;
        }
      }
      if (!String.IsNullOrEmpty(file)) ParseInputFile(file);
    }
    private void menuAbout_Click(object sender, EventArgs e) {
      new About().ShowDialog();
    }
    private void menuChinese_Click(object sender, EventArgs e) {
      if (this.menuChinese.Checked) return;
      this.menuChinese.Checked = true;
      this.menuEnglish.Checked = false;
      Lang.Select("zh_CN");
      LoadLang();
    }
    private void menuEnglish_Click(object sender, EventArgs e) {
      if (this.menuEnglish.Checked) return;
      this.menuChinese.Checked = false;
      this.menuEnglish.Checked = true;
      Lang.Select("en_US");
      LoadLang();
    }
    private void menuRentFileItem_Click(object sender, EventArgs e) {
      ToolStripMenuItem item = sender as ToolStripMenuItem;
      string txt = item.Text;
      int pos = txt.IndexOf(' ');
      int idx = int.Parse(txt.Substring(0, pos));
      string file = txt.Substring(pos + 1).Trim();
      Config.Instance.History.Remove(file);
      if (!File.Exists(file)) {
        MessageBox.Show(this, String.Format(Lang.T["MSG_NOFILE"], file), Lang.T["PROD_NAME"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        UpdateRecentFiles();
        return;
      }
      ParseInputFile(file);
    }
    private void menuTestFileItem_Click(object sender, EventArgs e) {
      ToolStripMenuItem item = sender as ToolStripMenuItem;
      string txt = item.Text;
      int pos = txt.IndexOf(' ');
      string filename = new FileInfo(".\\files\\TestFiles\\" + txt.Substring(pos+1).Trim()).FullName;
      if (!File.Exists(filename)) {
        MessageBox.Show(this, String.Format(Lang.T["MSG_NOFILE"], filename), Lang.T["PROD_NAME"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      ParseInputFile(filename);
    }
    private void menuASN1Modules_Click(object sender, EventArgs e) {
      if (m_SchemaDlg == null) m_SchemaDlg = new schema.SchemaDlg();
      m_SchemaDlg.ShowDialog();
    }
    private void ctxMenuCollapse_Click(object sender, EventArgs e) {
      TreeNode n = ctxMenuTree.Tag as TreeNode;
      n.Collapse();
    }
    private void ctxMenuExpand_Click(object sender, EventArgs e) {
      TreeNode n = ctxMenuTree.Tag as TreeNode;
      n.ExpandAll();
    }
    private void txtInput_TextChanged(object sender, EventArgs e) {
      ParseInputText(this.txtInput.Text);
    }
    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
      this.tabControl1.SelectTab(this.tabPageBytes);
      TreeNode tn = e.Node;
      ASNNode an = tn.Tag as ASNNode;
      this.hexViewer1.SelectNode(an.Start, an.End, an.ContentStart, an.ContentEnd);
      this.lbStatus.ForeColor = SystemColors.WindowText;
      this.lbStatus.Text = String.Format(Lang.T["STATUS_ASNINFO"], an.Start, an.TagNum, an.ContentEnd - an.ContentStart);
    }
    private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
      if (e.Button == MouseButtons.Right && e.Node.Nodes.Count > 0) {
        this.treeView1.SelectedNode = e.Node;
        ctxMenuTree.Tag = e.Node;
        this.ctxMenuTree.Show(this.treeView1, e.Location);
      }
    }

    private TreeNode CreateNode(ASNNode n) {
      TreeNode t = new TreeNode(n.ToString());
      t.Tag = n;
      for (int i = 0; i < n.Count; i++) {
        t.Nodes.Add(CreateNode(n[i]));
      }
      return t;
    }
    private void Err(String msg) {
      //MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
      //  MessageBoxOptions.DefaultDesktopOnly, false);
      this.lbStatus.Text = "ERROR: " + msg;
      this.lbStatus.ForeColor = Color.Red;
    }
    private void LoadLang() {
      this.Text                 = Lang.T["PROD_NAME"];
      this.menuFile.Text        = Lang.T["MENU_FILE"];
      this.menuView.Text        = Lang.T["MENU_VIEW"];
      this.menuASN1Modules.Text = Lang.T["MENU_MODULES"];
      this.menuTestFiles.Text   = Lang.T["MENU_TESTFILES"];
      this.menuHelp.Text        = Lang.T["MENU_HELP"];
      this.menuOpen.Text        = Lang.T["MENU_OPEN"];
      this.menuExit.Text        = Lang.T["MENU_EXIT"];
      this.menuRecent.Text      = Lang.T["MENU_RECENT"];
      this.menuLanguage.Text    = Lang.T["MENU_LANG"];
      this.menuChinese.Text     = Lang.T["MENU_CHINESE"];
      this.menuEnglish.Text     = Lang.T["MENU_ENGLISH"];
      this.menuAbout.Text       = Lang.T["MENU_ABOUT"];
      this.menuCheckUpdate.Text = Lang.T["MENU_CHECKVER"];
      this.ctxMenuCollapse.Text = Lang.T["MENU_COLLAPSE"];
      this.ctxMenuExpand.Text   = Lang.T["MENU_EXPAND"];
      this.tabPageInput.Text    = Lang.T["TAB_INPUT"];
      this.tabPageBytes.Text    = Lang.T["TAB_BYTES"];
      if (this.treeView1.SelectedNode != null) {
        ASNNode an = this.treeView1.SelectedNode.Tag as ASNNode;
        if (an != null) this.lbStatus.Text = String.Format(Lang.T["STATUS_ASNINFO"], an.Start, an.TagNum, an.ContentEnd - an.ContentStart);
      } else {

      }
      this.UpdateHexBytesView(null, null);
    }
    private void UpdateRecentFiles() {
      this.menuRecent.DropDownItems.Clear();
      for (int i = 0; i < Config.Instance.History.Count; i++) {
        ToolStripMenuItem mi = new ToolStripMenuItem();
        mi.Text = (i + 1) + " " + Config.Instance.History[i];
        mi.Click += this.menuRentFileItem_Click;
        this.menuRecent.DropDownItems.Add(mi);
      }
      this.menuRecent.Enabled = this.menuRecent.DropDownItems.Count > 0;
    }
    private void UpdateTestFiles() {
      this.menuTestFiles.DropDownItems.Clear();
      string[] files = Directory.GetFiles(".\\files\\TestFiles");
      for (int i = 0; i < files.Length; i++) {
        ToolStripMenuItem mi = new ToolStripMenuItem();
        mi.Text = (i + 1) + " " + new FileInfo(files[i]).Name;
        mi.Click += this.menuTestFileItem_Click;
        this.menuTestFiles.DropDownItems.Add(mi);
      }
      this.menuTestFiles.Enabled = this.menuTestFiles.DropDownItems.Count > 0;
    }


    private void ParseInputFile(string file) {
      FileInfo f = new FileInfo(file);
      if (!f.Exists) {
        Err("File \"" + file + "\" doesn't exists.");
        return;
      }

      if (f.Length > SIZE_100MB) {
        Err("File \"" + file + "\" exceed the size limit(100MB).");
        return;
      }
      if (f.Length == 0) {
        Err("File \"" + file + "\" is empty.");
        return;
      }
      byte[] data = File.ReadAllBytes(file);
      byte[] b64Data = Utils.ParsePEM(Utils.Get8BitString(data));

      byte[] asnData = data;
      if (b64Data != null) asnData = b64Data;
      if (ParseASN1(asnData)) {
        this.txtInput.TextChanged -= this.txtInput_TextChanged;
        if (b64Data != null) {
          this.txtInput.Text = Utils.FixCRLF(File.ReadAllText(file));
        } else {
          this.txtInput.Text = Utils.HexDump(data, 0);
        }
        this.txtInput.TextChanged += this.txtInput_TextChanged;

        // Update recent files
        string normalizePath = Path.GetFullPath(new Uri(file).LocalPath)
          .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (Config.Instance.History.Contains(normalizePath)) Config.Instance.History.Remove(normalizePath);
        Config.Instance.History.Insert(0, normalizePath);
        if (Config.Instance.History.Count > Config.Instance.MaxHistoryCount) {
          Config.Instance.History.RemoveAt(Config.Instance.History.Count - 1);
        }
        UpdateRecentFiles();

        // Update Form title
        UpdateFormTitle(file);
      }
    }
    private void ParseInputText(string text) {
      if (text.Length == 0) return;

      byte[] data = Utils.ParseHexBytes(text);
      
      if (data == null) {
        data = Utils.ParsePEM(text);
      }

      if (data == null) {
        Err("The input text is invalid ASN.1 data.");
        return;
      }

      if (ParseASN1(data)) {
        UpdateFormTitle(text);
      }
    }
    private bool ParseASN1(byte[] data) {
      try {
        ASNNode a = ASNNode.Parse(data);
        Dictionary<string, schema.SchemaFile> schemas = schema.SchemaFile.Schemas;

        // Match schema
        Dictionary<string, schema.TypeDef> matched = new Dictionary<string, schema.TypeDef>();
        foreach (KeyValuePair<string, schema.SchemaFile> kv in schemas) {
          Dictionary<string, schema.TypeDef> types = kv.Value.Types;
          foreach (KeyValuePair<string, schema.TypeDef> nt in types) {
            if (nt.Key == "PFX") {
              int debug = 1;
            }
            if (!matched.ContainsKey(nt.Key) && nt.Value.Match(a, false)) matched.Add(nt.Key, nt.Value);
          }
        }
        List<string> knownTypes = schema.SchemaFile.KNOWN_TYPES;
        for (int i = 0; i < knownTypes.Count; i++) {
          if (matched.ContainsKey(knownTypes[i])) {
            matched[knownTypes[i]].Match(a, true);
            break;
          }
        }
        
        this.treeView1.Nodes.Clear();
        this.treeView1.Nodes.Add(CreateNode(a));
        this.lbStatus.Text = "Succeed.";
        this.lbStatus.ForeColor = Color.Green;
        this.UpdateHexBytesView(data, a);
        return true;
      } catch (Exception ex) {
        Err(ex.Message);
      }

      return false;
    }

    private void UpdateHexBytesView(byte[] data, ASNNode a) {
      if (a == null) {
        if (hexViewer1.BlockCount > 1) this.hexViewer1.RefreshView();
      } else {
        this.hexViewer1.ClearData();
        List<int> ranges = ASNNode.GetDisplayBytes(a);
        for (int i = 0; i < ranges.Count / 2; i++) {
          this.hexViewer1.AddData(data, ranges[i * 2], ranges[i * 2 + 1]);
        }
        this.hexViewer1.RefreshView();
      }
    }
    private void UpdateFormTitle(string fileOrText) {
      if (File.Exists(fileOrText)) {
        this.Text = Lang.T["PROD_NAME"] + " - " + fileOrText;
      } else if (!String.IsNullOrEmpty(fileOrText)) {
        this.Text = Lang.T["PROD_NAME"] + " - " + Lang.T["TXT_TEXT"];
      } else {
        this.Text = Lang.T["PROD_NAME"];
      }
    }

  }
}
