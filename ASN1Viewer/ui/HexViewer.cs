using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer.ui {
  public class HexViewer : RichTextBox {
    private const int LINE_LEN    = 8 /*Offset*/ + 3 /*Separators*/ + 16 * 3 /*Bytes*/ + 3 /*Separators*/ + (8 + 1 + 8) /*Text*/ + 1 /*LF*/;
    private const int BYTE_OFFSET = 8 /*Offset*/ + 3 /*Separators*/;
    private const int TXT_OFFSET  = 8 /*Offset*/ + 3 /*Separators*/ + 16 * 3 /*Bytes*/ + 3 /*Separators*/;
    private const int EMPTY_LINE_LEN  = LINE_LEN - 1; // Get rid of LF
    private const int SKIP_TXT_OFFSET = 25;
    private static readonly string EMPTY_LINE = "\r\n" + new string(' ', EMPTY_LINE_LEN) + "\r\n";  
    private static readonly Color HIGHLIGHT = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xB3);

    private List<Block> m_Blocks = new List<Block>();
    private int m_SelectionStart = 0;
    private int m_SelectionEnd   = 0;

    private int m_SelectedStart = 0;
    private int m_SelectedEnd = 0;
    private byte[] m_SelectedBytes = null;

    public HexViewer() {
      this.ReadOnly = true;
      this.BackColor = SystemColors.Window;
      this.ForeColor = Color.Gray;
      this.Font = new System.Drawing.Font("YouYuan", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      if (this.Font.Name != "YouYuan") {
        this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      }
      this.WordWrap = false;
    }
    public int  BlockCount {  get { return m_Blocks.Count;  } }
    public void ClearData() { m_Blocks.Clear(); }
    public void AddData(byte[] data, int position, int len) {
      Block b = new Block();
      b.Data = Utils.CopyBytes(data, position, len);
      b.Position = position;
      m_Blocks.Add(b);
    }
    public void RefreshView() {
      if (m_Blocks.Count == 1) {
        this.Text = Utils.HexDump(m_Blocks[0].Data, 0);
        m_Blocks[0].Line = 0;
        return;
      }
      int line = 0;
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < m_Blocks.Count; i++) {
        m_Blocks[i].Line = line;
        sb.Append(Utils.HexDump(m_Blocks[i].Data, m_Blocks[i].Position));
        line += (m_Blocks[i].EndPosition - m_Blocks[i].Position + 15) / 16;
        if (i + 1 < m_Blocks.Count) {
          int skip = m_Blocks[i + 1].Position - m_Blocks[i].EndPosition;
          string text = String.Format(Lang.T["TXT_SKIP"], skip, skip / 16);
          sb.Append(EMPTY_LINE).
             Append(new string(' ', SKIP_TXT_OFFSET)).Append(text).Append(new string(' ', EMPTY_LINE_LEN - text.Length - SKIP_TXT_OFFSET)).
             Append(EMPTY_LINE);
          line += 3;
        }
      }
      this.Text = sb.ToString();
    }
    public void SelectNode(int tagLen, int start, int end, int contentStart, int contentEnd, byte[] data) {
      m_SelectedBytes = data;
      m_SelectedStart = start;
      m_SelectedEnd = end;

      if (m_SelectionEnd > m_SelectionStart) {
        this.Select(m_SelectionStart, m_SelectionEnd - m_SelectionStart);
        this.SelectionColor = ForeColor;
        this.SelectionBackColor = BackColor;
        m_SelectionStart = m_SelectionEnd = 0;
      }
      if (end > start) {
        SetColor(start, tagLen, Color.Red);
        SetColor(start + tagLen, contentStart - start - tagLen, Color.Blue);
        SetColor(contentEnd, end - contentEnd, Color.Blue);
        SetColor(contentStart, contentEnd - contentStart, Color.Green);
        m_SelectionStart = BytesPos2Line(start) * LINE_LEN;
        m_SelectionEnd = BytesPos2Line(end - 1) * LINE_LEN + LINE_LEN - 1;
        this.SelectionStart = m_SelectionStart;
        this.ScrollToCaret();
      }
    }

    protected override void WndProc(ref Message m)
    { 
      // Disable selection
      const int WM_SETFOCUS = 0x0007;
      const int WM_KILLFOCUS = 0x0008;
      if (m.Msg == WM_SETFOCUS)
        m.Msg = WM_KILLFOCUS;

      base.WndProc(ref m);
    }
    protected override void OnMouseUp(MouseEventArgs mevent) {
      base.OnMouseUp(mevent);
      if (mevent.Button == MouseButtons.Right) {
        this.ShowContextMenu(mevent.Location);
      }
    }
   
    private void SetColor(int line, int offset, int len, Color c) {
      if (len == 0) return;
      int offset1 = LINE_LEN * line + BYTE_OFFSET + offset * 3;
      int len1 = len * 3;
      int offset2 = LINE_LEN * line + TXT_OFFSET + offset;
      int len2 = len;
      if (offset >= 8) offset2++;
      if (offset < 8 && offset + len2 > 8) len2 += 1; 

      this.Select(offset1, len1);
      this.SelectionColor = c;
      this.SelectionBackColor = HIGHLIGHT;
      this.Select(offset2, len2);
      this.SelectionColor = c;
      this.SelectionBackColor = HIGHLIGHT;
    }
    private void SetColor(int pos, int len, Color c) {
      if (len <= 0) return;
      int startLine = BytesPos2Line(pos);
      int endLine = BytesPos2Line(pos + len - 1);
      int startOffset = pos % 16;
      int endOffset = (pos + len - 1) % 16;

      if (startLine == endLine) {
        SetColor(startLine, startOffset, len, c);
        return;
      }

      SetColor(startLine, startOffset, 16 - startOffset, c);
      SetColor(endLine, 0, endOffset + 1, c);
      for (int i = startLine + 1; i < endLine; i++) SetColor(i, 0, 16, c);
    }
    private int BytesPos2Line(int pos) {
      for (int i = 0; i < m_Blocks.Count; i++) {
        if (pos >= m_Blocks[i].Position && pos < m_Blocks[i].EndPosition) {
          return m_Blocks[i].Line + (pos - m_Blocks[i].Position) / 16;
        }
      }
      throw new Exception("BytesPos2Line: Impossible");
    }
    private void ShowContextMenu(Point location) {
      ContextMenuStrip cm = null;
      ToolStripMenuItem copy = null;
      if (this.ContextMenuStrip == null) {
        cm = new ContextMenuStrip(); //make a context menu instance
        copy = new ToolStripMenuItem(); //make a menuitem instance
        cm.Items.Add(copy);//add the item to the context menu
        copy.Click += (sender, args) => {
          Clipboard.SetText(Utils.GetHexString(m_SelectedBytes, m_SelectedStart, m_SelectedEnd - m_SelectedStart));
        };
      } else {
        cm = this.ContextMenuStrip;
        copy = cm.Items[0] as ToolStripMenuItem;
      }

      copy.Text = Lang.T["RICH_CTX_MENU_COPY"];//give the item a header

      cm.Show(this, location);//show the context menu
    }
  }

  class Block {
    public int    Position  = 0;
    public int    Line      = 0;
    public byte[] Data = null;

    public int    EndPosition {
      get { return Position + Data.Length;  }
    }
  }
  
}
