using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class Tokenizer {
    private string m_Text = "";
    private int m_Pos = 0;
    private int m_EndPos = 0;
    private List<string> m_Toks = new List<string>();
    private List<string> m_Segment = new List<string>();

    public Tokenizer(string text) {
      m_Text = text;
      m_Pos = 0;
      m_EndPos = text.Length;
    }

    public string Next() {
      if (m_Segment.Count == 0) FillSegments();
      if (m_Segment.Count == 0) return null;
      string ret = m_Segment[0];
      m_Segment.RemoveAt(0);
      return ret;
    }
    public string Peek() {
      if (m_Segment.Count == 0) FillSegments();
      if (m_Segment.Count == 0) return null;
      return m_Segment[0];
    }

    private void FillToks() {
      while (m_Toks.Count < 20) {
        if (m_Pos >= m_EndPos) return;

        int pos = m_Pos;
        char ch = m_Text[pos];

        // blank
        if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n') {
          while (m_Pos < m_EndPos && (m_Text[m_Pos] == ' ' || m_Text[m_Pos] == '\t' || m_Text[m_Pos] == '\r' || m_Text[m_Pos] == '\n')) m_Pos++;
          if (m_Pos >= m_EndPos) return;
          pos = m_Pos;
          ch = m_Text[pos];
        }

        // comments
        if (ch == '-' && pos + 1 < m_EndPos && m_Text[pos + 1] == '-') {
          m_Pos += 2;
          while (m_Pos < m_EndPos) {
            // 1. End with "--"
            if (m_Text[m_Pos] == '-' && m_Pos + 1 < m_EndPos && m_Text[m_Pos + 1] == '-') {
              m_Pos += 2;
              break;
            }
            // 2. End with '\n'
            else if (m_Text[m_Pos] == '\n') {
              m_Pos++;
              break;
            }
            m_Pos++;
          }
          if (m_Pos >= m_EndPos) return;
          continue;
        }

        if (ch >= '0' && ch <= '9') {
          while (m_Pos < m_EndPos && m_Text[m_Pos] >= '0' && m_Text[m_Pos] <= '9') m_Pos++;
          m_Toks.Add(m_Text.Substring(pos, m_Pos - pos));
          continue;
        }

        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
          while (m_Pos < m_EndPos && ((m_Text[m_Pos] >= 'a' && m_Text[m_Pos] <= 'z') ||
                                      (m_Text[m_Pos] >= 'A' && m_Text[m_Pos] <= 'Z') ||
                                      (m_Text[m_Pos] >= '0' && m_Text[m_Pos] <= '9') ||
                                      m_Text[m_Pos] == '-')) m_Pos++;
          m_Toks.Add(m_Text.Substring(pos, m_Pos - pos));
          continue;
        }

        if (ch == ':' && pos + 2 < m_EndPos && m_Text[pos + 1] == ':' && m_Text[pos + 2] == '=') {
          m_Pos += 3;
          m_Toks.Add(m_Text.Substring(pos, m_Pos - pos));
          continue;
        }
        if (ch == '.' && pos + 1 < m_EndPos && m_Text[pos + 1] == '.') {
          m_Pos += 2;
          m_Toks.Add(m_Text.Substring(pos, m_Pos - pos));
          continue;
        }

        m_Pos++;
        m_Toks.Add(m_Text.Substring(pos, 1));
      }
    }
    private string NextTok() {
      if (m_Toks.Count == 0) FillToks();
      if (m_Toks.Count == 0) return null;
      string ret = m_Toks[0];
      m_Toks.RemoveAt(0);
      return ret;
    }
    private string PeekTok() {
      if (m_Toks.Count == 0) FillToks();
      if (m_Toks.Count == 0) return null;
      return m_Toks[0];
    }

    private string FetchSize() {
      String ret = "SIZE (";
      NextTok();
      if (NextTok() != "(") throw new Exception("FetchSize: can not find '('");
      while (PeekTok() != ")") ret += NextTok();
      NextTok();
      ret += ")";
      return ret;
    }
    private void FillSegments() {
      string size = null;
      while (m_Segment.Count < 20) {
        string tok = this.NextTok();
        if (tok == null) return;
        switch (tok) {
          case "OBJECT":
            if (PeekTok() == "IDENTIFIER") m_Segment.Add("OBJECT " + NextTok());
            else m_Segment.Add("OBJECT");
            break;
          case "OCTET":
            if (PeekTok() == "STRING") m_Segment.Add("OCTET " + NextTok());
            else m_Segment.Add("OCTET");
            break;
          case "BIT":
            if (PeekTok() == "STRING") m_Segment.Add("BIT " + NextTok());
            else m_Segment.Add("BIT");
            break;
          case "DEFINED":
            if (PeekTok() == "BY") m_Segment.Add("DEFINED " + NextTok());
            else m_Segment.Add("DEFINED");
            break;
          case "SET":
            size = null;
            if (PeekTok() == "SIZE") size = FetchSize();
            if (PeekTok() == "OF") m_Segment.Add("SET " + NextTok());
            else m_Segment.Add("SET");
            if (size != null) m_Segment.Add(size);
            break;
          case "SEQUENCE":
            size = null;
            if (PeekTok() == "SIZE") size = FetchSize();
            if (PeekTok() == "OF") m_Segment.Add("SEQUENCE " + NextTok());
            else m_Segment.Add("SEQUENCE");
            if (size != null) m_Segment.Add(size);
            break;
          case "SIZE":
            m_Segment.Add(FetchSize());
            break;
          default:
            m_Segment.Add(tok);
            break;
        }
      }
    }
  }
}
