﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace ASN1Viewer {
  public static class Updater {
    public static DateTime[]  GetVersions() {
      try { 
        byte[] f = ReadFile("version.txt");
        if (f == null) return null;
        int start = 0; 
        // UTF8 BOM 0xEF,0xBB,0xBF
        if (f[0] == 0xEF && f[1] == 0xBB && f[2] == 0xBF) start += 3;
        string d = System.Text.Encoding.ASCII.GetString(f, start, f.Length - start);
        string[] lines = d.Split('\n');
        if (lines.Length < 2) goto failed;
        string[] ret = new string[2];
        for (int i = 0; i < lines.Length; i++) {
          if (lines[i].Contains("ASN1Viewer.exe")) ret[0] = lines[i].Replace("ASN1Viewer.exe","").Trim('\r', '\t', ' ');
          if (lines[i].Contains("files")) ret[1] = lines[i].Replace("files", "").Trim('\r', '\t', ' ');
          if (ret[0] != null && ret[1] != null) break;
        }
        if (ret[0] != null && ret[1] != null) {
          DateTime[] result = new DateTime[2];
          result[0] = DateTime.ParseExact(ret[0], "yyyyMMddHHmmss", null);
          result[1] = DateTime.ParseExact(ret[1], "yyyyMMddHHmmss", null);
          return result;
        }
  failed:
        return null;
      } catch (Exception ex) {
        return null;
      }
    }

    public static bool UpdateASN1Viewer() {
      try {
        byte[] f = ReadFile("ASN1Viewer.exe");
        if (f == null) return false;
        string backupName = "." + Config.AppName + ".tmp";
        if (File.Exists(Config.AppName)) { 
          if (File.Exists(backupName)) File.Delete(backupName);
          File.Move(Config.AppName, backupName);
          File.WriteAllBytes(Config.AppName, f);
          return true;
        } else {
          return false;
        }
      } catch (Exception ex) {
        return false;
      }
    }
    public static bool UpdateFiles() {
      try {
        byte[] f = ReadFile("files.zip");
        if (f == null) return false;
        File.WriteAllBytes("files.zip", f);
        Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile("files.zip");
        if (Directory.Exists("tmp")) Directory.Delete("tmp", true);
        zf.ExtractAll("tmp");
        zf.Dispose();
        File.Delete("files.zip");
        Directory.Delete("files", true);
        Directory.Move("tmp/files", "files");
        Directory.Delete("tmp", true);
        return true;
      } catch (Exception ex) {
        return false;
      }
    }
    public static byte[] ReadFile(string name) {
      try { 
        if (Config.Instance.UpdateLocation.StartsWith("http")) {
          return MyWebClient.Download(Config.Instance.UpdateLocation + name, 30);
        } else {
          return File.ReadAllBytes(Config.Instance.UpdateLocation + name);
        }
      } catch (Exception ex) {
        return null;
      }
    }
  }

  class MyWebClient : System.Net.WebClient {
    private int m_Timeout = 0;
    public int Timeout {
      get { return m_Timeout; }
      set { m_Timeout = value; }
    }

    protected override WebRequest GetWebRequest(Uri uri) {
      WebRequest req = base.GetWebRequest(uri);
      req.Timeout = m_Timeout * 1000;
      ((HttpWebRequest)req).ReadWriteTimeout = m_Timeout * 1000;
      return req;
    }

    public static byte[] Download(string url, int timeout) {
      MyWebClient c = new MyWebClient();
      c.Timeout = timeout;
      MemoryStream sr = new MemoryStream();
      using (Stream s = c.OpenRead(url)) {
        byte[] buf = new byte[4096];
        do {
          int r = s.Read(buf, 0, buf.Length);
          if (r > 0) sr.Write(buf, 0, r);
          else break;
        } while (true);
      }
      return sr.ToArray();
    }
  }
}
