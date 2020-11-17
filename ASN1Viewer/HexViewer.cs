using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer
{
  public class HexViewer : RichTextBox {

    public HexViewer() {
      this.ReadOnly = true;
      this.BackColor = SystemColors.Window;
      this.ForeColor = Color.Gray;
      this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    }

    private byte[] m_Data = null;
    private int m_Start = 0;
    private int m_End = 0;

    public byte[] Data {
      get { return this.m_Data; }
      set {
        m_Data = value;
        if (value == null || m_Data.Length == 0) {
          this.Text = "";
          return;
        }
        this.Text = Utils.HexDump(value, 0, value.Length, "");
      }
    }



    public void SelectNode(int start, int end, int contentStart, int contentEnd) {
      if (m_End > m_Start) {
        this.Select(m_Start, m_End - m_Start);
        this.SelectionColor = ForeColor;
        m_Start = m_End = 0;
      }
      if (end > start) {
        SetColor(start, 1, Color.Red);
        SetColor(start + 1, contentStart - start - 1, Color.Blue);
        SetColor(contentEnd, end - contentEnd, Color.Blue);
        SetColor(contentStart, contentEnd - contentStart, Color.Green);

        m_Start = start / 16 * LINE_LEN;
        m_End = end / 16 * LINE_LEN + LINE_LEN;
        this.SelectionStart = m_Start;
        this.ScrollToCaret();
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
      this.Select(offset2, len2);
      this.SelectionColor = c;
    }

    public const int LINE_LEN    = 8 /*Offset*/ + 3 /*Separators*/ + 16 * 3 /*Bytes*/ + 3 /*Separators*/ + (8 + 1 + 8) /*Text*/ + 1 /*LF*/;
    public const int BYTE_OFFSET = 8 /*Offset*/ + 3 /*Separators*/;
    public const int TXT_OFFSET  = 8 /*Offset*/ + 3 /*Separators*/ + 16 * 3 /*Bytes*/ + 3 /*Separators*/;

    public void SetColor(int pos, int len, Color c) {
      if (len <= 0) return;
      int startLine = pos / 16;
      int endLine   = (pos + len) / 16;
      int startOffset = pos % 16;
      int endOffset   = (pos + len) % 16;

      if (startLine == endLine) {
        SetColor(startLine, startOffset, len, c);
        return;
      }
      SetColor(startLine, startOffset, 16 - startOffset, c);
      SetColor(endLine, 0, endOffset, c);
      for (int i = startLine + 1; i < endLine; i++) SetColor(i, 0, 16, c);
    }
  }
  
}
