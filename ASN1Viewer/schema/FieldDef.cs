using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  public class FieldDef : ISchemaNode {
    private string m_FieldName = null;
    private string m_TypeName  = null;
    private int    m_Tag       = -1;
    private bool   m_Implicit  = false;
    private bool   m_Optional  = false;
    private string m_Default   = null;
    private bool   m_TagSpecified = false;

    private TypeDef m_Type     = null;

    private SizeDef m_Size     = null;

    public string Name {
      get {  return m_FieldName; }
    }
    public bool IsOptional {
      get { return m_Optional; }
    }

    public bool HasExplicitTag {
      get { return m_TagSpecified && !m_Implicit && m_Tag > 0; } 
    }

    public bool HasImplicitTag {
      get { return m_TagSpecified && m_Implicit && m_Tag > 0; }
    }

    public void FixValue(Dictionary<string, TypeDef> vals) {
      if (m_Type == null && m_TypeName != null) {
        if (!Utils.IsPrimeType(m_TypeName)) {
          m_Type = vals[m_TypeName];
        }
      }
      if (m_Type != null && m_TypeName == null) {
        m_Type.FixValue(vals);
      }
    }
    public void FixTag() {
      if (m_Tag >= 0) {
        if (m_TagSpecified && !m_Implicit) m_Tag |= ASNNode.NODE_CONSTRUCTED_MASK;
        if (m_TagSpecified && m_Implicit) {
          if (m_Type != null && (m_Type.GetPrimeType() == "SEQUENCE" || m_Type.GetPrimeType() == "SET")) {
            m_Tag |= ASNNode.NODE_CONSTRUCTED_MASK;
          } else if (m_TypeName == "SEQUENCE" || m_TypeName == "SET") {
            m_Tag |= ASNNode.NODE_CONSTRUCTED_MASK;
          }
        }
        return;
      }
      if (m_TypeName == "CHOICE" || m_TypeName == "ANY") {

      } else if (Utils.IsPrimeType(m_TypeName)) {
        m_Tag = Utils.GetPrimeTypeTag(m_TypeName);
        return;
      }
      if (m_Type != null) {
        m_Type.FixTag();
        m_Tag = m_Type.Tag;
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

    public bool Match(IASNNode asnNode, bool setSchema) {
      bool ret = false;
      IASNNode node = asnNode;
      if (HasExplicitTag) {
        if (m_Tag != asnNode.Tag) return false;
        if (asnNode.ChildCount != 1) return false;
        node = asnNode.GetChild(0);
      }
      
      if (m_Type != null) {
        if (HasImplicitTag) ret = m_Type.Match(node, setSchema, m_Tag);
        else                ret = m_Type.Match(node, setSchema);
        
      } else {
        if (m_TypeName == "ANY") {
          if (setSchema) asnNode.Schema = this;
          return true;
        }
        int tag = m_Tag;
        if (HasExplicitTag)  tag = Utils.GetPrimeTypeTag(m_TypeName);
        ret = (tag == node.Tag || (tag | ASNNode.NODE_CONSTRUCTED_MASK) == node.Tag);
      }
      if (ret && setSchema) asnNode.Schema = this;
      return ret;
    }
    public TreeNode ExportToTreeNode() {
      TreeNode node = new TreeNode(String.Format("{0} {1}", m_FieldName, m_TypeName == null ? "" : m_TypeName));
      if (m_Type != null){
        TreeNode p = m_Type.ExportToTreeNode();
        node.Nodes.Add(p);
        //for (int i = 0; i < p.Nodes.Count; i++) {
        //if (p.Parent != null) m_TreeNode.Nodes.Add(p.Nodes[i].Clone() as TreeNode);
          //else m_TreeNode.Nodes.Add(p.Nodes[i]);
        //}
      }
      return node;
    }
    private void ParseTag(Tokenizer tok) {
      m_TagSpecified = true;
      tok.Skip("[");
      int val = 0;
      if      (tok.Peek() == "UNIVERSAL")   { val = 0x00; tok.Next(); }  // 0000 0000
      else if (tok.Peek() == "APPLICATION") { val = 0x40; tok.Next(); }  // 0100 0000
      else if (tok.Peek() == "PRIVATE")     { val = 0xC0; tok.Next(); }  // 1100 0000
      else                                  { val = 0x80;             }  // 1000 0000
      string v = tok.Next();
      m_Tag = int.Parse(v);
      m_Tag |= val;
      tok.Skip("]");
    }
  }
}
