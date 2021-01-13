﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASN1Viewer {
  public class ASNNode : schema.IASNNode {
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
    public const int UNIVERSAL_OBJ_DESCRIPTOR   = 0x07;
    public const int UNIVERSAL_EXTERNAL         = 0x08;
    public const int UNIVERSAL_REAL             = 0x09;
    public const int UNIVERSAL_ENUMERATED       = 0x0A;
    public const int UNIVERSAL_UTF8_STR         = 0x0C;
    public const int UNIVERSAL_RELATIVE_OID     = 0x0D;
    public const int UNIVERSAL_SEQ_SEQOF        = 0x10;
    public const int UNIVERSAL_SET_SETOF        = 0x11;
    public const int UNIVERSAL_NUMSTRING        = 0x12;
    public const int UNIVERSAL_PRINTABLESTRING  = 0x13;
    public const int UNIVERSAL_T61STRING        = 0x14;  // TeletexString 
    public const int UNIVERSAL_VIDEOTEXSTRING   = 0x15;
    public const int UNIVERSAL_IA5STRING        = 0x16;
    public const int UNIVERSAL_UTCTIME          = 0x17;
    public const int UNIVERSAL_GENTIME          = 0x18;
    public const int UNIVERSAL_GRAPHIC_STR      = 0x19;
    public const int UNIVERSAL_ISO646STR        = 0x1A; // VisibleString 
    public const int UNIVERSAL_GENERAL_STR      = 0x1B;
    public const int UNIVERSAL_STRING           = 0x1C;
    public const int UNIVERSAL_BMPSTRING        = 0x1E;

    private byte   m_Tag          = 0x00;  
    private int    m_ElementStart = 0x00;
    private int    m_ElementEnd   = 0x00;
    private int    m_ContentStart = 0x00;
    private int    m_ContentEnd   = 0x00;
    private byte[] m_Data         = null;

    private List<ASNNode> m_Chidren    = null;
    private schema.ISchemaNode m_SchemaNode = null;

    private ASNNode(byte tag, int elementStart, int elementEnd, int contentStart, int contentEnd, byte[] data) {
      m_Tag = tag;
      m_ElementStart = elementStart;
      m_ElementEnd = elementEnd;
      m_ContentStart = contentStart;
      m_ContentEnd = contentEnd;
      m_Data = data;
      //System.Diagnostics.Debug.WriteLine("[" + elementStart + ", " + elementEnd + "] [" + contentStart + ", " + contentEnd + "]");
    }

    public int ChildCount {
      get { return m_Chidren == null ? 0 : m_Chidren.Count; }
    }
    public schema.IASNNode GetChild(int idx) {
      if (m_Chidren == null || idx < 0 || idx >= m_Chidren.Count) return null;
      return m_Chidren[idx];
    }

    public schema.ISchemaNode Schema {
      set {
        m_SchemaNode = value;
      }
    }

    public ASNNode this[int idx] {
      get {
        if (idx < 0 || idx >= ChildCount) return null;
        return m_Chidren[idx];
      }
    }

    public byte[] Value {
      get {
        return Utils.CopyBytes(m_Data, m_ContentStart, m_ContentEnd - m_ContentStart);
      }
    }

    public bool IsSequence    { get { return IsConstructed && TagNum == UNIVERSAL_SEQ_SEQOF;           } }
    public bool IsConstructed { get { return (m_Tag & NODE_CONSTRUCTED_MASK) == NODE_CONSTRUCTED_MASK; } }
    public int TagClass       { get { return m_Tag & NODE_CLASS_MASK;      } }
    public int TagNum         { get { return m_Tag & NODE_TAG_NUMBER_MASK; } }
    public int Tag            { get { return m_Tag;                        } }
    public int Start          { get { return m_ElementStart;               } }
    public int End            { get { return m_ElementEnd;                 } }
    public int ContentStart   { get { return m_ContentStart;               } }
    public int ContentEnd     { get { return m_ContentEnd;                 } }


    public override string ToString() {
      String str = "";
      if (m_SchemaNode != null) {
        str = m_SchemaNode.Name + " ";
      }
      if (TagClass != NODE_CLASS_UNIVERSAL) {
        if      (TagClass == NODE_CLASS_APPLICATION)  str += "[Application][" + TagNum + "]";
        else if (TagClass == NODE_CLASS_CONTEXT)      str += "[Context]["     + TagNum + "]";
        else if (TagClass == NODE_CLASS_PRIVATE)      str += "[Private]["     + TagNum + "]";
        if (ChildCount == 0) str += "" + GetString(Value);
      } else {
        if      (TagNum == UNIVERSAL_BOOLEAN)         str += "BOOLEAN "         + GetValue();
        else if (TagNum == UNIVERSAL_INTEGER)         str += "INTEGER "         + GetValue();
        else if (TagNum == UNIVERSAL_BITSTRING)       str += "BITSTRING "       + GetValue();
        else if (TagNum == UNIVERSAL_OCTETSTRING)     str += "OCTETSTRING "     + GetValue();
        else if (TagNum == UNIVERSAL_NULL)            str += "NULL";
        else if (TagNum == UNIVERSAL_OID)             str += "OID "             + GetValue();
        else if (TagNum == UNIVERSAL_OBJ_DESCRIPTOR)  str += "OID_Desc "        + GetValue();
        else if (TagNum == UNIVERSAL_EXTERNAL)        str += "EXTERNAL "        + GetValue();
        else if (TagNum == UNIVERSAL_REAL)            str += "REAL "            + GetValue();
        else if (TagNum == UNIVERSAL_ENUMERATED)      str += "ENUMERATED "      + GetValue();
        else if (TagNum == UNIVERSAL_UTF8_STR)        str += "UTF8String "      + GetValue();
        else if (TagNum == UNIVERSAL_RELATIVE_OID)    str += "RelativeOID "     + GetValue();
        else if (TagNum == UNIVERSAL_SEQ_SEQOF)       str += "SEQUENCE "        + GetValue();
        else if (TagNum == UNIVERSAL_SET_SETOF)       str += "SET "             + GetValue();
        else if (TagNum == UNIVERSAL_NUMSTRING)       str += "NumString "       + GetValue();
        else if (TagNum == UNIVERSAL_PRINTABLESTRING) str += "PrintableString " + GetValue();
        else if (TagNum == UNIVERSAL_T61STRING)       str += "T61String "       + GetValue();
        else if (TagNum == UNIVERSAL_VIDEOTEXSTRING)  str += "VideotexString "  + GetValue();
        else if (TagNum == UNIVERSAL_IA5STRING)       str += "IA5String "       + GetValue();
        else if (TagNum == UNIVERSAL_UTCTIME)         str += "UTCTime "         + GetValue();
        else if (TagNum == UNIVERSAL_GENTIME)         str += "GeneralizedTime " + GetValue();
        else if (TagNum == UNIVERSAL_GRAPHIC_STR)     str += "GraphicString "   + GetValue();
        else if (TagNum == UNIVERSAL_ISO646STR)       str += "ISO646String "    + GetValue();
        else if (TagNum == UNIVERSAL_GENERAL_STR)     str += "GeneralString "   + GetValue();
        else if (TagNum == UNIVERSAL_STRING)          str += "UniversalString " + GetValue();
        else if (TagNum == UNIVERSAL_BMPSTRING)       str += "BMPString "       + GetValue();
      }
      if (ChildCount > 0) {
        str += " (" + ChildCount + " elem)";
      }
      if (str.Length > 0) return str;
      else
        return  base.ToString();
    }

    public object GetValue() {
      
      if (ChildCount == 0) {
        byte[] d = Value;
        if (TagNum == UNIVERSAL_INTEGER) {
          ulong val = 0;
          for (int i = 0; i < d.Length; i++) {
            val = (val << 8) | d[i];
          }
          return val.ToString();
        } else if (TagNum == UNIVERSAL_BITSTRING) {
          int paddingCount = d[0];
          int bitCount = (d.Length - 1)*8 - paddingCount;
          return String.Format("({0} bits){1}", bitCount, Utils.HexEncode(d, 1, d.Length - 1));
        }
        else if (TagNum == UNIVERSAL_OCTETSTRING)  return GetString(d);
        else if (TagNum == UNIVERSAL_OID) {
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
        else if (TagNum == UNIVERSAL_UTF8_STR)        return Utils.GetUtf8String(d);
        else if (TagNum == UNIVERSAL_PRINTABLESTRING) return GetString(d);
        else if (TagNum == UNIVERSAL_T61STRING)       return GetString(d);
        else if (TagNum == UNIVERSAL_IA5STRING)       return GetString(d);
        else if (TagNum == UNIVERSAL_UTCTIME)         return ParseUTCTime(d);
        else if (TagNum == UNIVERSAL_GENTIME)         return ParseGeneralizedTime(d);
        else if (TagNum == UNIVERSAL_BMPSTRING)       return GetBMPString(d);
      }
      // For a constructed type, its value is empty.
      return "";
    }

    public void ParseChild() {
      bool   hasChild = IsConstructed;
      byte[] data     = m_Data;
      int    start    = m_ContentStart;
      int    end      = m_ContentEnd;

      if (!hasChild && m_Tag == UNIVERSAL_OCTETSTRING) {
        hasChild = IsValidASN(data, start, end);
      }
      if (!hasChild && m_Tag == UNIVERSAL_BITSTRING) {
        start++; // Skip padding length byte
        hasChild = IsValidASN(data, start, end);
      }
      if (!hasChild) return;
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

      m_Chidren = new List<ASNNode>();
      m_Chidren.Add(new ASNNode((byte)retType, retElementStart, retElementEnd, retContentStart, retContentEnd, data));
      start = retElementEnd;

      while (start < end) {
        if (!MeasureElement(data, start, end,
          ref retType,
          ref retContentStart,
          ref retContentEnd,
          ref retElementStart,
          ref retElementEnd)) {
          throw new Exception("Failed to parse child.(next)");
        };

        m_Chidren.Add(new ASNNode((byte)retType, retElementStart, retElementEnd, retContentStart, retContentEnd, data));
        start = retElementEnd;
      }
      if (start != end) throw new Exception("Invalid ASN data.");

      for (int i = 0; m_Chidren != null && i < m_Chidren.Count; i++) {
        this[i].ParseChild();
      }
    }

    //=====================================
    // Static members
    //=====================================

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
    public static List<int> GetDisplayBytes(ASNNode root) {
      int start = 0;
      Stack<ASNNode> n = new Stack<ASNNode>();
      List<int> ret = new List<int>();
      n.Push(root);
      while (n.Count > 0) {
        ASNNode an = n.Pop();
        if (an.ChildCount > 0) {
          for (int i = an.ChildCount - 1; i >= 0; i--) n.Push(an[i]);
        } else {
          if (an.End - an.Start > 8192) {
            int skipStart = (an.Start + 16 * 4) / 16 * 16;
            int skipEnd = (an.End - 16 * 4) / 16 * 16;
            ret.Add(start);
            ret.Add(skipStart - start); // length
            start = skipEnd;
          }
        }
      }
      ret.Add(start);
      ret.Add(root.End - start);
      return ret;
    }

    private static String GetString(byte[] d) {
      if (Utils.IsPrintable(d)) return Utils.GetUtf8String(d);
      return "(" + d.Length + " bytes)" + Utils.HexEncode(d, 0, d.Length);
    }
    private static String GetBMPString(byte[] d) {
      if (d.Length % 2 == 0) return System.Text.Encoding.GetEncoding("UTF-16BE").GetString(d);
      return "(" + d.Length + " bytes)" + Utils.HexEncode(d, 0, d.Length);
    }
    private static String GetOIDName(String oid) {
      return schema.SchemaFile.GetOIDName(oid);
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
      return retElementStart == start && retElementEnd == end && retType != 0;
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
