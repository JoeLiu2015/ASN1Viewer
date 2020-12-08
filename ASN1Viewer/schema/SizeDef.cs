using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class SizeDef {
    private string m_Low;
    private string m_High;

    private int m_LowInt;
    private int m_HighInt;


    public void FixValue(Dictionary<string, int> vals) {
      if (!int.TryParse(m_Low, out m_LowInt))   m_LowInt  = vals[m_Low];
      if (!int.TryParse(m_High, out m_HighInt)) {
        if (m_High == "MAX") m_HighInt = int.MaxValue;
        else m_HighInt = vals[m_High];
      }
    }

    public static SizeDef Parse(Tokenizer tok) {
      return Parse(tok, true);
    }

    public static SizeDef Parse(Tokenizer tok, bool hasSize) {
      SizeDef sd = new SizeDef();
      if (hasSize) tok.Skip("SIZE");
      tok.Skip("(");
      sd.m_Low = tok.Next();
      if (tok.Peek() == ")") {
        sd.m_High = sd.m_Low;
        tok.Next();
      } else {
        tok.Skip("..");
        sd.m_High = tok.Next();
        tok.Skip(")");
      }
      return sd;
    }
  }
}
