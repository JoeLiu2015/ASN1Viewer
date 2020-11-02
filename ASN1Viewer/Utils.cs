using System;
using System.Collections.Generic;
using System.Text;

namespace ASN1Viewer {
  public class Utils {
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
      return Encoding.UTF8.GetBytes(str);
    }
    public static string GetUtf8String(byte[] bytes) {
      if (bytes != null && bytes.Length > 0 && bytes[bytes.Length - 1] == 0x00) {
        return GetUtf8String(bytes, 0, bytes.Length - 1);
      } else {
        return GetUtf8String(bytes, 0, bytes.Length);
      }
    }
    public static string GetUtf8String(byte[] bytes, int offset, int length) {
      return Encoding.UTF8.GetString(bytes, offset, length);
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
  }
}
