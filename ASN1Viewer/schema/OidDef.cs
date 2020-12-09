using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class OidDef {
    // { 0 9 2342 19200300 100 1 25 }
    // { iso(1) identified-organization(3) dod(6) internet(1) security(5) mechanisms(5) pkix(7) }
    // { pkcs-9 1 }
    private List<OidPart> m_Parts  = new List<OidPart>(); 
    private string m_Name = "";

    public string GetValue() {
      return ToString();
    }
    public string Name {
      get { return m_Name; }
    }

    public void FixValue(Dictionary<string, OidDef> oids) {
      for (int i = 0; i < m_Parts.Count; i++) {
        if (m_Parts[i].Value == null) m_Parts[i].Value = oids[m_Parts[i].Name].GetValue();
      }
    }

    public static OidDef Parse(string name, Tokenizer tok) {
      try {
        OidDef oid = new OidDef();
        oid.m_Name = name;
        tok.Skip("{");
        string word = tok.Next();
        while (true) {
          if (word == "}") break;
          OidPart p = new OidPart();
          if (IsInt(word))  p.Value = word;
          else {
            p.Name = word;
            if (tok.Peek() == "(") {
              tok.Skip("(");
              string val = tok.Next();
              tok.Skip(")");
              if (!IsInt(val)) throw new Exception("Expect int value but not.");
              p.Value = val;
            }
          }
          oid.m_Parts.Add(p);
          word = tok.Next();
        }
        return oid;
      } catch (Exception ex) {
        throw new Exception("Failed to parse OID: " + ex.Message);
      }
    }

    private static bool IsInt(string s) {
      return s[0] >= '0' && s[0] <= '9';
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < m_Parts.Count; i++) {
        OidPart op = m_Parts[i];
        if (sb.Length > 0) sb.Append(".");
        sb.Append(op.Value == null ? op.Name : op.Value);
      }
      return sb.ToString();
    }
  }
  class OidPart {
    public string Name = null;
    public string Value = null;
  }
}
