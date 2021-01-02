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
    <Language>{0}</Language>
    <AutoUpdate>{1}</AutoUpdate>
    <UpdateLocation>{2}</UpdateLocation>
    <ASN1ViewerMT>{3}</ASN1ViewerMT>
    <ASN1ModulesMT>{4}</ASN1ModulesMT>
    <MaxHistoryCount>{5}</MaxHistoryCount>
    <History>{6}</History>
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

   
    
    private string       m_Lang            = "zh_CN";
    private bool         m_AutoUpdate      = true;
    private string       m_UpdateLocation  = "";
    private DateTime     m_ASN1ViewerMT    = DateTime.MinValue;
    private DateTime     m_ASN1ModulesMT   = DateTime.MinValue;
    private int          m_MaxHistoryCount = 15;
    private List<string> m_HistoryFiles    = new List<string>();

    private Config() { }

    public string Language {
      get { return m_Lang; }
      set { m_Lang = value; }
    }
    public bool AutoUpdate {
      get { return m_AutoUpdate; }
      set { m_AutoUpdate = value; }
    }
    public string UpdateLocation {
      get { return m_UpdateLocation; }
      set { m_UpdateLocation = value; }
    }
    public DateTime ASN1ViewerMT {
      get { return m_ASN1ViewerMT; }
      set { m_ASN1ViewerMT = value; }
    }
    public DateTime ASN1ModulesMT {
      get { return m_ASN1ModulesMT; }
      set { m_ASN1ModulesMT = value; }
    }
    public List<string> History {
      get {  return m_HistoryFiles; }
    }
    public int MaxHistoryCount {
      get { return m_MaxHistoryCount; }
      set { m_MaxHistoryCount = value;}
    }

    public static string AppName {
      get {
        return System.AppDomain.CurrentDomain.FriendlyName;
      }
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
          if (node.Name.Equals("Language", StringComparison.OrdinalIgnoreCase))                m_Lang = ParseLang(node.InnerText);
          else if (node.Name.Equals("AutoUpdate", StringComparison.OrdinalIgnoreCase))         m_AutoUpdate = ParseBool(node.InnerText, true);
          else if (node.Name.Equals("UpdateLocation", StringComparison.OrdinalIgnoreCase))     m_UpdateLocation = node.InnerText;
          else if (node.Name.Equals("ASN1ViewerVer", StringComparison.OrdinalIgnoreCase))      m_ASN1ViewerMT = ParseDateTime(node.InnerText);
          else if (node.Name.Equals("ASN1ModulesVer", StringComparison.OrdinalIgnoreCase))     m_ASN1ModulesMT = ParseDateTime(node.InnerText);
          else if (node.Name.Equals("MaxHistoryCount", StringComparison.OrdinalIgnoreCase))    m_MaxHistoryCount = ParseInt(node.InnerText, 15);
          else if (node.Name.Equals("History", StringComparison.OrdinalIgnoreCase))            m_HistoryFiles = ParseHistory(node.InnerText);
        }
      } catch (Exception ex) {
      }
    }
    public void Save() {
      string cfgFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".config";
      string file = string.Format(DEFAULT,
        m_Lang,
        m_AutoUpdate,
        m_UpdateLocation,
        m_ASN1ViewerMT.ToString("yyyyMMddHHmmss"),
        m_ASN1ModulesMT.ToString("yyyyMMddHHmmss"),
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

    private DateTime ParseDateTime(string v) {
      try {
        DateTime val = DateTime.ParseExact(v, "yyyyMMddHHmmss", null);
        return val;
      } catch (Exception) {
        return DateTime.MinValue;
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
