using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer
{
  public partial class MainForm : Form
  {
   
    public MainForm()
    {
      InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e) {
      menuChinese_Click(menuChinese, EventArgs.Empty);
      UpdateRecentFiles();
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
    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      this.Close();
    }
    private void openToolStripMenuItem_Click(object sender, EventArgs e) {
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
    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      new About().ShowDialog();
      //Schema sc = new Schema();
      //sc.Add(@"D:\GitHub\ASN1Viewer\ASN1Viewer\schemas\X509.txt");

      //SchemaTokenizer st = new SchemaTokenizer(File.ReadAllText(@"D:\GitHub\ASN1Viewer\ASN1Viewer\schemas\X509.txt"));

      //string s = st.Next();
      //while (s != null) {
      //  System.Diagnostics.Debug.WriteLine(s);
      //  s = st.Next();
      //}

    }
    private void txtInput_TextChanged(object sender, EventArgs e) {
      ParseInputText(this.txtInput.Text);
    }

    private TreeNode CreateNode(ASNNode n) {
      TreeNode t = new TreeNode(n.ToString());
      t.Tag = n;
      for (int i = 0; i < n.GetChildCount(); i++) {
        t.Nodes.Add(CreateNode(n.GetChild(i)));
      }
      return t;
    }

    private void Err(String msg) {
      //MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
      //  MessageBoxOptions.DefaultDesktopOnly, false);
      this.lbStatus.Text = "ERROR: " + msg;
      this.lbStatus.ForeColor = Color.Red;
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
      byte[] b64Data = ParsePEM(Utils.Get8BitString(data));

      byte[] asnData = data;
      if (b64Data != null) asnData = b64Data;
      if (ParseASN1(asnData)) {
        this.txtInput.TextChanged -= this.txtInput_TextChanged;
        if (b64Data != null) {
          this.txtInput.Text = Utils.FixCRLF(File.ReadAllText(file));
        } else {
          this.txtInput.Text = Utils.HexDump(data, 0, data.Length, "");
        }
        this.txtInput.TextChanged += this.txtInput_TextChanged;

        // Update recent files
        Config.Instance.History.Insert(0, file);
        if (Config.Instance.History.Count > Config.Instance.MaxHistory) {
          Config.Instance.History.RemoveAt(Config.Instance.History.Count - 1);
        }
        UpdateRecentFiles();
      }
    }
    private void ParseInputText(string text) {
      if (text.Length == 0) return;

      byte[] data = ParseHexBytes(text);
      
      if (data == null) {
        data = ParsePEM(text);
      }

      if (data == null) {
        Err("The input text is invalid ASN.1 data.");
        return;
      }

      ParseASN1(data);
    }
    private bool ParseASN1(byte[] data) {
      try {
        ASNNode a = ASNNode.Parse(data);
        Schema schema = LoadSchema();
        Dictionary<string, TypeDef> types = schema.Types;
        foreach (KeyValuePair<string, TypeDef> kv in types) {
          //if (kv.Key != "Certificate") continue;
          if (a.MatchSchema(kv.Key, kv.Value)) break;
        }

        this.treeView1.Nodes.Clear();
        this.treeView1.Nodes.Add(CreateNode(a));
        this.lbStatus.Text = "Succeed.";
        this.lbStatus.ForeColor = Color.Green;
        this.hexViewer1.Data = data;
        return true;
      } catch (Exception ex) {
        Err(ex.Message);
      }

      return false;
    }
    private byte[] ParseHexBytes(string text) {
      if (DIGITS == null) {
        DIGITS = new byte[128];
        for (int i = (int)'0'; i <= (int)'9'; i++) DIGITS[i] = (byte)(i - (int)'0');
        for (int i = (int)'a'; i <= (int)'f'; i++) DIGITS[i] = (byte)(i - (int)'a' + 10);
        for (int i = (int)'A'; i <= (int)'F'; i++) DIGITS[i] = (byte)(i - (int)'A' + 10);
      }
      MemoryStream ms = new MemoryStream();
      int pos = 0;
      while (pos < text.Length) {
        char ch = text[pos];
        if (ch == '\r' || ch == '\n' || ch == ' ' || ch == '\t') {
          pos++;
          continue;
        }
        if (pos + 1 < text.Length && IsHexChar(text[pos]) && IsHexChar(text[pos + 1])) {
          ms.WriteByte((byte)((DIGITS[text[pos]] << 4) | DIGITS[text[pos + 1]]));
          pos += 2;
        } else {
          return null;
        }
      }

      if (ms.Length == 0) return null;
      return ms.ToArray();
    }
    private byte[] ParsePEM(string text) {
      if (B64MAP == null) {
        B64MAP = new bool[256];
        for (int i = (int)'0'; i <= (int)'9'; i++) B64MAP[i] = true;
        for (int i = (int)'a'; i <= (int)'z'; i++) B64MAP[i] = true;
        for (int i = (int)'A'; i <= (int)'Z'; i++) B64MAP[i] = true;
        B64MAP['+'] = true;
        B64MAP['/'] = true;
      }
      if (text.Contains("-----BEGIN")) {
        int start = text.IndexOf("-----", 5);
        int end = text.LastIndexOf("-----");
        if (end > 0) end = text.LastIndexOf("-----", end);
        if (start > 0 && end > 0) {
          text = text.Substring(start + 5, end - start - 5);
        }
      }
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < text.Length; i++) {
        char ch = text[i];
        if (ch >= 256) return null;
        if (ch == '\r' || ch == '\n' || ch == ' ' || ch == '\t') continue;
        if (!B64MAP[ch] && ch != '=') return null;
        sb.Append(ch);
      }
      try {
        String s = sb.ToString();
        if (s.Length == 0) return null;
        return Convert.FromBase64String(s);
      } catch (Exception ex) {
        return null;
      }
    }
    private bool IsHexChar(char ch) {
      return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
    }

    private Schema m_Schema = null;

    private Schema LoadSchema() {
      if (m_Schema != null) return m_Schema;
      m_Schema = new Schema();
      string[] files = Directory.GetFiles("schemas");
      for (int i = 0; i < files.Length; i++) {
        m_Schema.Add(files[i]);
      }

      return m_Schema;
    }

    private static byte[] DIGITS = null;
    private static bool[] B64MAP = null;
    private const long SIZE_100MB = 1024 * 1024 * 100L;

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
      this.tabControl1.SelectTab(this.tabPageBytes);
      TreeNode tn = e.Node;
      ASNNode an = tn.Tag as ASNNode;
      this.hexViewer1.SelectNode(an.Start, an.End, an.ContentStart, an.ContentEnd);
      this.lbStatus.ForeColor = SystemColors.WindowText;
      this.lbStatus.Text = String.Format(Lang.T["STATUS_ASNINFO"], an.Start, an.GetTagNum(), an.ContentEnd - an.ContentStart);
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
    private void LoadLang() {
      this.Text              = Lang.T["PROD_NAME"];
      this.menuFile.Text     = Lang.T["MENU_FILE"];
      this.menuOptions.Text  = Lang.T["MENU_OPTIONS"];
      this.menuHelp.Text     = Lang.T["MENU_HELP"];
      this.menuOpen.Text     = Lang.T["MENU_OPEN"];
      this.menuExit.Text     = Lang.T["MENU_EXIT"];
      this.menuRecent.Text   = Lang.T["MENU_RECENT"];
      this.menuLanguage.Text = Lang.T["MENU_LANG"];
      this.menuChinese.Text  = Lang.T["MENU_CHINESE"];
      this.menuEnglish.Text  = Lang.T["MENU_ENGLISH"];
      this.menuAbout.Text    = Lang.T["MENU_ABOUT"];
      this.tabPageInput.Text = Lang.T["TAB_INPUT"];
      this.tabPageBytes.Text = Lang.T["TAB_BYTES"];
      if (this.treeView1.SelectedNode != null) {
        ASNNode an = this.treeView1.SelectedNode.Tag as ASNNode;
        if (an != null) this.lbStatus.Text = String.Format(Lang.T["STATUS_ASNINFO"], an.Start, an.GetTagNum(), an.ContentEnd - an.ContentStart);
      } else {

      }
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

    
  }
}
