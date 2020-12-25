using System;
using System.IO;
using System.Net;

namespace ASN1Viewer {
  class Updater {
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

    public static string Download(string url, int timeout) {
      MyWebClient c = new MyWebClient();
      c.Timeout = timeout;
      string result = null;
      using (Stream s = c.OpenRead(url)) {
        StreamReader sr = new StreamReader(s);
        result = sr.ReadToEnd();
      }
      return result;
    }
  }
}
