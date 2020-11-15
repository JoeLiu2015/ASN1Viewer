using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASN1Viewer {
  internal class Config {
    public const string INI_FILE = "settings.ini";
    private Dictionary<string, string> m_Data = new Dictionary<string, string>();
    public void Load() {
      string[] lines = File.ReadAllLines(INI_FILE);
      for (int i = 0; i < lines.Length; i++) {
        string line = lines[i].Trim(' ', '\t');
        if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";") || line.StartsWith("[")) continue;
        int pos = line.IndexOf('=');
        if (pos > 0) {
          string key = line.Substring(0, pos).Trim();
          string val = line.Substring(pos + 1).Trim();
          if (val.Contains("%")) {
            //val = val.Replace("%VERSION%", prod.version + "." + prod.build);
          }
          if (val.Contains("\\n")) {
            val = val.Replace("\\n", "\r\n");
          }
          m_Data[key] = val;
        }
      }
    }
  }
}
