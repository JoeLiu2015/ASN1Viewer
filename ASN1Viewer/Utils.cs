using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace ASN1Viewer {
  public class Utils {
    public static Encoding Encoding8Bit = Encoding.GetEncoding("ISO-8859-1");
    public static Encoding EncodingUTF8 = Encoding.UTF8;
    private static byte[] DIGITS = null;
    private static bool[] B64MAP = null;

    public static readonly byte[] BYTE0 = new byte[0]; 
    public static byte[] CopyBytes(byte[] value, int offset, int length) {
      if (value == null) return BYTE0;
      if (offset < 0) {
        offset = 0;
      } else if (offset > value.Length) {
        offset = value.Length;
      }

      if (length < 0) length = 0;

      if (offset + length > value.Length) {
        length = value.Length - offset;
      }

      return CopyBytesNoChecks(value, offset, length);
    }
    public static byte[] CopyBytesNoChecks(byte[] value, int offset, int length) {
      byte[] result = new byte[length];
      Array.Copy(value, offset, result, 0, length);
      return result;
    }
    public static byte[] GetUtf8Bytes(string str) {
      return EncodingUTF8.GetBytes(str);
    }
    public static string GetUtf8String(byte[] bytes) {
      if (bytes != null && bytes.Length > 0 && bytes[bytes.Length - 1] == 0x00) {
        return GetUtf8String(bytes, 0, bytes.Length - 1);
      } else {
        return GetUtf8String(bytes, 0, bytes.Length);
      }
    }
    public static string GetUtf8String(byte[] bytes, int offset, int length) {
      return EncodingUTF8.GetString(bytes, offset, length);
    }
    public static byte[] Get8BitBytes(string str) {
      return Encoding8Bit.GetBytes(str);
    }
    public static String Get8BitString(byte[] data) {
      return Encoding8Bit.GetString(data);
    }
    public static string GetHexString(byte[] bytes, int offset, int len) {
      StringBuilder sb = new StringBuilder();
      String hex = "0123456789ABCDEF";
      for (int i = 0; i < len; i++) {
        sb.Append(hex[bytes[i + offset] >> 4]);
        sb.Append(hex[bytes[i + offset] & 0x0F]);
      }

      return sb.ToString();
    }
    public static bool BytesEqual(byte[] b1, byte[] b2) {
      if (b1 == null || b2 == null || b1.Length != b2.Length) return false;
      for (int i = 0; i < b1.Length; i++) {
        if (b1[i] != b2[i]) return false;
      }
      return true;
    }
    public static bool IsUTF8String(byte[] data) {
      bool isASCII = true;
      for (int i = 0; i < data.Length; i++) {
        if ((data[i] & 0xFF) >= 0x80) {
          isASCII = false;
          break;
        }
      }
      if (isASCII) return true;
      byte[] d = GetUtf8Bytes(GetUtf8String(data, 0, data.Length));
      return BytesEqual(d, data);
    }
    public static bool IsPrintable(byte[] d) {
      for (int i = 0; i < d.Length; i++) {
        if (d[i] == '\r' || d[i] == '\n' || d[i] == '\t') continue;
        if (d[i] < 32 || d[i] > 126) return false;
      }
      return true;
    }
    public static String HexEncode(byte[] data, int offset, int len) {
      String digits = "0123456789ABCDEF";
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < len; i++, offset++) {
        byte b = data[offset];
        sb.Append(digits[(b >> 4) & 0xF]);
        sb.Append(digits[b & 0xF]);
      }
      return sb.ToString();
    }

    public static byte[] HexDecode(String s) {
      if (s.Length == 0) return new byte[0];
      MemoryStream ms = new MemoryStream();
      for (int i = 0; i < s.Length; i++) {
        char c = s[i];
        if (c == ' ' || c == '\t' || c == '\r' || c == '\n') continue;
        if (('0' <= c && c <= '9') || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F')) {
          if (i + 1 < s.Length && (('0' <= s[i+1] && s[i+1] <= '9') || ('a' <= s[i+1] && s[i+1] <= 'f') || ('A' <= s[i+1] && s[i+1] <= 'F'))) {
            ms.WriteByte(HexToByte(c, s[i+1]));
            i++;
          } else {
            return null;
          }
        } else {
          return null;
        }
      }
      return ms.ToArray();
    }
    public static void IntToBytes(long uinteger, byte[] bytes, int offset, int length) {
      for (int i = offset + length - 1; i >= offset; i--, uinteger >>= 8) {
        bytes[i] = (byte)(uinteger & 0x0ff);
      }
    }
    public static byte[] LongToBytes(long uinteger, int length) {
      byte[] buff = new byte[length];
      IntToBytes(uinteger, buff, 0, length);
      return buff;
    }
    public static String IntToHex(int val) {
      StringBuilder sb = new StringBuilder();
      byte[] d = LongToBytes(val, 4);
      for (int i = 0; i < d.Length; i++) sb.Append(ByteToHex(d[i]));
      return sb.ToString();
    }
    public static String ByteToHex(byte c) {
      char[] d = new char[] { '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      return "" + d[(c & 0xFF) >> 4] + d[c & 0x0F];
    }

    public static byte HexToByte(char c, char s) {
      int ret = 0;
      if (c >= '0' && c <= '9') {
        ret = c - '0';
      } else if (c >= 'a' && c <= 'f') {
        ret = c - 'a' + 10;
      } else if (c >= 'A' && c <= 'F') {
        ret = c - 'A' + 10;
      } else {
        throw new Exception("out of range");
      }

      ret = ret * 16;
      if (s >= '0' && s <= '9') {
        ret += s - '0';
      } else if (s >= 'a' && s <= 'f') {
        ret += s - 'a' + 10;
      } else if (s >= 'A' && s <= 'F') {
        ret += s - 'A' + 10;
      } else {
        throw new Exception("out of range");
      }
      return (byte)ret;
    }

    public static byte[] Reverse(byte[] data) {
      for (int i = 0; i < data.Length / 2; i++) {
        byte b = data[i];
        data[i] = data[data.Length - 1 - i];
        data[data.Length - 1 - i] = b;
      }

      return data;
    }
    public static String HexDump(byte[] val, int position) {
      if (val == null || val.Length == 0) return "";

      StringBuilder sb = new StringBuilder();
      byte[] d = new byte[] {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7', (byte) '8',
        (byte) '9', (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', (byte) 'E', (byte) 'F'
      };
      byte[] defaultBuf = Get8BitBytes("                                                  |                 "); // length: 67 (16*3 + 3 + 8 + 1 + 8)
      byte[] buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      int len = val.Length;
      int len16 = (len + 15) / 16 * 16; // Extend length to multiple of 16, or use as-is if it's already a multiple of 16.

      for (int i = 0; i < len16;) {
        for (int j = 0; j < 16; j++, i++) {
          if (i < len) {
            byte c = val[i];
            // Offsets into buf for byte characters and text character. (Offset 51 is where text characters start.)
            int pos1 = j * 3, pos2 = 16*3 + 3 + j;
            if (j >= 8) { pos1 += 1; pos2 += 1; }
            buf[pos1]     = d[(c & 0xFF) >> 4];
            buf[pos1 + 1] = d[c & 0x0F];
            buf[pos2]     = (c < 0x20 || c > 0x7E) ? (byte)'.' : c;
          } // Else: Do nothing, the positions already hold space (0x20) characters.
        }

        sb.Append(IntToHex(i - 16 + position));
        sb.Append("|  ");
        sb.Append(Get8BitString(buf));
        if (i < len) sb.Append("\r\n"); // Make sure we don't add an extra CRLF after the data.
        buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      }

      return sb.ToString();
    }
    public static String FixCRLF(String input) {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < input.Length; i++) {
        if (input[i] == '\n' && (i == 0 || input[i - 1] != '\r')) sb.Append('\r');
        sb.Append(input[i]);
      }
      return sb.ToString();
    }
    public static byte[] ParseHexBytes(string text) {
      if (DIGITS == null) {
        DIGITS = new byte[128];
        for (int i = (int)'0'; i <= (int)'9'; i++) DIGITS[i] = (byte)(i - (int)'0');
        for (int i = (int)'a'; i <= (int)'f'; i++) DIGITS[i] = (byte)(i - (int)'a' + 10);
        for (int i = (int)'A'; i <= (int)'F'; i++) DIGITS[i] = (byte)(i - (int)'A' + 10);
      }
      MemoryStream ms = new MemoryStream();
      int pos = 0;
      while (pos < text.Length) {
        char ch = text[pos];
        if (ch == '\r' || ch == '\n' || ch == ' ' || ch == '\t') {
          pos++;
          continue;
        }
        if (pos + 1 < text.Length && IsHexChar(text[pos]) && IsHexChar(text[pos + 1])) {
          ms.WriteByte((byte)((DIGITS[text[pos]] << 4) | DIGITS[text[pos + 1]]));
          pos += 2;
        } else {
          return null;
        }
      }

      if (ms.Length == 0) return null;
      return ms.ToArray();
    }
    public static bool IsHexChar(char ch) {
      return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
    }
    public static byte[] ParsePEM(string text) {
      if (B64MAP == null) {
        B64MAP = new bool[256];
        for (int i = (int)'0'; i <= (int)'9'; i++) B64MAP[i] = true;
        for (int i = (int)'a'; i <= (int)'z'; i++) B64MAP[i] = true;
        for (int i = (int)'A'; i <= (int)'Z'; i++) B64MAP[i] = true;
        B64MAP['+'] = true;
        B64MAP['/'] = true;
      }

      List<String> lines = new List<string>();
      if (text.Contains("-----BEGIN") && text.Contains("-----END")) {
        text = text.Replace("\r", "");
        // Add LF for one-line certificate
        if (text.Contains("-----BEGIN CERTIFICATE-----") && !text.Contains("-----BEGIN CERTIFICATE-----\n")) {
          text = text.Replace("-----BEGIN CERTIFICATE-----", "-----BEGIN CERTIFICATE-----\n");
        }
        if (text.Contains("-----END CERTIFICATE-----") && !text.Contains("\n-----END CERTIFICATE-----")) {
          text = text.Replace("-----END CERTIFICATE-----", "\n-----END CERTIFICATE-----");
        }
        String[] strlines = text.Split('\n');
       lines.AddRange(strlines);
      } else {
        // pure base64 text without BEGIN/END identifier
        lines.Add("-----BEGIN FACK DATA-----");
        lines.Add(text);
        lines.Add("-----END FACK DATA-----");
      }

      while (lines.Count > 0) {
        int start = -1, end = -1;
        text = null;
        for (int i = 0; i < lines.Count; i++) {
          if (lines[i].Contains("-----BEGIN")) {
            start = i + 1;
          } else if (lines[i].Contains("-----END")) {
            end = i - 1;
          }

          if (start >= 0 && end >= start) {
            text = string.Join("\n", lines.GetRange(start, end - start + 1).ToArray());
            lines.RemoveRange(0, end+ 2);
            break;
          } else if (i == lines.Count - 1) {
            return null; // No BEGIN .... END found.
          }
        }
        if (text == null) { continue;}
DO_PARSE:
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++) {
          char ch = text[i];
          if (ch >= 256) {
            sb.Length = 0;
            break;
          }
          if (ch == '\r' || ch == '\n' || ch == ' ' || ch == '\t') continue;
          if (!B64MAP[ch] && ch != '=') {
            sb.Length = 0;
            break;
          }
          sb.Append(ch);
        }
        if (sb.Length == 0) continue;
        try {
          String s = sb.ToString();
          if (s.Length == 0) return null;
          return Convert.FromBase64String(s);
        } catch (Exception) { }
      }

      return null;
    }

    public static DateTime GetAssembyBuildTime() {
      Assembly assembly = Assembly.GetExecutingAssembly();

      // Retrieve all custom attributes of the assembly
      object[] attributes = assembly.GetCustomAttributes(false);

      // Iterate over the attributes to find the AssemblyFileVersionAttribute
      foreach (object attribute in attributes) {
        if (attribute is AssemblyFileVersionAttribute) {
          AssemblyFileVersionAttribute fileVersionAttribute = (AssemblyFileVersionAttribute)attribute;
          String s = fileVersionAttribute.Version;
          int pos = s.LastIndexOf(".");
          if (pos > 0) {
            s = s.Substring(pos + 1);
            return DateTime.ParseExact(s, "yyyyMMdd", null);
          }
        }
      }

      return DateTime.MinValue;
    }
  }
}
