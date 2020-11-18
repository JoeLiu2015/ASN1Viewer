using System;
using System.Collections.Generic;
using System.Text;

namespace ASN1Viewer {
  public class Utils {
    public static Encoding Encoding8Bit = Encoding.GetEncoding("ISO-8859-1");
    public static Encoding EncodingUTF8 = Encoding.UTF8;

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
    public static String HexDump(byte[] val, int offset, int length, string prefix) {
      if (val == null || val.Length == 0) return "";

      StringBuilder sb = new StringBuilder();
      byte[] d = new byte[] {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7', (byte) '8',
        (byte) '9', (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', (byte) 'E', (byte) 'F'
      };
      byte[] defaultBuf = Get8BitBytes("                                                  |                 "); // length: 67 (16*3 + 3 + 8 + 1 + 8)
      byte[] buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      int len = length;
      int len16 = (len + 15) / 16 * 16; // Extend length to multiple of 16, or use as-is if it's already a multiple of 16.

      for (int i = 0; i < len16;) {
        for (int j = 0; j < 16; j++, i++) {
          if (i < len) {
            byte c = val[i + offset];
            // Offsets into buf for byte characters and text character. (Offset 51 is where text characters start.)
            int pos1 = j * 3, pos2 = 51 + j;
            if (j >= 8) { pos1 += 1; pos2 += 1; }
            buf[pos1] = d[(c & 0xFF) >> 4];
            buf[pos1 + 1] = d[c & 0x0F];
            buf[pos2] = (c < 0x20 || c > 0x7E) ? (byte)'.' : c;
          } // Else: Do nothing, the positions already hold space (0x20) characters.
        }

        if (!String.IsNullOrEmpty(prefix)) sb.Append(prefix);
        sb.Append(IntToHex(i - 16));
        sb.Append("|  ");
        sb.Append(Get8BitString(buf));
        if (i < len) sb.Append("\r\n"); // Make sure we don't add an extra LF after the data.
        buf = CopyBytes(defaultBuf, 0, defaultBuf.Length);
      }

      return sb.ToString();
    }
    public static String FixCRLF(String input) {
      StringBuilder sb = new StringBuilder();
      int start = 0;
      for (int i = 0; i < input.Length; i++) {
        if (input[i] == '\n') {
          if (i == 0) { sb.Append('\r'); }
          else if (input[i] != '\r') {
            if (i > start) sb.Append(input, start, i - start);
            sb.Append("\r\n");
            start = i + 1;
          }
        }
      }
      if (sb.Length == 0) return input;
      if (input.Length > start) sb.Append(input, start, input.Length - start);
      return sb.ToString();
    }
  }
}
