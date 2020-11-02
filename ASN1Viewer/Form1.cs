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
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }



    private TreeNode CreateNode(ASNNode n) {
      TreeNode t = new TreeNode(n.ToString());
      for (int i = 0; i < n.GetChildCount(); i++) {
        t.Nodes.Add(CreateNode(n.GetChild(i)));
      }
      return t;
    }

    private void fileToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }


    private bool IsPrintableText(byte[] data) {
      for (int i = 0; i < data.Length; i++) {
        if (data[i] == 13 || data[i] == 10 || data[i] == 9) continue; // \r \n \t
        if (data[i] >= 32 && data[i] <= 127) continue;
        return false;
      }

      return true;
    }

    private static byte[] Get8BitBytes(string str) {
      return Encoding.GetEncoding("ISO-8859-1").GetBytes(str);
    }
    private static String Get8BitString(byte[] data) {
      return Encoding.GetEncoding("ISO-8859-1").GetString(data);
    }
    public static byte[] CopyBytes(byte[] value, int offset, int length) {
      if (value == null) return new byte[0];

      if (offset < 0) {
        offset = 0;
      } else if (offset > value.Length) {
        offset = value.Length;
      }

      if (length < 0) length = 0;

      if (offset + length > value.Length) {
        length = value.Length - offset;
      }

      byte[] result = new byte[length];
      Array.Copy(value, offset, result, 0, length);
      return result;
    }
    public static void IntToBytes(long uinteger, byte[] bytes, int offset, int length) {
      for (int i = offset + length - 1; i >= offset; i--, uinteger >>= 8) {
        bytes[i] = (byte)(uinteger & 0x0ff);
      }
    }
    public static byte[] LongToBytes(long uinteger, int length) {
      byte[] buff = new byte[length];
      IntToBytes(uinteger, buff, 0, length);
      return buff;
    }

    public static String IntToHex(int val) {
      StringBuilder sb = new StringBuilder();
      byte[] d = LongToBytes(val, 4);
      for (int i = 0; i < d.Length; i++) sb.Append(ByteToHex(d[i]));
      return sb.ToString();
    }

    public static String ByteToHex(byte c) {
      char[] d = new char[] { '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      return "" + d[(c & 0xFF) >> 4] + d[c & 0x0F];
    }

    private String HexDump(byte[] val, int offset, int length, string prefix) {
      if (val == null || val.Length == 0) return "";

      StringBuilder sb = new StringBuilder();
      byte[] d = new byte[] {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7', (byte) '8',
        (byte) '9', (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', (byte) 'E', (byte) 'F'
      };
      byte[] defaultBuf = Get8BitBytes("                                                                    "); // length: 68
      byte[] buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      int len = length;
      int len16 = (len + 15) / 16 * 16; // Extend length to multiple of 16, or use as-is if it's already a multiple of 16.

      for (int i = 0; i < len16;) {
        for (int j = 0; j < 16; j++, i++) {
          if (i < len) {
            byte c = val[i + offset];
            // Offsets into buf for byte characters and text character. (Offset 51 is where text characters start.)
            int pos1 = j * 3, pos2 = 52 + j;
            if (j >= 8) pos1 += 1;
            buf[pos1] = d[(c & 0xFF) >> 4];
            buf[pos1 + 1] = d[c & 0x0F];
            buf[pos2] = (c < 0x20 || c > 0x7E) ? (byte) '.' : c;
          } // Else: Do nothing, the positions already hold space (0x20) characters.
        }

        if (!String.IsNullOrEmpty(prefix)) sb.Append(prefix);
        sb.Append(IntToHex(i - 16));
        sb.Append("  ");
        sb.Append(Get8BitString(buf));
        if (i < len) sb.Append("\r\n"); // Make sure we don't add an extra CRLF after the data.
        buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      }

      return sb.ToString();
    }


    private void Form1_DragDrop(object sender, DragEventArgs e) {
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0) ParseInputFile(files[0]);
    }

    private void Form1_DragEnter(object sender, DragEventArgs e) {
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

    private void Err(String msg) {
      //MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
      //  MessageBoxOptions.DefaultDesktopOnly, false);
      this.lbStatus.Text = "ERROR: " + msg;
      this.lbStatus.ForeColor = Color.Red;
    }


  


    private void textBox1_TextChanged(object sender, EventArgs e) {
      ParseInputText(this.textBox1.Text);
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
      byte[] b64Data = ParsePEM(Get8BitString(data));

      byte[] asnData = data;
      if (b64Data != null) asnData = b64Data;
      if (ParseASN1(asnData)) {
        this.textBox1.TextChanged -= this.textBox1_TextChanged;
        if (b64Data != null) {
          this.textBox1.Text = File.ReadAllText(file);
        } else {
          this.textBox1.Text = HexDump(data, 0, data.Length, "");
        }

        this.textBox1.TextChanged += this.textBox1_TextChanged;
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
        this.treeView1.Nodes.Clear();
        this.treeView1.Nodes.Add(CreateNode(a));
        this.lbStatus.Text = "Succeed.";
        this.lbStatus.ForeColor = Color.Green;
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
        int end = text.LastIndexOf("-----", text.Length - 5);
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

    private static byte[] DIGITS = null;
    private static bool[] B64MAP = null;
    private const long SIZE_100MB = 1024 * 1024 * 100L;
  }
}
