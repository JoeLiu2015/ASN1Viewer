using System;
using System.Collections.Generic;

namespace ASN1Viewer {
  public static class ASNParser {
    public static byte[] DecodeText(string text) {
      return Utils.ParseHexBytes(text) ?? Utils.ParsePEM(text);
    }

    public static ASNNode ParseText(string text) {
      byte[] data = DecodeText(text);
      if (data == null) throw new FormatException("The input text is invalid ASN.1 data.");
      return Parse(data);
    }

    public static ASNNode Parse(byte[] data) {
      ASNNode a = ASNNode.Parse(data);
      Dictionary<string, schema.SchemaFile> schemas = schema.SchemaFile.Schemas;

      // Match schema
      Dictionary<string, schema.TypeDef> matched = new Dictionary<string, schema.TypeDef>();
      foreach (KeyValuePair<string, schema.SchemaFile> kv in schemas) {
        Dictionary<string, schema.TypeDef> types = kv.Value.Types;
        foreach (KeyValuePair<string, schema.TypeDef> nt in types) {
          if (!matched.ContainsKey(nt.Key) && nt.Value.Match(a, false)) matched.Add(nt.Key, nt.Value);
        }
      }
      List<string> knownTypes = schema.SchemaFile.KNOWN_TYPES;
      for (int i = 0; i < knownTypes.Count; i++) {
        if (matched.ContainsKey(knownTypes[i])) {
          matched[knownTypes[i]].Match(a, true);
          break;
        }
      }
      
      return a;
    }
  }
}
