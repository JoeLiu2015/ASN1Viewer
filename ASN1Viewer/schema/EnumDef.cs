using System;
using System.Collections.Generic;

namespace ASN1Viewer.schema {
  public class EnumDef {
    private Dictionary<string, int> m_EnumVals = new Dictionary<string, int>();

    public static EnumDef Parse(Tokenizer tok) {
      EnumDef ed = new EnumDef();
      tok.Skip("{");
      do {
        string name = tok.Next();
        tok.Skip("(");
        int val = int.Parse(tok.Next());
        tok.Skip(")");
        ed.m_EnumVals.Add(name, val);
      } while (tok.Next() != "}");
      return ed;
    }
  }
}
