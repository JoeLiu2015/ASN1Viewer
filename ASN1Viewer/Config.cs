using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASN1Viewer {
  internal class Config {
    private const  string INI_FILE = "settings.ini";
    private static Config m_Ins = null;
    public static Config Instance {
      get {
        if (m_Ins == null) {
          m_Ins = new Config();
          m_Ins.Load();
        }
        return m_Ins;
      }
    }

    private Dictionary<string, string> m_Data           = new Dictionary<string, string>();
    private List<string>               m_HistoryFiles   = null;

    private Config() { }
    public void Load() {
      if (!File.Exists(INI_FILE)) return;
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
          if (val.Contains("\\n")) val = val.Replace("\\n", "\r\n");
          m_Data[key] = val;
        }
      }
    }
    public void Save() {
      if (m_HistoryFiles != null) { 
        m_Data["history"] = String.Join(",", m_HistoryFiles.ToArray());
      }
      StringBuilder sb = new StringBuilder();
      if (m_Data.ContainsKey("max_history")) {
        sb.AppendFormat("max_history={0}\r\n", m_Data["max_history"]);
      }
      if (m_Data.ContainsKey("history")) {
        sb.AppendFormat("history={0}\r\n", m_Data["history"]);
      }
      if (m_Data.ContainsKey("auto_update")) {
        sb.AppendFormat("auto_update={0}\r\n", m_Data["auto_update"]);
      }
      if (m_Data.ContainsKey("auto_update_asn1_modules")) {
        sb.AppendFormat("auto_update_asn1_modules={0}\r\n", m_Data["auto_update_asn1_modules"]);
      }
      File.WriteAllText(INI_FILE, sb.ToString());
    }
    public List<string> History {
      get {
        if (m_HistoryFiles == null) {
          m_HistoryFiles = new List<string>();
          if (!m_Data.ContainsKey("history")) return m_HistoryFiles;
          String s = m_Data["history"];
          string[] his = s.Split(',');
          for (int i = 0; i < his.Length; i++) {
            if (File.Exists(his[i])) m_HistoryFiles.Add(his[i]);
          }
        }
        return m_HistoryFiles;
      }
    }
   
    public int MaxHistory {
      get {
        return getInt("max_history", 15);
      } set {
        m_Data["max_history"] = value + "";
      }
    }

    public bool AuthUpdate {
      get { return getBool("auto_update", true); }
      set { m_Data["auto_update"] = value + "";  }
    }
    public bool AuthUpdateASN1Modules {
      get { return getBool("auto_update_asn1_modules", true); }
      set { m_Data["auto_update_asn1_modules"] = value + ""; }
    }


    private int getInt(string name, int defValue) {
      if (!m_Data.ContainsKey(name)) return defValue;
      try {
        int val = int.Parse(m_Data[name]);
        return val; 
      } catch (Exception) {
        return defValue;
      }
    }
    private bool getBool(string name, bool defValue) {
      if (!m_Data.ContainsKey(name)) return defValue;
      try {
        bool val = bool.Parse(m_Data[name]);
        return val;
      } catch (Exception) {
        return defValue;
      }
    }
  }
}
