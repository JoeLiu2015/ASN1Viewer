using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASN1Viewer.schema {
  public class SchemaFile {
    private static Dictionary<string, SchemaFile> SCHEMA_FILES = new Dictionary<string, SchemaFile>();
    private string m_Name     = "";
    private OidDef m_Oid      = null;
    private bool   m_Implicit = false;
    private Dictionary<string, TypeDef> m_Types  = new Dictionary<string, TypeDef>();
    private Dictionary<string, OidDef>  m_Oids   = new Dictionary<string, OidDef>();
    private Dictionary<string, int>     m_Consts = new Dictionary<string, int>();
    private Dictionary<string, object>  m_Imports = new Dictionary<string, object>();


    public object GetTypeValue(string name) {
      if (m_Types.ContainsKey(name))  return m_Types[name];
      if (m_Oids.ContainsKey(name))   return m_Oids[name];
      if (m_Consts.ContainsKey(name)) return m_Consts[name];
      throw new Exception(String.Format("Failed to find the type '{0}' in the schema '{1}'", name, m_Name));
    }

    public void Parse(string file) {
      Tokenizer tok = new Tokenizer(File.ReadAllText(file));
      m_Name = tok.Next();
      m_Oid  = OidDef.Parse(m_Name, tok);
      tok.Skip("DEFINITIONS");
      string imp = tok.Next();
      if (imp != "IMPLICIT" && imp != "EXPLICIT") throw new Exception(String.Format("Failed to parse file '{0}', expect 'IMPLICIT' or 'EXPLICIT'.", file));
      m_Implicit = (imp == "IMPLICIT");
      tok.Skip("TAGS");
      tok.Skip("::=");
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
                break;
              }
            }
          }
          SchemaFile schemaFile = SCHEMA_FILES[docName];
          for (int m = 0; m < importTypes.Length; m++) {
            m_Imports.Add(importTypes[m], schemaFile.GetTypeValue(importTypes[m]));
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
          throw new Exception("Failed to parse expression");
        }

        words = tok.ReadTo("::=");
      }
    }

    private bool IsOID(string typeName) {
      if (typeName == "OBJECT IDENTIFIER") return true;
      while (m_Types.ContainsKey(typeName)) typeName = m_Types[typeName].BaseTypeName;
      return typeName == "OBJECT IDENTIFIER";
    }
  }
}
