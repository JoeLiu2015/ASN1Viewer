using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ASN1Viewer.ui;

namespace ASN1Viewer
{
  public partial class MainForm : Form
  {
    private const long SIZE_100MB = 1024 * 1024 * 100L;
    private schema.SchemaDlg m_SchemaDlg = null;
    private ui.EditASN1Node m_EditNodeDlg = null;

    public MainForm()
    {
      InitializeComponent();
      this.txtInput.Font = this.hexViewer1.Font;
      this.txtInput.AllowDrop = true;
      this.txtInput.DragDrop += MainForm_DragDrop;
      this.txtInput.DragEnter += MainForm_DragEnter;
    }

    private void MainForm_Load(object sender, EventArgs e) {
      menuTopMost.Checked = Config.Instance.TopMost;
      this.TopMost = Config.Instance.TopMost;
      if (Config.Instance.TopMost) this.BringToFront();
      if (Config.Instance.Language == "zh_CN") menuChinese_Click(menuChinese, EventArgs.Empty);
      else                                     menuEnglish_Click(menuChinese, EventArgs.Empty);
      UpdateRecentFiles();
      UpdateTestFiles();
      UpdateToolbar();
      new System.Threading.Thread(CheckUpdate).Start();
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
      using (OpenFileDialog dlg = new OpenFileDialog()) {
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
      new About().ShowDialog(this);
    }
    private void menuChinese_Click(object sender, EventArgs e) {
      this.menuChinese.Checked = true;
      this.menuEnglish.Checked = false;
      Lang.Select("zh_CN");
      Config.Instance.Language = "zh_CN";
      LoadLang();
    }
    private void menuEnglish_Click(object sender, EventArgs e) {
      this.menuChinese.Checked = false;
      this.menuEnglish.Checked = true;
      Lang.Select("en_US");
      Config.Instance.Language = "en_US";
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
    private void menuTopMost_Click(object sender, EventArgs e) {
      menuTopMost.Checked = !menuTopMost.Checked;
      tbBtnTopMost.Checked = menuTopMost.Checked;
      Config.Instance.TopMost = menuTopMost.Checked;
      Config.Instance.Save();

      this.TopMost = Config.Instance.TopMost;
      if (Config.Instance.TopMost) this.BringToFront();
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
      if (m_SchemaDlg == null) {
        m_SchemaDlg = new schema.SchemaDlg();
        m_SchemaDlg.Icon = this.Icon;
      }
      m_SchemaDlg.BringToFront();
      m_SchemaDlg.ShowDialog(this);
    }
    private void menuCheckUpdate_Click(object sender, EventArgs e) {
      CheckUpdate(true);
    }
    private void ctxMenuCollapse_Click(object sender, EventArgs e) {
      TreeNode n = ctxMenuTree.Tag as TreeNode;
      n.Collapse();
    }
    private void ctxMenuExpand_Click(object sender, EventArgs e) {
      TreeNode n = ctxMenuTree.Tag as TreeNode;
      n.ExpandAll();
    }
    private void ctxMenuEdit_Click(object sender, EventArgs e) {
      TreeNode n = ctxMenuTree.Tag as TreeNode;

      if (this.m_EditNodeDlg == null || this.m_EditNodeDlg.IsDisposed) {
        this.m_EditNodeDlg = new EditASN1Node();
        this.m_EditNodeDlg.OnNodeChanged = (o, ee) => {
          TreeNode t = o as TreeNode;
          ASNNode root = (t.Tag as ASNNode).Root;
          t.Text = (t.Tag as ASNNode).ToString();
          this.UpdateHexBytesView(root.Data, root);
          this.treeView1_AfterSelect(treeView1, new TreeViewEventArgs(t));
          m_EditNodeDlg.Text = m_EditNodeDlg.Title + "(" + t.Text + ")";
        };
        m_EditNodeDlg.Node = n;
        m_EditNodeDlg.Show(this);
      } else {
        m_EditNodeDlg.Node = n;
      }

      m_EditNodeDlg.Text = m_EditNodeDlg.Title + "(" + n.Text + ")";
    }
    private void txtInput_TextChanged(object sender, EventArgs e) {
      ParseInputText(this.txtInput.Text);
			// Test merge
    }
    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
      this.tabControl1.SelectTab(this.tabPageBytes);
      TreeNode tn = e.Node;
      ASNNode an = tn.Tag as ASNNode;
      this.hexViewer1.SelectNode(an.Start, an.End, an.ContentStart, an.ContentEnd, an.Data);
      this.ShowNodeInfo(tn);
    }
    private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			// Test merge
      if (e.Button == MouseButtons.Right && e.Node.Nodes.Count > 0) {
        this.treeView1.SelectedNode = e.Node;
        ctxMenuTree.Tag = e.Node;
        ctxMenuCollapse.Enabled = true;
        ctxMenuExpand.Enabled = true;
        ctxMenuEdit.Enabled = false;
        this.ctxMenuTree.Show(this.treeView1, e.Location);
      } else if (e.Button == MouseButtons.Right && e.Node.Nodes.Count == 0) {
        this.treeView1.SelectedNode = e.Node;
        ctxMenuCollapse.Enabled = false;
        ctxMenuExpand.Enabled = false;
        ctxMenuEdit.Enabled = true;
        ctxMenuTree.Tag = e.Node;
        this.ctxMenuTree.Show(this.treeView1, e.Location);
      }
    }
    private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e) {
      TreeView tv = e.Node.TreeView;
      if (!tv.Focused && e.Node == tv.SelectedNode) {
        Font treeFont = (e.Node.NodeFont == null) ? tv.Font : e.Node.NodeFont;
        e.Graphics.FillRectangle(SystemBrushes.ControlDarkDark, e.Bounds);
        ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, SystemColors.HighlightText, SystemColors.Highlight);
        TextRenderer.DrawText(e.Graphics, e.Node.Text, treeFont, e.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
      } else {
        e.DrawDefault = true;
      }
    }
    private TreeNode CreateNode(ASNNode n) {
      TreeNode t = new TreeNode(n.ToString());
      t.Tag = n;
      // Initialize image index.
      int tag    = n.Tag;
      int tagNum = tag & ASNNode.NODE_TAG_NUMBER_MASK;
      int tagClass = tag & ASNNode.NODE_CLASS_MASK;
      if (tagClass == ASNNode.NODE_CLASS_CONTEXT     ||
          tagClass == ASNNode.NODE_CLASS_APPLICATION ||
          tagClass == ASNNode.NODE_CLASS_PRIVATE) {
        t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.CONTEXT_SPECIFIC;
      } else {
        switch (tagNum) {
          case ASNNode.UNIVERSAL_BOOLEAN:         { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.BOOLEAN;           break; }
          case ASNNode.UNIVERSAL_INTEGER:         { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.INTEGER;           break; }
          case ASNNode.UNIVERSAL_BITSTRING:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.BIT_STRING;        break; }
          case ASNNode.UNIVERSAL_OCTETSTRING:     { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.OCTET_STRING;      break; }
          case ASNNode.UNIVERSAL_NULL:            { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.TAG_NULL;          break; }
          case ASNNode.UNIVERSAL_OID:             { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.OBJECT_IDENTIFIER; break; }
          case ASNNode.UNIVERSAL_RELATIVE_OID:    { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.RELATIVE_OID;      break; }
          case ASNNode.UNIVERSAL_OBJ_DESCRIPTOR:  { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.OBJECT_DESCRIPTOR; break; }
          case ASNNode.UNIVERSAL_EXTERNAL:        { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.EXTERNAL;          break; }
          case ASNNode.UNIVERSAL_REAL:            { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.REAL;              break; }
          case ASNNode.UNIVERSAL_ENUMERATED:      { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.ENUMERATED;        break; }
          case ASNNode.UNIVERSAL_UTF8_STR:        { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.UTF8_STRING;       break; }
          case ASNNode.UNIVERSAL_SEQ_SEQOF:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.SEQUENCE;          break; }
          case ASNNode.UNIVERSAL_SET_SETOF:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.SET;               break; }
          case ASNNode.UNIVERSAL_NUMSTRING:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.NUMERIC_STRING;    break; }
          case ASNNode.UNIVERSAL_PRINTABLESTRING: { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.PRINTABLE_STRING;  break; }
          case ASNNode.UNIVERSAL_T61STRING:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.T61_STRING;        break; }
          case ASNNode.UNIVERSAL_VIDEOTEXSTRING:  { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.VIDEOTEXT_STRING;  break; }
          case ASNNode.UNIVERSAL_IA5STRING:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.IA5_STRING;        break; }
          case ASNNode.UNIVERSAL_UTCTIME:         { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.UTC_TIME;          break; }
          case ASNNode.UNIVERSAL_GENTIME:         { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.GENERALIZED_TIME;  break; }
          case ASNNode.UNIVERSAL_GRAPHIC_STR:     { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.GRAPHIC_STRING;    break; }
          case ASNNode.UNIVERSAL_ISO646STR:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.VISIBLE_STRING;    break; }
          case ASNNode.UNIVERSAL_GENERAL_STR:     { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.GENERAL_STRING;    break; }
          case ASNNode.UNIVERSAL_STRING:          { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.UNIVERSAL_STRING;  break; }
          case ASNNode.UNIVERSAL_BMPSTRING:       { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.BMPSTRING;         break; }
          default:                                { t.SelectedImageIndex = t.ImageIndex = (int)ImgIndex.CONTEXT_SPECIFIC;  break; }
        };
      }
      for (int i = 0; i < n.ChildCount; i++) {
        t.Nodes.Add(CreateNode(n[i]));
      }
      return t;
    }

    private void LoadLang() {
      this.Text                 = Lang.T["PROD_NAME"];
      this.menuFile.Text        = Lang.T["MENU_FILE"];
      this.menuView.Text        = Lang.T["MENU_VIEW"];
      this.menuTopMost.Text     = Lang.T["MENU_TOPMOST"];
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
      this.ctxMenuEdit.Text     = Lang.T["MENU_EDIT"];
      this.tabPageInput.Text    = Lang.T["TAB_INPUT"];
      this.tabPageBytes.Text    = Lang.T["TAB_BYTES"];
      if (this.treeView1.SelectedNode != null) {
        this.ShowNodeInfo(this.treeView1.SelectedNode);
      }
      this.UpdateHexBytesView(null, null);
      this.UpdateToolbar();
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

    private void UpdateToolbar() {
      this.toolbar.ImageList = imageListToolbar;
      this.tbBtnOpen.ImageIndex = 0;
      this.tbBtnRecent.ImageIndex = 1;
      this.tbBtnTopMost.ImageIndex = 2;
      this.tbBtnLang.ImageIndex = 3;
      this.tbBtnCheckUpdate.ImageIndex = 4;

      this.tbBtnOpen.ToolTipText = this.menuOpen.Text;
      this.tbBtnOpen.Click -= this.menuOpen_Click;
      this.tbBtnOpen.Click += this.menuOpen_Click;
      this.tbBtnRecent.ToolTipText = this.menuRecent.Text;
      this.tbBtnRecent.DropDown = this.menuRecent.DropDown;
      this.tbBtnTopMost.ToolTipText = this.menuTopMost.Text;
      this.tbBtnTopMost.Checked = this.menuTopMost.Checked;
      this.tbBtnTopMost.Click -= this.menuTopMost_Click;
      this.tbBtnTopMost.Click += this.menuTopMost_Click;
      this.tbBtnLang.ToolTipText = this.menuLanguage.Text;
      this.tbBtnLang.DropDown = this.menuLanguage.DropDown;
      this.tbBtnCheckUpdate.ToolTipText = this.menuCheckUpdate.Text;
      this.tbBtnCheckUpdate.Click -= this.menuCheckUpdate_Click;
      this.tbBtnCheckUpdate.Click += this.menuCheckUpdate_Click;
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
      string data = File.ReadAllText(file);
      byte[] bytes = Utils.ParseHexBytes(data);
      if (bytes == null) {
        bytes = Utils.ParsePEM(data);
      }
      if (bytes == null) {
        data = null;
        bytes = File.ReadAllBytes(file);
      }
      if (ParseASN1(bytes)) {
        this.txtInput.TextChanged -= this.txtInput_TextChanged;
        if (data != null) {
          this.txtInput.Text = Utils.FixCRLF(data);
        } else {
          this.txtInput.Text = Utils.HexDump(bytes, 0);
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
            //if (nt.Key == "PFX") {
            //  int debug = 1;
            //}
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
        this.ShowStatusText(Color.Green, Lang.T["STATUS_SUCCEED"]);
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

    private void CheckUpdate() {
      if (!Config.Instance.AutoUpdate) return;
      System.Threading.Thread.Sleep(10000); // 10s
      
      CheckUpdate(false);
    }
    private void CheckUpdate(bool byUI) {
      return; // Disable it now
      if (this.Cursor == Cursors.WaitCursor) return;
      if (byUI) Cursor.Current = Cursors.WaitCursor;
      try {
        Updater.TryRemoveOldExe();
        DateTime[] vers = Updater.GetVersions();
        if (vers[0] <= Config.Instance.ASN1ViewerMT && vers[1] <= Config.Instance.ASN1ModulesMT) {
          ShowUpdateResult("OK", null);
          return;
        }

        bool needRestart = false;
        if (vers[0] > Config.Instance.ASN1ViewerMT) {
          Updater.UpdateASN1Viewer();
          Config.Instance.ASN1ViewerMT = vers[0];
          Config.Instance.Save();
          needRestart = true;
        }
        if (vers[1] > Config.Instance.ASN1ModulesMT) {
          Updater.UpdateFiles();
          Config.Instance.ASN1ModulesMT = vers[1];
          Config.Instance.Save();
        }
        ShowUpdateResult("Success", null);
        if (byUI && needRestart) {
          if (MsgBox.Show(this, Lang.T["PROD_NAME"], Lang.T["MST_RESTART"], Lang.T["BTN_YES"], Lang.T["BTN_NO"]) == DialogResult.OK) {
            this.Close();
            System.Diagnostics.Process.Start(Config.AppName);
            return;
          }
        } else {
          schema.SchemaFile.ReloadSchemas();
        }
      } catch (Exception ex) {
        ShowUpdateResult("Failed", ex.Message);
      } finally {
        if (byUI) Cursor.Current = Cursors.Default;
      }
    }

    private delegate void ShowUpdatResultHandler(string ret, string error);
    private void ShowUpdateResult(string ret, string error) {
      if (this.InvokeRequired) {
        this.BeginInvoke(new ShowUpdatResultHandler(ShowUpdateResult), ret, error);
      } else { 
        switch (ret) {
          case "OK":
            this.lbStatusRight.Text = Lang.T["MSG_ALREADYLATEST"];
            this.lbStatusRight.ForeColor = Color.Green;
            break;
          case "Success":
            this.lbStatusRight.Text = Lang.T["MSG_UPDATESUCCESS"];
            this.lbStatusRight.ForeColor = Color.Green;
            break;
          case "Failed":
            this.lbStatusRight.Text = string.Format(Lang.T["MSG_UPDATEFAILED"], error);
            this.lbStatusRight.ForeColor = Color.Red;
            break;
          default:
            this.lbStatusRight.Text = "";
            break;
        }
      }
    }

    private void Err(String msg) {
      this.ShowStatusText(Color.Red, "ERROR: " + msg);
    }
    private void ShowStatusText(Color color, string text) { 
      this.lbStatus.ForeColor = color;
      this.lbStatus.Text = text;
    }

    private void ShowNodeInfo(TreeNode tn) {
      ASNNode an = tn.Tag as ASNNode;
      this.hexViewer1.SelectNode(an.Start, an.End, an.ContentStart, an.ContentEnd, an.Data);
      this.ShowStatusText(SystemColors.WindowText, String.Format(Lang.T["STATUS_ASNINFO"], an.Start, an.TagNum, an.ContentEnd - an.ContentStart, GetAllChildCount(tn)));
    }

    private static int GetAllChildCount(TreeNode tn) {
      int count = tn.Nodes.Count;
      int total = count;
      for (int i = 0; i < count; i++) {
        total += GetAllChildCount(tn.Nodes[i]);
      }
      return total;
    }
  }
  public enum ImgIndex {
    BOOLEAN             = 21, //  1
    INTEGER             =  1, //  2
    BIT_STRING          = 23, //  2
    OCTET_STRING        = 25, //  4
    TAG_NULL            = 18, //  5
    OBJECT_IDENTIFIER   = 11, //  6
    OBJECT_DESCRIPTOR   = 11, //  7
    EXTERNAL            =  6, //  8
    REAL                =  1, //  9
    ENUMERATED          =  1, // 10
    UTF8_STRING         =  7, // 12
    RELATIVE_OID        = 35, // 13
    SEQUENCE            = 32, // 16
    SET                 = 33, // 17
    NUMERIC_STRING      =  8, // 18
    PRINTABLE_STRING    =  7, // 19
    T61_STRING          =  7, // 20
    VIDEOTEXT_STRING    =  7, // 21
    IA5_STRING          =  7, // 22
    UTC_TIME            = 34, // 23
    GENERALIZED_TIME    = 34, // 24
    GRAPHIC_STRING      =  7, // 25
    VISIBLE_STRING      =  7, // 26
    GENERAL_STRING      =  7, // 27
    UNIVERSAL_STRING    =  7, // 28
    BMPSTRING           =  7, // 30
    CONTEXT_SPECIFIC    = 24  // 31
  }
}
