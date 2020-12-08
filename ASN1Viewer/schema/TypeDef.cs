using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class TypeDef {
    private string  m_TypeName = null;
    private string  m_BaseTypeName = null;
    private TypeDef m_BaseType = null;
    private bool    m_Implicit = false;
    private int     m_Tag      = -1;

    // Used for SEQUENCE OF
    private SizeDef m_SeqOfSize     = null;
    private string  m_SeqOfTypeName = null;
    private TypeDef m_SeqOfType     = null;

    // Used for SET,CHOICE,SEQUENCE
    private List<FieldDef> m_Fields = null;

    // STRING or INTEGER size
    private SizeDef m_ValSize       = null;
    private bool    m_SizeFixed     = false;
    
    private EnumDef m_Enum          = null;
    

    public TypeDef(string name, bool isImplicit) {
      m_TypeName = name;
      m_Implicit = isImplicit;
    }

    public string TypeName {
      get { return m_TypeName; }
    }

    public string BaseTypeName {
      get { return m_BaseTypeName; }
    }

    public string GetPrimeType() {
      if (Utils.IsPrimeType(m_BaseTypeName)) return m_BaseTypeName;
      if (m_BaseType != null) return m_BaseType.GetPrimeType();
      throw new Exception("Failed to get the prime type of '" + m_BaseTypeName + "'");
    }

    public void FixValue(Dictionary<string, TypeDef> vals) {
      if (m_BaseType == null) {
        if (!Utils.IsPrimeType(m_BaseTypeName)) {
          m_BaseType = vals[m_BaseTypeName];
        }
      }

      if (m_SeqOfType == null && m_SeqOfTypeName != null) {
        if (!Utils.IsPrimeType(m_SeqOfTypeName)) {
          m_SeqOfType = vals[m_SeqOfTypeName];
        }
      }

      if (m_Fields != null) {
        for (int i = 0; i < m_Fields.Count; i++) m_Fields[i].FixValue(vals);
      }
    }

    public void FixSize(Dictionary<string, int> vals) {
      if (m_SizeFixed) return;
      m_SizeFixed = true;

      if (m_SeqOfSize != null) m_SeqOfSize.FixValue(vals);
      if (m_ValSize != null) m_ValSize.FixValue(vals);
      if (m_Fields != null) {
        for (int i = 0; i < m_Fields.Count; i++) m_Fields[i].FixSize(vals);
      }
    }

    public void Parse(Tokenizer tok) {
      if (tok.Peek() == "[") ParseTag(tok);
      if (tok.Peek() == "IMPLICIT" || tok.Peek() == "EXPLICIT") m_Implicit = (tok.Next() == "IMPLICIT");
      if (tok.Peek() == "SEQUENCE" || tok.Peek() == "SET" || tok.Peek() == "CHOICE") {
        m_BaseTypeName = tok.Next();
        if (tok.Peek() == "{") {
          ParseSeqFields(tok);
        } else {
          if (tok.Peek() == "SIZE") m_SeqOfSize = SizeDef.Parse(tok);
          tok.Skip("OF");
          string next2 = tok.Peek(2);
          if (next2 == "SEQUENCE{" || next2 == "SET{" || next2 == "CHOICE{") {
            m_SeqOfType = new TypeDef("", m_Implicit);
            m_SeqOfType.Parse(tok);
          } else {
            m_SeqOfTypeName = tok.Next();
          }
        }
      } else {
        m_BaseTypeName = tok.Next();
      }

      if ((m_BaseTypeName == "INTEGER" || m_BaseTypeName == "BIT STRING" || m_BaseTypeName == "ENUMERATED") && tok.Peek() == "{") {
        if (m_BaseTypeName == "ENUMERATED") m_BaseTypeName = "INTEGER";
        m_Enum = EnumDef.Parse(tok);
      }

      if (m_BaseTypeName == "OBJECT IDENTIFIER" && tok.Peek() == "(") {
        // Ignore something like: PolicyQualifierId ::= OBJECT IDENTIFIER ( id-qt-cps | id-qt-unotice )
        while (tok.Next() != ")") ;
      }

      if (tok.Peek(2) == "(SIZE") {
        tok.Skip("(");
        m_ValSize = SizeDef.Parse(tok);
        tok.Skip(")");
      } else if (tok.Peek() == "(" && tok.Peek(3).EndsWith("..")) {
        m_ValSize = SizeDef.Parse(tok, false);
      }


    }

    private void ParseTag(Tokenizer tok) {
      tok.Skip("[");
      if (tok.Peek() == "UNIVERSAL") tok.Next();
      if (tok.Peek() == "APPLICATION") tok.Next();
      string tag = tok.Next();
      m_Tag = int.Parse(tag);
      tok.Skip("]");
    }

    private void ParseSeqFields(Tokenizer tok) {
      m_Fields = new List<FieldDef>();
      tok.Skip("{");
      do {
        FieldDef fd = new FieldDef();
        fd.Parse(tok);
        m_Fields.Add(fd);
      } while (tok.Peek() != "}");
      tok.Skip("}");
    }

    

    public override string ToString() {
      return String.Format("{0}: {1}", m_TypeName, m_BaseTypeName);
    }
  }
}
