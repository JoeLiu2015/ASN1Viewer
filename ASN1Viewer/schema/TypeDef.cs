using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public class TypeDef {
    private string  m_TypeName = null;
    private string  m_BaseTypeName = null;
    private TypeDef m_BaseType = null;
    private bool    m_Implicit = false;
    private int     m_Tag      = -1;

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


    public void Parse(Tokenizer tok) {
      if (tok.Peek() == "[") ParseTag(tok);
      if (tok.Peek() == "IMPLICIT" || tok.Peek() == "EXPLICIT") m_Implicit = (tok.Next() == "IMPLICIT");

      if (IsPrimeType(tok.Peek())) m_BaseTypeName = tok.Next();


    }

    private void ParseTag(Tokenizer tok) {
      tok.Skip("[");
      if (tok.Peek() == "UNIVERSAL") tok.Next();
      m_Tag = int.Parse(tok.Next());
      tok.Skip("]");
    }

    private bool IsPrimeType(string typeName) {
      return typeName == "OCTET STRING" ||
             typeName == "OBJECT IDENTIFIER" ||
             typeName == "BIT STRING" ||
             typeName == "INTEGER" ||
             typeName == "BOOLEAN" ||
             typeName == "PrintableString" ||
             typeName == "NumericString" ||
             typeName == "IA5String" ||
             typeName == "ANY" ||
             typeName == "UTCTime" ||
             typeName == "GeneralizedTime" ||
             typeName == "TeletexString";
    }

    public override string ToString() {
      return String.Format("{0}: {1}", m_TypeName, m_BaseTypeName);
    }
  }
}
