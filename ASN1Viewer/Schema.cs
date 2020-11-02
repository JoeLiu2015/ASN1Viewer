using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASN1Viewer {

  public class SchemaNode {
    private string     m_TypeName   = "";
    private SchemaNode m_FirstChild = null;
    private SchemaNode m_Next       = null;
  }
  public class Schema {
    private Dictionary<string, SchemaNode> m_m_Nodes = new Dictionary<string, SchemaNode>();

    public void Add(string file) {
      string s = File.ReadAllText(file);

    }

  }

  public class SchemaTokenizer {
    private string m_Text = "";

    public SchemaTokenizer(string text) {
      m_Text = text; 
    }


  }
}
