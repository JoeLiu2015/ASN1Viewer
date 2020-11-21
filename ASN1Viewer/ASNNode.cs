using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ASN1Viewer {
  public class ASNNode {
    // Bits 8 and 7 specify the class (see Table 2), bit 6 has value "0," indicating that the encoding is primitive, and bits 5-1 give the tag number.
    // Universal        - for types whose meaning is the same in all applications; these types are only defined in X.208.
    // Application      - for types whose meaning is specific to an application, such as X.500 directory services; types in two different applications may have the same application-specific tag and different meanings.
    // Private          - for types whose meaning is specific to a given enterprise.
    // Context-specific - for types whose meaning is specific to a given structured type; context-specific tags are used to distinguish between component types with the same underlying tag within the context of a given structured type, and component types in two different structured types may have the same tag and different meanings.
    public const int NODE_CLASS_UNIVERSAL   = 0x00; // 0000 0000
    public const int NODE_CLASS_APPLICATION = 0x40; // 0100 0000
    public const int NODE_CLASS_CONTEXT     = 0x80; // 1000 0000
    public const int NODE_CLASS_PRIVATE     = 0xC0; // 1100 0000

    public const int NODE_CLASS_MASK        = 0xC0; // 1100 0000
    public const int NODE_CONSTRUCTED_MASK  = 0x20; // 0010 0000 
    public const int NODE_TAG_NUMBER_MASK   = 0x1F; // 0001 1111 

    // Following are universal tags
    public const int UNIVERSAL_BOOLEAN          = 0x01;
    public const int UNIVERSAL_INTEGER          = 0x02;
    public const int UNIVERSAL_BITSTRING        = 0x03;
    public const int UNIVERSAL_OCTETSTRING      = 0x04;
    public const int UNIVERSAL_NULL             = 0x05;
    public const int UNIVERSAL_OID              = 0x06;
    public const int UNIVERSAL_REAL             = 0x09;
    public const int UNIVERSAL_SEQ_SEQOF        = 0x10;
    public const int UNIVERSAL_SET_SETOF        = 0x11;
    public const int UNIVERSAL_NUMSTRING        = 0x12;
    public const int UNIVERSAL_PRINTABLESTRING  = 0x13;
    public const int UNIVERSAL_T61STRING        = 0x14;
    public const int UNIVERSAL_VIDEOTEXSTRING   = 0x15;
    public const int UNIVERSAL_IA5STRING        = 0x16;
    public const int UNIVERSAL_UTCTIME          = 0x17;
    public const int UNIVERSAL_GENTIME          = 0x18;
    public const int UNIVERSAL_GRAPHIC_STR      = 0x19;
    public const int UNIVERSAL_ISO646STR        = 0x1A;
    public const int UNIVERSAL_GENERAL_STR      = 0x1B;
    public const int UNIVERSAL_STRING           = 0x1C;
    public const int UNIVERSAL_BMPSTRING        = 0x1E;

    private byte   m_Tag = 0x00;  
    private int    m_ElementStart = 0x00;
    private int    m_ElementEnd = 0x00;
    private int    m_ContentStart = 0x00;
    private int    m_ContentEnd = 0x00;
    private byte[] m_Data = null;
    private ASNNode m_FirstChild = null;
    private ASNNode m_Next = null;
    private TypeDef m_Schema = null;
    private string m_SchemaName = null;

    private ASNNode(byte tag, int elementStart, int elementEnd, int contentStart, int contentEnd, byte[] data) {
      m_Tag = tag;
      m_ElementStart = elementStart;
      m_ElementEnd = elementEnd;
      m_ContentStart = contentStart;
      m_ContentEnd = contentEnd;
      m_Data = data;

      System.Diagnostics.Debug.WriteLine("[" + elementStart + ", " + elementEnd + "] [" + contentStart + ", " + contentEnd + "]");
    }
    public int GetChildCount() {
      int count = 0;
      for (ASNNode n = m_FirstChild; n != null; n = n.m_Next) {
        count++;
      }
      return count;
    }
    public ASNNode GetChild(int idx) {
      int cur = 0;
      for (ASNNode n = m_FirstChild; n != null; n = n.m_Next, cur++) {
        if (cur == idx) return n;
      }
      return null;
    }
    public bool IsSequence() {
      return IsConstructed(m_Tag) && GetTagNum(m_Tag) == UNIVERSAL_SEQ_SEQOF;
    }
    public bool IsConstructed() {
      return IsConstructed(m_Tag);
    }
    public int GetClass() {
      return GetTagClass(m_Tag);
    }
    public int GetTagNum() {
      return GetTagNum(m_Tag);
    }

    public TypeDef Schema {
      get { return m_Schema;  }
    }

    public int Start { get { return m_ElementStart;  } }
    public int End { get { return m_ElementEnd; } }
    public int ContentStart { get { return m_ContentStart; } }
    public int ContentEnd   { get { return m_ContentEnd; } }

    private byte GetSchemaTag(string primeType, List<string> tag) {
      if (tag != null && tag.Count > 0) {
        if (tag.Count == 1) return (byte)int.Parse(tag[0]);
      }
      switch (primeType) {
        case "SEQUENCE":          return UNIVERSAL_SEQ_SEQOF;
        case "OCTET STRING":      return UNIVERSAL_OCTETSTRING;
        case "OBJECT IDENTIFIER": return UNIVERSAL_OID;
        case "BIT STRING":        return UNIVERSAL_BITSTRING;
        case "INTEGER":           return UNIVERSAL_INTEGER;
        case "BOOLEAN":           return UNIVERSAL_BOOLEAN;
        case "PrintableString":   return UNIVERSAL_PRINTABLESTRING;
        case "NumericString":     return UNIVERSAL_OCTETSTRING;
        case "IA5String":         return UNIVERSAL_IA5STRING;
        case "UTCTime":           return UNIVERSAL_UTCTIME;
        case "GeneralizedTime":   return UNIVERSAL_GENTIME;
        case "TeletexString":     return UNIVERSAL_T61STRING;
        case "SET":               return UNIVERSAL_SET_SETOF;
      }

      throw new Exception("Failed to get schema tag");
    }

    public bool MatchSchema(string name, TypeDef t) {
      bool b = MatchSchema(name, t.PrimeType, t.Tag, t);
      if (!b) {
        List<ASNNode> n = new List<ASNNode>();
        n.Add(this);
        while (n.Count > 0) {
          ASNNode an = n[n.Count - 1];
          n.RemoveAt(n.Count - 1);
          an.m_Schema = null;
          an.m_SchemaName = "";
          an = an.m_FirstChild;
          while (an != null) {
            n.Add(an);
            an = an.m_Next;
          }
        }
      }
      return b;
    }


    public bool MatchSchema(string name, string primeType, List<string> tag, TypeDef t) {
      if (t != null) {
        primeType = t.PrimeType;
      }
      

      if (tag == null && t != null) {
        tag = t.Tag;
      }

      if (primeType == "ANY" ) {
        if (m_FirstChild == null) {
          m_SchemaName = name;
          m_Schema = t;
          return true;
        } else {
          return false;
        }
      }
      if (primeType == "CHOICE") {
        for (int i = 0; i < t.Fields.Count; i++) {
          if (MatchSchema(t.Fields[i].Name, t.Fields[i].PrimeType, t.Fields[i].Tag, t.Fields[i].TypeObj)) {
            m_SchemaName = String.IsNullOrEmpty(name) ? t.Fields[i].Name : name;
            m_Schema = t.Fields[i].TypeObj;
            return true;
          }
        }

        return false;
      }

      if (GetSchemaTag(primeType, tag) != GetTagNum()) {
        return false;
      }

      if (tag != null && tag.Count > 0) {
        return m_FirstChild != null && m_FirstChild.MatchSchema(name, primeType, null, t);
      }

      if (primeType == "SEQUENCE") {
        int i = 0;
        ASNNode c = m_FirstChild;
        if (t.Fields == null) {
          while (c != null) {
            if (!c.MatchSchema(t.TypeName, t.TypeObj.PrimeType, t.TypeObj.Tag, t.TypeObj)) {
              return false;
            }

            c = c.m_Next;
          }
        } else {
          while (c != null && i < t.Fields.Count) {
            if (!t.Fields[i].Optional) {
              if (!c.MatchSchema(t.Fields[i].Name, t.Fields[i].PrimeType, t.Fields[i].Tag, t.Fields[i].TypeObj)) {
                return false;
              }

              i++;
              c = c.m_Next;
            } else {
              if (!c.MatchSchema(t.Fields[i].Name, t.Fields[i].PrimeType, t.Fields[i].Tag, t.Fields[i].TypeObj)) {
                i++;
              } else {
                i++;
                c = c.m_Next;
              }
            }
          }

          if (c == null) {
            for (; i < t.Fields.Count; i++) {
              if (!t.Fields[i].Optional)
                return false;
            }
          } else {
            return false;
          }
        }
      } else if (primeType == "SET") {
        ASNNode c = m_FirstChild;
        if (t.Fields == null) {
          while (c != null) {
            if (!c.MatchSchema(t.TypeName, t.TypeObj.PrimeType, t.TypeObj.Tag, t.TypeObj)) {
              return false;
            }
            c = c.m_Next;
          }
        } else {
          throw new Exception("TODO:");
        }
      }
      m_SchemaName = name;
      m_Schema = t;
      return true;
    }

    public override string ToString() {
      String str = "";
      if (m_SchemaName != null) {
        str = m_SchemaName + " ";
      }
      if (GetClass() != NODE_CLASS_UNIVERSAL) {
        if      (GetClass() == NODE_CLASS_APPLICATION)  str += "[Application][" + GetTagNum() + "]";
        else if (GetClass() == NODE_CLASS_CONTEXT)      str += "[Context]["     + GetTagNum() + "]";
        else if (GetClass() == NODE_CLASS_PRIVATE)      str += "[Private]["     + GetTagNum() + "]";
      } else {
        if      (GetTagNum() == UNIVERSAL_BOOLEAN)         str += "BOOLEAN "         + GetValue();
        else if (GetTagNum() == UNIVERSAL_INTEGER)         str += "INTEGER "         + GetValue();
        else if (GetTagNum() == UNIVERSAL_BITSTRING)       str += "BITSTRING "       + GetValue();
        else if (GetTagNum() == UNIVERSAL_OCTETSTRING)     str += "OCTETSTRING "     + GetValue();
        else if (GetTagNum() == UNIVERSAL_NULL)            str += "NULL";
        else if (GetTagNum() == UNIVERSAL_OID)             str += "OID "             + GetValue();
        else if (GetTagNum() == UNIVERSAL_SEQ_SEQOF)       str += "SEQUENCE "        + GetValue();
        else if (GetTagNum() == UNIVERSAL_SET_SETOF)       str += "SET "             + GetValue();
        else if (GetTagNum() == UNIVERSAL_PRINTABLESTRING) str += "PrintableString " + GetValue();
        else if (GetTagNum() == UNIVERSAL_T61STRING)       str += "T61String "       + GetValue();
        else if (GetTagNum() == UNIVERSAL_IA5STRING)       str += "IA5String "       + GetValue();
        else if (GetTagNum() == UNIVERSAL_UTCTIME)         str += "UTCTime "         + GetValue();
        else if (GetTagNum() == UNIVERSAL_GENTIME)         str += "GeneralizedTime " + GetValue();
        else if (GetTagNum() == UNIVERSAL_GRAPHIC_STR)     str += "GraphicString "   + GetValue();
        else if (GetTagNum() == UNIVERSAL_ISO646STR)       str += "ISO646String "    + GetValue();
        else if (GetTagNum() == UNIVERSAL_GENERAL_STR)     str += "GeneralString "   + GetValue();
        else if (GetTagNum() == UNIVERSAL_STRING)          str += "UniversalString " + GetValue();
        else if (GetTagNum() == UNIVERSAL_BMPSTRING)       str += "BMPString "       + GetValue();
      }
      if (m_FirstChild != null) {
        str += " (" + GetChildCount() + " elem)";
      }
      if (str.Length > 0) return str;
      else
        return  base.ToString();
    }

    public object GetValue() {
      
      if (GetChildCount() == 0) {
        byte[] d = new byte[m_ContentEnd - m_ContentStart];
        Array.Copy(m_Data, m_ContentStart, d, 0, d.Length);
        if (GetTagNum() == UNIVERSAL_INTEGER) {
          ulong val = 0;
          for (int i = 0; i < d.Length; i++) {
            val = (val << 8) | d[i];
          }
          return val.ToString();
        } else if (GetTagNum() == UNIVERSAL_BITSTRING) {
          int paddingCount = d[0];
          int bitCount = (d.Length - 1)*8 - paddingCount;
          return String.Format("({0} bits){1}", bitCount, Utils.HexEncode(d, 1, d.Length - 1));
        }
        else if (GetTagNum() == UNIVERSAL_OCTETSTRING)  return GetString(d);
        else if (GetTagNum() == UNIVERSAL_OID) {
          StringBuilder sb = new StringBuilder();
          sb.Append(String.Format("{0}.{1}", d[0]/40, d[0]%40));
          int pos = 1;
          long val = 0;
          while (pos < d.Length) {
            val = val*128 + (d[pos] & 0x7F);
            if ((d[pos] & 0x80) == 0) {
              sb.Append(".").Append(val);
              val = 0;
            }
            pos++;
          }
          if (val > 0) throw new Exception("Invalid OID");
          String oid = sb.ToString();
          return oid + " " + GetOIDName(oid) ;
        } 
        else if (GetTagNum() == UNIVERSAL_PRINTABLESTRING) return GetString(d);
        else if (GetTagNum() == UNIVERSAL_T61STRING) return GetString(d);
        else if (GetTagNum() == UNIVERSAL_IA5STRING) return GetString(d);
        else if (GetTagNum() == UNIVERSAL_UTCTIME) return ParseUTCTime(d);
        else if (GetTagNum() == UNIVERSAL_GENTIME) return ParseGeneralizedTime(d);
      }
      // For a constructed type, its value is empty.
      return "";
    }

    public void ParseChild() {
      if (!(IsConstructed() || (m_Tag == UNIVERSAL_OCTETSTRING && IsValidASN(m_Data, m_ContentStart, m_ContentEnd)))) return;

      byte[] data = m_Data;
      int start = m_ContentStart;
      int end = m_ContentEnd;
      if (start >= end) return;

      int retType = 0;
      int retContentStart = 0;
      int retContentEnd = 0;
      int retElementStart = 0;
      int retElementEnd = 0;

      if (!MeasureElement(data, start, end,
        ref retType,
        ref retContentStart,
        ref retContentEnd,
        ref retElementStart,
        ref retElementEnd)) {
        throw new Exception("Failed to parse child.");
      };

      m_FirstChild = new ASNNode((byte)retType, retElementStart, retElementEnd, retContentStart, retContentEnd, data);
      start = retElementEnd;

      ASNNode pre = m_FirstChild;
      while (start < end) {
        if (!MeasureElement(data, start, end,
          ref retType,
          ref retContentStart,
          ref retContentEnd,
          ref retElementStart,
          ref retElementEnd)) {
          throw new Exception("Failed to parse child.(next)");
        };

        pre.m_Next = new ASNNode((byte)retType, retElementStart, retElementEnd, retContentStart, retContentEnd, data);
        pre = pre.m_Next;
        start = retElementEnd;
      }
      if (start != end) throw new Exception("Invalid ASN data.");

      for (ASNNode n = m_FirstChild; n != null; n = n.m_Next) {
        n.ParseChild();
      }
    }

    //=====================================
    // Static members
    //=====================================
    public static int GetTagClass(byte tag) {
      return tag & NODE_CLASS_MASK;
    }
    public static int GetTagNum(byte tag) {
      return tag & NODE_TAG_NUMBER_MASK;
    }
    public static bool IsConstructed(byte tag) {
      return (tag & NODE_CONSTRUCTED_MASK) == NODE_CONSTRUCTED_MASK;
    }
    public static ASNNode Parse(byte[] asn) {
      int retType = 0;
      int retContentStart = 0;
      int retContentEnd = 0;
      int retElementStart = 0;
      int retElementEnd = 0;
      if (!MeasureElement(asn, 0, asn.Length, 
        ref retType, 
        ref retContentStart, 
        ref retContentEnd, 
        ref retElementStart,
        ref retElementEnd)) {
        throw new Exception("Invalid ASN.1 data.");
      };
      if (retElementEnd < asn.Length) {
        for (int i = retElementEnd; i < asn.Length; i++) {
          if (asn[i] != 0) throw new Exception("Invalid ASN.1 data. No zero bytes unparsed.");
        }
      }

      ASNNode node = new ASNNode((byte)retType, retElementStart, retElementEnd, retContentStart, retContentEnd, asn);
      node.ParseChild();
      return node;
    }


    private static String GetString(byte[] d) {
      if (Utils.IsPrintable(d)) return Utils.GetUtf8String(d);
      return "(" + d.Length + " bytes)" + Utils.HexEncode(d, 0, d.Length);

    }

    private static Dictionary<String, String> oids = null; 
    private static String GetOIDName(String oid) {
      if (oids == null) {
        String[] lines = File.ReadAllLines("oids.txt");
        oids = new Dictionary<String, String>();
        for (int i = 0; i < lines.Length; i++) {
          String[] parts = lines[i].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Length == 2) {
            oids.Add(parts[0], parts[1]);
          }
        }
      }
      if (oids.ContainsKey(oid)) return "(" + oids[oid] + ")";
      return "";
    }
    private static String ParseUTCTime(byte[] data) {
      //"YYMMDDhhmm[ss]Z" or "YYMMDDhhmm[ss](+|-)hhmm"
      String val = Encoding.ASCII.GetString(data);
      String year = val.Substring(0, 2);
      String mon = val.Substring(2, 2);
      String day = val.Substring(4, 2);
      String hour = val.Substring(6, 2);
      String min = val.Substring(8, 2);
      String second = "";
      String timezone = "";
      if (val.EndsWith("Z") && val.Length == 13) second = val.Substring(10, 2);
      if (val.Contains("+") || val.Contains("-")) {
        int pos = val.IndexOfAny(new char[]{'+', '-'});
        if (pos >= 12) second = val.Substring(10, 2);
        timezone = val.Substring(pos);
        if (timezone.Length == 5) timezone = timezone.Substring(0, 3) + ":" + timezone.Substring(3);
      }
      year = (int.Parse(year) < 70) ? "20" + year : "19" + year;
      return String.Format("{0}-{1}-{2} {3}:{4}{5}{6}", year, mon, day, hour, min, second.Length > 0 ? ":" + second : "",
                           timezone);

    }
    private static String ParseGeneralizedTime(byte[] data) {
      // 1. Local time only. ``YYYYMMDDHH[MM[SS[.fff]]]'', where the optional fff is accurate to three decimal places.
      // 2. Universal time (UTC time) only. ``YYYYMMDDHH[MM[SS[.fff]]]Z''.
      // 3. Difference between local and UTC times. ``YYYYMMDDHH[MM[SS[.fff]]]+-HHMM''.
      String val = Encoding.ASCII.GetString(data);
      String year = val.Substring(0, 4);
      String mon = val.Substring(6, 2);
      String day = val.Substring(8, 2);
      String hour = val.Substring(10, 2);
      String min = "", second = "";
      String ms = "";
      String timezone = "";
      if (val.Length >= 14 && char.IsDigit(val[12])) {
        min = val.Substring(12, 2);
        if (val.Length >= 16 && char.IsDigit(val[14])) {
          second = val.Substring(14, 2);
          if (val.Length >= 20 && val[16] == '.') ms = val.Substring(16, 4);
        }
      }
      if (val.Contains("+") || val.Contains("-")) {
        int pos = val.IndexOfAny(new char[] { '+', '-' });
        timezone = val.Substring(pos);
        if (timezone.Length == 5) timezone = timezone.Substring(0, 3) + ":" + timezone.Substring(3);
      }
      return String.Format("{0}-{1}-{2} {3}{4}{5}{6}{7}", year, mon, day, hour, min.Length > 0 ? ":" + min : "", second.Length > 0 ? ":" + second : "",
        ms.Length > 0 ? ms : "", timezone);

    }

    private static bool IsValidASN(byte[] lpData, int start, int end) {
      int retType = 0;
      int retContentStart = 0;
      int retContentEnd = 0;
      int retElementStart = 0;
      int retElementEnd = 0;
      if (!MeasureElement(lpData, start, end, ref retType, ref retContentStart, ref retContentEnd, ref retElementStart, ref retElementEnd)) return false;
      return retElementStart == start && retElementEnd == end;
    }
    private static bool MeasureElement(byte[] data, int startPos, int endPos, ref int retType, ref int retContentStart, ref int retContentEnd, ref int retElementStart, ref int retElementEnd) {
      int pos = startPos;
      int len = 0;
      bool indefiniteLen = false;
      retElementStart = pos;

      retType = data[pos++] & 0xFF;  // skip type byte

      int lengthByte = data[pos++] & 0xFF;
      if (retType == 0x00 && lengthByte == 0x00) {
        retContentStart = pos;
        retContentEnd = pos;
        retElementEnd = pos;
        return true;
      }
      if ((lengthByte & 0x80) > 0) {  // multiple bytes length
        lengthByte = (char)(lengthByte & ~0x80);
        if (lengthByte > 4) return false;
        else if (lengthByte == 0) indefiniteLen = true; // infinite length
        else {
          for (int i = 0; i < lengthByte; i++) { len = (len << 8) | (data[pos++] & 0xFF); }
        }
      } else {
        len = lengthByte;
      }

      if (len < 0) return false;

      retContentStart = pos;
      if (indefiniteLen) {
        if ((retType & 0x20) == 0x20) {
          int t = 0;
          int cStart = 0;
          int cEnd = 0;
          int eStart = 0;
          int eEnd = 0;
          int s = pos, e = endPos;
          while (s < e && MeasureElement(data, s, e, ref t, ref cStart, ref cEnd, ref eStart, ref eEnd)) {
            if (t == 0x00 && eEnd - eStart == 2) { // 0x00, 0x00
              retContentEnd = eEnd - 2;
              retElementEnd = eEnd;
              return true;
            } else {
              s = eEnd;
            }
          }
          return false;
        } else {
          for (int i = pos; i < endPos - 1; i++) {
            byte b1 = data[i];
            byte b2 = data[i + 1];
            if (((b1 & ~0x20) == 0) && ((b2 & ~0x20) == 0)) {
              retContentEnd = i;
              retElementEnd = i + 2;
              return true;
            }
          }
          return false;
        }
      } else {
        if (pos + len <= endPos) {
          retContentEnd = pos + len;
          retElementEnd = pos + len;
          return true;
        } else {
          return false;
        }
      }
    }
  }
}
