using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public static class Utils {
    public static bool IsPrimeType(string typeName) {
      return typeName == "OCTET STRING" ||
             typeName == "OBJECT IDENTIFIER" ||
             typeName == "BIT STRING" ||
             typeName == "INTEGER" ||
             typeName == "BOOLEAN" ||
             typeName == "PrintableString" ||
             typeName == "NumericString" ||
             typeName == "IA5String" ||
             typeName == "ANY" ||
             typeName == "UTCTime" ||
             typeName == "GeneralizedTime" ||
             typeName == "TeletexString" ||
             typeName == "VisibleString" ||
             typeName == "SEQUENCE" ||
             typeName == "SET" ||
             typeName == "CHOICE";
    }
  }
}
