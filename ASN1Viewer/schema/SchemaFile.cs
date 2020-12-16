using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace ASN1Viewer.schema {
  public class SchemaFile {
    private static Dictionary<string, SchemaFile> SCHEMA_FILES = new Dictionary<string, SchemaFile>();
    public  static Dictionary<string, SchemaFile> PARSED_SCHEMA = new Dictionary<string, SchemaFile>();
    private string m_FileName = "";
    private string m_Name     = "";
    private OidDef m_Oid      = null;
    private bool   m_Implicit = false;
    private Dictionary<string, TypeDef> m_Types  = new Dictionary<string, TypeDef>();
    private Dictionary<string, OidDef>  m_Oids   = new Dictionary<string, OidDef>();
    private Dictionary<string, int>     m_Consts = new Dictionary<string, int>();
    private Dictionary<string, object>  m_Imports = new Dictionary<string, object>();

    public Dictionary<string, TypeDef> Types {
      get { return m_Types; }
    }

    public object GetTypeValue(string name) {
      if (m_Types.ContainsKey(name))  return m_Types[name];
      if (m_Oids.ContainsKey(name))   return m_Oids[name];
      if (m_Consts.ContainsKey(name)) return m_Consts[name];
      throw new Exception(String.Format("Failed to find the type '{0}' in the schema '{1}'", name, m_Name));
    }

    public static Dictionary<string, SchemaFile> Schemas {
      get {
        if (SCHEMA_FILES.Count > 0) return SCHEMA_FILES;
        if (Directory.Exists(".\\files\\Asn1Modules")) {
          string[] files = Directory.GetFiles(".\\files\\Asn1Modules");
          for (int i = 0; i < files.Length; i++) {
            FileInfo fi = new FileInfo(files[i]);
            try {
              SchemaFile sf = SchemaFile.ParseFrom(fi.FullName);
              SCHEMA_FILES.Add(fi.FullName, sf);
            } catch (Exception ex) {
              
            }
          }
        }
        return SCHEMA_FILES;
      }
    }

    public static SchemaFile ParseFrom(string file) {
      file = new FileInfo(file).FullName;
      if (PARSED_SCHEMA.ContainsKey(file)) return PARSED_SCHEMA[file];

      SchemaFile sf = new SchemaFile();
      sf.Parse(file);
      SCHEMA_FILES[sf.m_Name] = sf;
      PARSED_SCHEMA[file] = sf;
      return sf;
    }

    public void Parse(string file) {
      m_FileName = file;
      Tokenizer tok = new Tokenizer(File.ReadAllText(file));
      m_Name = tok.Next();
      m_Oid  = OidDef.Parse(m_Name, tok);
      tok.Skip("DEFINITIONS");
      string imp = tok.Next();
      if (imp == "::=") goto BEGIN;
      if (imp != "IMPLICIT" && imp != "EXPLICIT") throw new Exception(String.Format("Failed to parse file '{0}', expect 'IMPLICIT' or 'EXPLICIT'.", file));
      m_Implicit = (imp == "IMPLICIT");
      tok.Skip("TAGS");
      tok.Skip("::=");
BEGIN:
      tok.Skip("BEGIN");

      if (tok.Peek() == "IMPORTS") {
        ImportDef import = ImportDef.Parse(tok);
        for (int i = 0; i < import.Count; i++) {
          string[] importTypes = import.GetImportTypes(i);
          string   docName     = import.GetDocName(i);
          if (!SCHEMA_FILES.ContainsKey(docName)) {
            FileInfo f = new FileInfo(file);
            FileInfo[] fs = Directory.GetParent(f.FullName).GetFiles();
            for (int j = 0; j < fs.Length; j++) {
              if (fs[j].FullName.EndsWith(docName + ".txt")) {
                SchemaFile sfi = new SchemaFile();
                sfi.Parse(fs[j].FullName);
                SCHEMA_FILES.Add(docName, sfi);
                PARSED_SCHEMA.Add(fs[j].FullName, sfi);
                break;
              }
            }
          }
          SchemaFile schemaFile = SCHEMA_FILES[docName];
          for (int m = 0; m < importTypes.Length; m++) {
            object val = schemaFile.GetTypeValue(importTypes[m]);
            if (val is int) m_Consts.Add(importTypes[m], (int)val);
            else if (val is OidDef) m_Oids.Add(importTypes[m], (OidDef)val);
            else if (val is TypeDef) m_Types.Add(importTypes[m], (TypeDef)val);
            else throw new Exception("Unknown import type '" + importTypes[m] + "'");
          }
        }
      }

      string[] words = tok.ReadTo("::=");
      while (words.Length > 0) {
        if (words.Length == 2 && IsOID(words[1])) {
          OidDef od = OidDef.Parse(words[0], tok);
          od.FixValue(m_Oids);
          m_Oids.Add(words[0], od);
        } else if (words.Length == 2 && words[1] == "INTEGER") {
          m_Consts.Add(words[0], int.Parse(tok.Next()));
        } else if (words.Length == 1) {
          TypeDef t = new TypeDef(words[0], m_Implicit);
          t.Parse(tok);
          m_Types.Add(t.TypeName, t);
        } else {
          throw new Exception("Failed to parse type.");
        }

        if (tok.Peek() == "END") break;
        else  words = tok.ReadTo("::=");
      }
      tok.Skip("END");

      foreach (KeyValuePair<string, TypeDef> ts in m_Types) {
        ts.Value.FixValue(m_Types);
        ts.Value.FixSize(m_Consts);
      }
      foreach (KeyValuePair<string, TypeDef> ts in m_Types) {
        ts.Value.FixTag();
      }
    }

    public TreeNode ExportToTreeNode() {
      string name = m_FileName;
      int pos = name.LastIndexOfAny(new char[] { '\\', '/'});
      if (pos >= 0) name = name.Substring(pos + 1);
      TreeNode ret = new TreeNode(String.Format("{0} ({1} {2})", name, m_Name, m_Oid.GetValue()));
      if (m_Types.Count > 0) {
        TreeNode types = new TreeNode(String.Format("Types ({0})", m_Types.Count));
        foreach (KeyValuePair<string, TypeDef> val in m_Types) {
          TreeNode p = val.Value.ExportToTreeNode();
          if (p.Parent != null) types.Nodes.Add(p.Clone() as TreeNode);
          else types.Nodes.Add(p);
        }
        ret.Nodes.Add(types);
      }
      if (m_Oids.Count > 0) {
        TreeNode oids = new TreeNode(String.Format("OIDs ({0})", m_Oids.Count));
        foreach (KeyValuePair<string, OidDef> val in m_Oids) {
          oids.Nodes.Add(new TreeNode(String.Format("{0} {1}", val.Value.Name, val.Value.GetValue())));
        }
        ret.Nodes.Add(oids);
      }
      return ret;
    }

    private bool IsOID(string typeName) {
      if (typeName == "OBJECT IDENTIFIER") return true;
      while (m_Types.ContainsKey(typeName)) typeName = m_Types[typeName].BaseTypeName;
      return typeName == "OBJECT IDENTIFIER";
    }
  }
}
