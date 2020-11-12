using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASN1Viewer {

  public class TypeDef {
    public string           Name       = "";
    public string           TypeName   = "";
    public List<FieldDef>   Fields     = null;
    public List<string>     Tag        = null;
    public List<string>     Other      = null;
    public string           Collection = null;
    public object           Size       = null;
    public Dictionary<string, int> IntEnums = null;

    public string          PrimeType   = "";
    public TypeDef         TypeObj     = null;

    public override string ToString() {
      return Collection == null ? Name + ": " + TypeName : Name + ": " + Collection + " " + TypeName;
    }
  }

  public class FieldDef {
    public string Name     = "";
    public string TypeName = "";
    public bool   Optional = false;
    public string Collection = "";
    public object Size = null;
    public List<string> Tag = null;
    public List<string> Other = null;

    public string  PrimeType = "";
    public TypeDef TypeObj   = null;

    public override string ToString() {
      return Name + ": " + TypeName;
    }
  }




  public class Schema {
    private Dictionary<string, TypeDef> m_Types = new Dictionary<string, TypeDef>();
    private Dictionary<string, string>  m_Oids = new Dictionary<string, string>();
    private Dictionary<string, object>  m_Consts = new Dictionary<string, object>();

    public void Add(string file) {
      string s = File.ReadAllText(file);
      SchemaTokenizer tok = new SchemaTokenizer(s);

      List<string> words = new List<string>();

      string word = tok.Next();
      while (word != null) {
        if (word == "::=") {
          if (words.Count == 2 && IsOID(words[1])) {
            ParseOid(words[0], tok);
          } else if (words.Count == 2 && words[1] == "INTEGER") {
            int val = int.Parse(tok.Next());
            m_Consts.Add(words[0], val);
          } else if (words.Count == 1) {
            ParseTypeDef(words[0], tok);
          } else {
            throw new Exception("Failed to parse expression");
          }

          words.Clear();
        } else {
          words.Add(word);
        }

        word = tok.Next();
      }

      if (!m_Consts.ContainsKey("MAX")) m_Consts["MAX"] = int.MaxValue;
      foreach (KeyValuePair<string, TypeDef> kv in m_Types) {
        if (kv.Value.Size != null) kv.Value.Size = FixSize(kv.Value.Size);
        if (kv.Value.Fields == null) continue;
        for (int i = 0; i < kv.Value.Fields.Count; i++) {
          if (kv.Value.Fields[i].Size != null) kv.Value.Fields[i].Size = FixSize(kv.Value.Fields[i].Size);
        }
      }

      foreach (KeyValuePair<string, TypeDef> kv in m_Types) {
        InitPrimeType(kv.Value);
        InitTypeObj(kv.Value);
      }
    }

    public string GetOidName(string oid) {
      if (m_Oids.ContainsKey(oid)) return m_Oids[oid];
      return null;
    }

    public Dictionary<string, TypeDef> Types {
      get { return m_Types; }
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

    private string GetPrimeType(string typeName) {
      if (IsPrimeType(typeName)) return typeName;
      if (typeName == "SEQUENCE" || typeName == "SET" || typeName == "CHOICE") return typeName;
      if (m_Types.ContainsKey(typeName)) {
        return GetPrimeType(m_Types[typeName].TypeName);
      }

      throw new Exception("Can not get the prime type for '" + typeName + "'");
    }
    private void InitPrimeType(TypeDef td) {
      if (td.Collection == "SEQUENCE OF") {
        td.PrimeType = "SEQUENCE";
      } else if (td.Collection == "SET OF") {
        td.PrimeType = "SET";
      } else {
        td.PrimeType = GetPrimeType(td.TypeName);
      }

      if (td.Fields != null) {
        if (td.TypeName != "SEQUENCE" && td.TypeName != "SET" && td.TypeName != "CHOICE")
          throw new Exception("Fields only appear in SEQUENCE, SET and CHOICE.");
        for (int i = 0; i < td.Fields.Count; i++) {
          td.Fields[i].PrimeType = GetPrimeType(td.Fields[i].TypeName);
        }
      }
    }

    private void InitTypeObj(TypeDef td) {
      if (IsPrimeType(td.TypeName)) return;
      
      if (td.TypeName == "SEQUENCE" || td.TypeName == "SET" || td.TypeName == "CHOICE") {
        for (int i = 0; i < td.Fields.Count; i++) {
          if (IsPrimeType(td.Fields[i].TypeName)) continue;
          if (m_Types.ContainsKey(td.Fields[i].TypeName)) {
            td.Fields[i].TypeObj = m_Types[td.Fields[i].TypeName];
            continue;
          }
          throw new Exception("Failed to get TypeObj for the field '" + td.Fields[i].Name + "'");
        }

        return;
      }
      if (m_Types.ContainsKey(td.TypeName)) {
        td.TypeObj = m_Types[td.TypeName];
        return;
      }

      throw new Exception("Can not get the type obj");
    }

    private object FixSize(object size) {
      string[] s = (string[]) size;
      int[] ret = new int[2];
      int val = 0;
      if (s[0] != null) {
        if (int.TryParse(s[0], out val)) ret[0] = val;
        else                             ret[0] = (int)m_Consts[s[0]];
      }
      if (s[1] != null) {
        if (int.TryParse(s[1], out val)) ret[1] = val;
        else                             ret[1] = (int)m_Consts[s[1]];
      }
      return ret;
    }
    private void ParseOid(string name, SchemaTokenizer tok) {
      string value = "";
      string word = tok.Next();
      if (word != "{") throw new Exception("Invalid OID");
      word = tok.Next();
      while (true) {
        if (word == "}") break;
        string next = tok.Next();
        if (next == "(") {
          if (value.Length > 0) value += ".";
          value += tok.Next();
          if (tok.Next() != ")") throw new Exception("Invalid OID");
          word = tok.Next();
        } else if (m_Oids.ContainsKey(word)) {
          value = m_Oids[word];
          word = next;
        } else {
          if (IsInt(word)) {
            if (value.Length > 0) value += ".";
            value += word;
            word = next;
          } else {
            throw new Exception("Invalid OID");
          }
        }
      }

      m_Oids.Add(name, value);
    }

    private void ParseFieldDef(TypeDef td, SchemaTokenizer tok) {
      FieldDef fd = new FieldDef();
      fd.Name = tok.Next();

      while (true) {
        string w = tok.Next();
        if (w == "}") {
          td.Fields.Add(fd);
          break;
        }
        if (w == ",") {
          td.Fields.Add(fd);
          fd = new FieldDef();
          fd.Name = tok.Next();
          continue;
        }

        if (w == "[") {
          w = tok.Next();
          fd.Tag = new List<string>();
          while (w != "]") {
            fd.Tag.Add(w);
            w = tok.Next();
          }
          continue;
        }
        if (w == "IMPLICIT" || w == "EXPLICIT") {
          if (fd.Other == null) fd.Other = new List<string>();
          fd.Other.Add(w);
          continue;
        }
        if (w == "SET OF" || w == "SEQUENCE OF") {
          fd.Collection = w;
        } else if (w == "OPTIONAL") {
          fd.Optional = true;
        } else if (w == "(SIZE") {
          fd.Size = ParseSize(tok);
          if (tok.Next() != ")") throw new Exception("Invalid Field Size");
        } else if (w == "DEFAULT") {
          tok.Next();  // ignore  default value like "Version DEFAULT v1,"
          fd.Optional = true;
        } else if (w == "SEQUENCE" || w == "SET") {
          if (tok.Peek() == "{") {
            fd.TypeName = w + "_" + fd.Name;
            tok.Push(w);
            ParseTypeDef(fd.TypeName, tok);
          } else {
            throw new Exception("TODO:");
          }
        } else if (w == "ANY") {
          fd.TypeName = w;
          if (tok.Peek() == "DEFINED BY") {
            tok.Next();
            tok.Next(); // ignore "DEFINED BY algorithm"
          }
        } else if (w == "INTEGER") {
          fd.TypeName = w;
          if (tok.Peek() == "(") {
            fd.Size = ParseSize(tok);
          }
        } else {
          fd.TypeName = w;
        }
        
      }
    }

    private string[] ParseSize(SchemaTokenizer tok) {
      string[] ret = new string[2];
      if (tok.Next() != "(") throw new Exception("Invalid Size");
      ret[0] = tok.Next();
      string next = tok.Next();
      if (next == ")") {
        ret[1] = null;
        return ret;
      }
      if (next != "..") throw new Exception("Invalid Size");
      ret[1] = tok.Next();
      if (tok.Next() != ")") throw new Exception("Invalid Size");
      return ret;
    }

    private Dictionary<string, int> ParseIntEnum(SchemaTokenizer tok) {
      if (tok.Next() != "{") throw new Exception("Failed to parse IntEnum {");
      Dictionary<string, int> ret = new Dictionary<string, int>();
      string name = "";
      int val = 0;
      while (true) {
        name = tok.Next();
        if (tok.Next() != "(") throw new Exception("Failed to parse IntEnum (");
        val = int.Parse(tok.Next());
        if (tok.Next() != ")") throw new Exception("Failed to parse IntEnum )");
        ret.Add(name, val);
        if (tok.Peek() == ",") {
          tok.Next();
          continue;
        } else if (tok.Peek() == "}") {
          tok.Next();
          if (tok.Peek() == "(") {
            // Skip range (1..ub-integer-options)
            while (tok.Next() != ")");
          }
          break;
        } else {
          throw new Exception("Failed to parse IntEnum " + tok.Peek());
        }
      }
      return ret;
    }
    private bool IsInt(string val) {
      try {
        int.Parse(val);
        return true;
      } catch (Exception ex) {
        return false;
      }
    }
    private bool IsOID(string type) {
      if (type == "OBJECT IDENTIFIER") return true;
      while (m_Types.ContainsKey(type)) type = m_Types[type].TypeName;
      return type == "OBJECT IDENTIFIER";
    }
    private void ParseTypeDef(string name, SchemaTokenizer tok) {
      TypeDef td = new TypeDef();
      td.Name = name;

      string word = null;
      while (true) {
        word = tok.Next();
        if (word == null) return;
        if (word == "[") {
          word = tok.Next();
          td.Tag = new List<string>();
          while (word != "]") {
            td.Tag.Add(word);
            word = tok.Next();
          }
          continue;
        }

        if (word == "IMPLICIT" || word == "EXPLICIT") {
          if (td.Other == null) td.Other = new List<string>();
          td.Other.Add(word);
          continue;
        }
        if (word == "SET OF" || word == "SEQUENCE OF") {
          td.Collection = word;
          continue;
        }

        if (word == "INTEGER" && tok.Peek() == "{") {
          td.TypeName = word;
          td.IntEnums = ParseIntEnum(tok);
          break;
        }

        if (IsPrimeType(word)) {
          td.TypeName = word;
          if (tok.Peek() == "(SIZE") {
            tok.Next();
            ParseSize(tok);
            if (tok.Next() != ")") throw new Exception("Invalid Size");
          }
          break;
        }

        if (word == "SEQUENCE" || word == "CHOICE" || word == "SET") {
          td.TypeName = word;
          string next = tok.Next();
          if (next == "{") {
            td.Fields = new List<FieldDef>();
            ParseFieldDef(td, tok);
          } else {
            throw new Exception("Failed to parse SEQUENCE,CHOICE,SET at '" + next + "'");
          }
          
          break;
        }

        td.TypeName = word;
        if (tok.Peek() == "(SIZE") {
          tok.Next();
          td.Size = ParseSize(tok);
          if (tok.Next() != ")") throw new Exception("Failed to parse size in ParseTypeDef");
        }
        if (tok.Peek() == "{") {
          throw new Exception("TODO: ParseType(" + name + ":" + word + ")");
        }

        break;
      }

      m_Types.Add(td.Name, td);
    }


  }


  public class SchemaTokenizer {
    private string m_Text = "";
    private int m_Pos = 0;
    private int m_EndPos = 0;
    private string m_NextTok = null;
    private List<string> m_Next = new List<string>();

    public SchemaTokenizer(string text) {
      m_Text = text;
      m_Pos = 0;
      m_EndPos = text.Length;
      m_NextTok = null;
    }

    private string PeekTok() {
      if (m_NextTok == null) m_NextTok = NextTok();
      return m_NextTok;
    }
    private string NextTok() {
      if (m_NextTok != null) {
        string ret = m_NextTok;
        m_NextTok = null;
        return ret;
      }
      if (m_Pos >= m_EndPos) return null;

RESTART:
      int pos = m_Pos;
      char ch = m_Text[pos];


      // blank
      if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n') {
        while (m_Pos < m_EndPos && (m_Text[m_Pos] == ' ' || m_Text[m_Pos] == '\t' || m_Text[m_Pos] == '\r' || m_Text[m_Pos] == '\n')) m_Pos++;
        if (m_Pos >= m_EndPos) return null;
        pos = m_Pos;
        ch = m_Text[pos];
      }

      // comments
      if (ch == '-' && pos + 1 < m_EndPos && m_Text[pos + 1] == '-') {
        m_Pos += 2;
        while (m_Pos < m_EndPos) {
          // 1. End with "--"
          if (m_Text[m_Pos] == '-' && m_Pos + 1 < m_EndPos && m_Text[m_Pos + 1] == '-') {
            m_Pos += 2;
            break;
          } 
          // 2. End with '\n'
          else if (m_Text[m_Pos] == '\n') {
            m_Pos++;
            break;
          }
          m_Pos++;
        }
        if (m_Pos >= m_EndPos) return null;
        goto RESTART;
      }

      if (ch >= '0' && ch <= '9') {
        while (m_Pos < m_EndPos && m_Text[m_Pos] >= '0' && m_Text[m_Pos] <= '9') m_Pos++;
        return m_Text.Substring(pos, m_Pos - pos);
      }

      if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
        while (m_Pos < m_EndPos && ((m_Text[m_Pos] >= 'a' && m_Text[m_Pos] <= 'z') || 
                                    (m_Text[m_Pos] >= 'A' && m_Text[m_Pos] <= 'Z') ||
                                    (m_Text[m_Pos] >= '0' && m_Text[m_Pos] <= '9') ||
                                    m_Text[m_Pos] == '-')) m_Pos++;
        return m_Text.Substring(pos, m_Pos - pos);
      }

      if (ch == ':' && pos + 2 < m_EndPos && m_Text[pos + 1] == ':' && m_Text[pos + 2] == '=') {
        m_Pos += 3;
        return m_Text.Substring(pos, m_Pos - pos);
      }
      if (ch == '.' && pos + 1 < m_EndPos && m_Text[pos + 1] == '.') {
        m_Pos += 2;
        return m_Text.Substring(pos, m_Pos - pos);
      }

      m_Pos++;
      return m_Text.Substring(pos, 1);
    }

    public void Push(string w) {
      m_Next.Insert(0, w);
    }
    public string Peek() {
      if (m_Next.Count == 0) m_Next.Add(Next());
      return m_Next[0];
    }
    public string Next() {
      if (m_Next.Count > 0) {
        string ret = m_Next[0];
        m_Next.RemoveAt(0);
        return ret;
      }

      string tok = this.NextTok();
      if (tok == null) return tok;
      switch (tok) {
        case "OBJECT":
          if (PeekTok() == "IDENTIFIER") return "OBJECT " + NextTok();
          break;
        case "OCTET":
          if (PeekTok() == "STRING") return "OCTET " + NextTok();
          break;
        case "SET":
          if (PeekTok() == "SIZE") {
            NextTok();
            if (NextTok() != "(") throw new Exception("Invalid Size after SET");
            while (NextTok() != ")");
          }
          if (PeekTok() == "OF") return "SET " + NextTok();
          break;
        case "SEQUENCE":
          if (PeekTok() == "SIZE") {
            NextTok();
            if (NextTok() != "(") throw new Exception("Invalid Size after SEQUENCE");
            while (NextTok() != ")") ;
          }
          if (PeekTok() == "OF") return "SEQUENCE " + NextTok();
          break;
        case "(":
          if (PeekTok() == "SIZE") return "(" + NextTok();
          break;
        case "DEFINED":
          if (PeekTok() == "BY") return "DEFINED " + NextTok();
          break;
        case "BIT":
          if (PeekTok() == "STRING") return "BIT " + NextTok();
          break;
      }
      return tok;
    }
  }
}
