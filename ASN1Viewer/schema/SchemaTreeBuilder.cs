using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ASN1Viewer.schema {
  internal static class SchemaTreeBuilder {
    public static TreeNode Create(SchemaFile model) {
      string name = model.FileName;
      int pos = name.LastIndexOfAny(new char[] { '\\', '/'});
      if (pos >= 0) name = name.Substring(pos + 1);
      TreeNode ret = new TreeNode(String.Format("{0} ({1} {2})", name, model.Name, model.Oid.GetValue()));
      if (model.Types.Count > 0) {
        TreeNode types = new TreeNode(String.Format("Types ({0})", model.Types.Count));
        foreach (KeyValuePair<string, TypeDef> val in model.Types) {
          TreeNode p = Create(val.Value);
          if (p.Parent != null) types.Nodes.Add(p.Clone() as TreeNode);
          else types.Nodes.Add(p);
        }
        ret.Nodes.Add(types);
      }
      if (model.Oids.Count > 0) {
        TreeNode oids = new TreeNode(String.Format("OIDs ({0})", model.Oids.Count));
        foreach (KeyValuePair<string, OidDef> val in model.Oids) {
          oids.Nodes.Add(new TreeNode(String.Format("{0} {1}", val.Value.Name, val.Value.GetValue())));
        }
        ret.Nodes.Add(oids);
      }
      return ret;
    }

    public static TreeNode Create(TypeDef model) {
      TreeNode node = new TreeNode(String.Format("{0}({1})", model.TypeName, model.BaseTypeName));
      if (model.SequenceOfTypeName != null) {
        node.Nodes.Add(new TreeNode(model.SequenceOfTypeName));
      } else if (model.SequenceOfType != null) {
        node.Nodes.Add(Create(model.SequenceOfType));
      }
      if (model.Fields != null && model.Fields.Count > 0) {
        for (int i = 0; i < model.Fields.Count; i++) node.Nodes.Add(Create(model.Fields[i]));
      }
      return node;

    }

    public static TreeNode Create(FieldDef model) {
      TreeNode node = new TreeNode(String.Format("{0} {1}", model.Name, model.TypeName == null ? "" : model.TypeName));
      if (model.Type != null){
        TreeNode p = Create(model.Type);
        node.Nodes.Add(p);
        //for (int i = 0; i < p.Nodes.Count; i++) {
        //if (p.Parent != null) m_TreeNode.Nodes.Add(p.Nodes[i].Clone() as TreeNode);
          //else m_TreeNode.Nodes.Add(p.Nodes[i]);
        //}
      }
      return node;
    }
  }
}
