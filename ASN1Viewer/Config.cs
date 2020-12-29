using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace ASN1Viewer {
  internal class Config {
    private const string DEFAULT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup useLegacyV2RuntimeActivationPolicy=""true"">
    <supportedRuntime version=""v2.0.50727""/>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.0""/>
  </startup>
  <appSettings>
    <AutoUpdateASN1Modules>{0}</AutoUpdateASN1Modules>
    <Language>{1}</Language>
    <AutoUpdateInterval>{2}</AutoUpdateInterval>
    <LastUpdateDate>{3}</LastUpdateDate>
    <MaxHistoryCount>{4}</MaxHistoryCount>
    <History>{5}</History> 
  </appSettings>
</configuration>
";

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

   
    private List<string> m_HistoryFiles          = new List<string>();
    private bool         m_AutoUpdateASN1Modules = true;
    private int          m_AutoUpdateInterval    = 1; // day
    private DateTime     m_LastUpdteDate         = DateTime.Now;
    private string       m_Lang                  = "zh_CN";
    private int          m_MaxHistoryCount       = 15;

    private Config() { }

    public List<string> History {
      get {  return m_HistoryFiles; }
    }
    public int MaxHistoryCount {
      get {
        return m_MaxHistoryCount;
      }
      set {
        m_MaxHistoryCount = value;
      }
    }
    public string Language {
      get {  return m_Lang; }
      set { m_Lang = value; }
    }
    public void Load() {
      string cfgFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".config";
      XmlDocument xmlDoc = new XmlDocument();
      if (!File.Exists(cfgFile)) return;
      try {
        xmlDoc.Load(cfgFile);
        XmlNode settings = xmlDoc.SelectSingleNode("/configuration/appSettings");
        for (int i = 0; i < settings.ChildNodes.Count; i++) {
          XmlNode node = settings.ChildNodes[i];
          if (node.Name.Equals("AutoUpdateASN1Modules", StringComparison.OrdinalIgnoreCase))   m_AutoUpdateASN1Modules = ParseBool(node.InnerText, true);
          else if (node.Name.Equals("AutoUpdateInterval", StringComparison.OrdinalIgnoreCase)) m_AutoUpdateInterval = ParseInt(node.InnerText, 1);
          else if (node.Name.Equals("LastUpdteDate", StringComparison.OrdinalIgnoreCase))      m_LastUpdteDate = ParseDate(node.InnerText);
          else if (node.Name.Equals("Language", StringComparison.OrdinalIgnoreCase))           m_Lang = ParseLang(node.InnerText);
          else if (node.Name.Equals("MaxHistoryCount", StringComparison.OrdinalIgnoreCase))    m_MaxHistoryCount = ParseInt(node.InnerText, 15);
          else if (node.Name.Equals("History", StringComparison.OrdinalIgnoreCase))            m_HistoryFiles = ParseHistory(node.InnerText); 
        }
      } catch (Exception ex) {
      }
    }
    public void Save() {
      string cfgFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".config";
      string file = string.Format(DEFAULT,
        m_AutoUpdateASN1Modules,
        m_Lang,
        m_AutoUpdateInterval,
        m_LastUpdteDate.ToString("yyyy-MM-dd"),
        m_MaxHistoryCount,
        string.Join(";", m_HistoryFiles.ToArray())
        );
      File.WriteAllText(cfgFile, file);
    }
    private bool ParseBool(string v, bool defValue) {
      try {
        bool val = bool.Parse(v);
        return val;
      } catch (Exception) {
        return defValue;
      }
    }
    private int ParseInt(string v, int defValue) {
      try {
        int val = int.Parse(v);
        return val;
      } catch (Exception) {
        return defValue;
      }
    }

    private DateTime ParseDate(string v) {
      try {
        DateTime val = DateTime.ParseExact(v, "yyyy-MM-dd", null);
        return val;
      } catch (Exception) {
        return DateTime.Now;
      }
    }
    private string ParseLang(string v) {
      if (v.ToLower() == "zh_cn") return "zh_CN";
      return "en_US";
    }

    private List<string> ParseHistory(string v) {
      List<string> ret = new List<string>();
      try { 
        v = v.Trim();
        string[] vals = v.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < vals.Length; i++) {
          if (vals[i].Trim().Length > 0) ret.Add(vals[i].Trim());
        }
      } catch (Exception ex) {

      }
      return ret;
    }
  }
}
