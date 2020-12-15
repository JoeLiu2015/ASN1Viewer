using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  public class TypeDef : ISchemaNode {
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

    public string Name {
      get { return m_TypeName; }
    }

    public string TypeName {
      get { return m_TypeName; }
    }

    public string BaseTypeName {
      get { return m_BaseTypeName; }
    }

    public int Tag {
      get { return m_Tag; }
    }

    public string GetPrimeType() {
      if (Utils.IsPrimeType(m_BaseTypeName)) return m_BaseTypeName;
      if (m_BaseType != null) return m_BaseType.GetPrimeType();
      throw new Exception("Failed to get the prime type of '" + m_BaseTypeName + "'");
    }
    public List<FieldDef> GetBaseFileds() {
      if (m_Fields == null && m_BaseType != null) return m_BaseType.GetBaseFileds();
      return m_Fields;
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

    public void FixTag() {
      if (m_Tag >= 0) return;
      if (m_BaseTypeName == "CHOICE" || m_BaseTypeName == "ANY") goto FIX_CHILD;
      if (Utils.IsPrimeType(m_BaseTypeName)) {
        m_Tag = Utils.GetPrimeTypeTag(m_BaseTypeName);
        goto FIX_CHILD;
      }
      if (m_BaseType != null) {
        m_BaseType.FixTag();
        m_Tag = m_BaseType.m_Tag;
        goto FIX_CHILD;
      }
      
FIX_CHILD:
      if (m_Fields == null) {
        string primeType = GetPrimeType();
        if (primeType == "SEQUENCE" || primeType == "SET" || primeType == "CHOICE") {
          m_Fields = GetBaseFileds();
        }
      }
      if (m_Fields != null && m_Fields.Count > 0) {
        for (int i = 0; i < m_Fields.Count; i++) m_Fields[i].FixTag();
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

    public bool Match(IASNNode asnNode, bool setSchema) {
      return Match(asnNode, setSchema, m_Tag);
    }
    public bool Match(IASNNode asnNode, bool setSchema, int tagVal) {
      string primeType = GetPrimeType();
      if (primeType == "ANY") {
        if (setSchema) asnNode.Schema = this;
        return true;
      }
      if (primeType == "CHOICE") {
        for (int i = 0; i < m_Fields.Count; i++) {
          if (m_Fields[i].Match(asnNode, setSchema)) {
            if (setSchema) asnNode.Schema = m_Fields[i];
            return true;
          }
        }
        return false;
      }
      if (tagVal != asnNode.Tag) {
        return false;
      }
      if (m_SeqOfTypeName != null || m_SeqOfType != null) {
        if (m_SeqOfType != null) {
          for (int i = 0; i < asnNode.ChildCount; i++) {
            if (!m_SeqOfType.Match(asnNode.GetChild(i), setSchema)) {
              return false;
            }
          }
          return true;
        } else {
          int tag = Utils.GetPrimeTypeTag(m_SeqOfTypeName);
          for (int i = 0; i < asnNode.ChildCount; i++) {
            if (tag != asnNode.GetChild(i).Tag) {
              return false;
            }
          }
          return true;
        }
      }
      if (m_Fields != null && m_Fields.Count > 0) {
        if (primeType == "SEQUENCE") {
          int j = 0;
          for (int i = 0; i < asnNode.ChildCount; i++) {
            bool matched = false;
            while (j < m_Fields.Count) {
              if (m_Fields[j].Match(asnNode.GetChild(i), setSchema)) {
                j++;
                matched = true;
                break;
              } else {
                if (!m_Fields[j].IsOptional) {
                  return false;
                } else {
                  j++;
                }
              }
            }
            if (!matched) {
              return false;
            }
          }
          for (; j < m_Fields.Count; j++) {
            if (!m_Fields[j].IsOptional) {
              return false;
            }
          }
        } else if (primeType == "SET") {
          List<FieldDef> matchedFields = new List<FieldDef>();
          for (int i = 0; i < asnNode.ChildCount; i++) {
            bool matched = false;
            for (int j = 0; !matched && j < m_Fields.Count; j++) {
              if (m_Fields[j].Match(asnNode.GetChild(i), setSchema)) {
                if (!matchedFields.Contains(m_Fields[j])) matchedFields.Add(m_Fields[j]);
                matched = true;
              }
            }
            if (!matched) {
              return false;
            }
          }
          for (int j = 0; j < m_Fields.Count; j++) {
            if (!matchedFields.Contains(matchedFields[j]) && !matchedFields[j].IsOptional) {
              return false;
            }
          }
          return true;
        } else {
          throw new Exception("Impossible.");
        }
      }
      if (setSchema) asnNode.Schema = this;
      return true;
    }
    public TreeNode ExportToTreeNode() {
      TreeNode node = new TreeNode(String.Format("{0}({1})", m_TypeName, m_BaseTypeName));
      if (m_SeqOfTypeName != null) {
        node.Nodes.Add(new TreeNode(m_SeqOfTypeName));
      } else if (m_SeqOfType != null) {
        node.Nodes.Add(m_SeqOfType.ExportToTreeNode());
      }
      if (m_Fields != null && m_Fields.Count > 0) {
        for (int i = 0; i < m_Fields.Count; i++) node.Nodes.Add(m_Fields[i].ExportToTreeNode());
      }
      return node;

    }

    private void ParseTag(Tokenizer tok) {
      tok.Skip("[");
      int val = 0;
      if (tok.Peek() == "UNIVERSAL")        { val = 0x00; tok.Next(); }  // 0000 0000
      else if (tok.Peek() == "APPLICATION") { val = 0x40; tok.Next(); }  // 0100 0000
      else if (tok.Peek() == "PRIVATE")     { val = 0xC0; tok.Next(); }  // 1100 0000
      else                                  { val = 0x80;             }  // 1000 0000
      string tag = tok.Next();
      m_Tag = int.Parse(tag);
      m_Tag |= val;
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
