using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class FieldDef {
    private string m_FieldName = null;
    private string m_TypeName  = null;
    private int    m_Tag       = -1;
    private bool   m_Implicit  = false;
    private bool   m_Optional  = false;
    private string m_Default   = null;

    private TypeDef m_Type     = null;

    private SizeDef m_Size     = null;


    public void FixValue(Dictionary<string, TypeDef> vals) {
      if (m_Type == null && m_TypeName != null) {
        if (!Utils.IsPrimeType(m_TypeName)) {
          m_Type = vals[m_TypeName];
        }
      }
    }
    public void FixSize(Dictionary<string, int> vals) {
      if (m_Size != null) m_Size.FixValue(vals);
      if (m_Type != null) m_Type.FixSize(vals);
    }
    public void Parse(Tokenizer tok) {
      m_FieldName = tok.Next();

      if (tok.Peek() == "[") ParseTag(tok);

      if (tok.Peek() == "IMPLICIT" || tok.Peek() == "EXPLICIT") m_Implicit = (tok.Next() == "IMPLICIT");

      if (tok.Peek() == "SEQUENCE" || tok.Peek() == "SET" || tok.Peek() == "CHOICE" || tok.Peek(2) == "ENUMERATED{") {
        m_Type = new TypeDef("", m_Implicit);
        m_Type.Parse(tok);
      } else {
        m_TypeName = tok.Next();
      }

      if (m_TypeName == "INTEGER" && tok.Peek() == "(") {
        m_Size = SizeDef.Parse(tok, false);
      } else if (m_TypeName == "ANY" && tok.Peek() == "DEFINED BY") {
        tok.Next();
        tok.Next(); // skip it.
      } else if (tok.Peek(2) == "(SIZE") {
        tok.Skip("(");
        m_Size = SizeDef.Parse(tok);
        tok.Skip(")");
      }

      if (tok.Peek() == "OPTIONAL") { m_Optional = true; tok.Next(); }

      if (tok.Peek() == "DEFAULT") {
        tok.Next();
        bool hasBracket = false;
        if (tok.Peek() == "{") { hasBracket = true; tok.Next(); }
        m_Default = tok.Next();
        if (hasBracket) tok.Skip("}");
        m_Optional = true;
      }

      if (tok.Peek() == ",") {
        tok.Next();
      } else if (tok.Peek() == "}") {

      } else {
        throw new Exception("Failed to parse field '" + m_FieldName + "'");
      }
    }

    private void ParseTag(Tokenizer tok) {
      tok.Skip("[");
      if (tok.Peek() == "UNIVERSAL") tok.Next();
      m_Tag = int.Parse(tok.Next());
      tok.Skip("]");
    }
  }
}
