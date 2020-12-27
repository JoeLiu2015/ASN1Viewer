using System;
using System.Collections.Generic;

namespace ASN1Viewer.schema {
  public class ImportDef {
    private List<string>   m_DocName     = new List<string>();
    private List<OidDef>   m_DocOid      = new List<OidDef>();
    private List<string[]> m_ImportTypes = new List<string[]>();

    private ImportDef() { }

    public int Count { get { return m_DocName.Count; } }
    public string GetDocName(int idx) { return m_DocName[idx]; }
    public string[] GetImportTypes(int idx) { return m_ImportTypes[idx]; }

    public static ImportDef Parse(Tokenizer tok) {
      try {
        ImportDef import = new ImportDef();
        tok.Skip("IMPORTS");

        do {
          List<string> types = new List<string>();
          do {
            types.Add(tok.Next());
          } while (tok.Next() == ",");
          string name = tok.Next();
          OidDef oid = OidDef.Parse(name, tok);
          import.m_DocName.Add(name);
          import.m_DocOid.Add(oid);
          import.m_ImportTypes.Add(types.ToArray());
        } while (tok.Peek() != ";");
        tok.Next(); // Skip ";"
        return import;
      } catch (Exception ex) {
        throw new Exception("Failed to parse IMPORTS: " + ex.Message);
      }
    }
  }
}
