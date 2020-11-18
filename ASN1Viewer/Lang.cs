using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.IO;

namespace ASN1Viewer {
  class Lang {
    private static R m_Data = new R();

    public static R T {
      get { return m_Data; }
    }

    public static bool Select(string lang) {
      String langFile = String.Format("./lang/lang-{0}.txt", lang);
      String input = "";
      if (File.Exists(langFile)) {
        input = File.ReadAllText(langFile);
      } else if (lang == "en_US") {
        input = Properties.Resources.lang_en_US;
      } else if (lang == "zh_CN") {
        input = Properties.Resources.lang_zh_CN;
      } else {
        m_Data.Init(Properties.Resources.lang_en_US, input);
        return false;
      }

      m_Data.Init(Properties.Resources.lang_en_US, input);
      return true;
    }
  }


  class R {
    private static Dictionary<string, string> l = new Dictionary<string, string>();
    private static Dictionary<string, string> e = new Dictionary<string, string>();
    public string this[string key] {
      get {
        try {
          return l[key];
        } catch (Exception ex) {
          return e[key];
        }
      }
      set { l[key] = value; }
    }

    public void Init(string en, string local) {
      Parse(en, e);
      Parse(local, l);
    }
    public static void Parse(string input, Dictionary<string, string> dic) {
      dic.Clear();
      string[] lines = input.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < lines.Length; i++) {
        string line = lines[i].Trim(' ', '\t');
        if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";")) continue;
        int pos = line.IndexOf(':');
        if (pos > 0) {
          string key = line.Substring(0, pos).Trim();
          string val = line.Substring(pos + 1).Trim();
          if (val.Contains("%")) {
            //val = val.Replace("%VERSION%", prod.version + "." + prod.build);
          }
          if (val.Contains("\\n")) {
            val = val.Replace("\\n", "\r\n");
          }
          dic[key] = val;
        }
      }
    }
  }
}
