using System;
using System.Collections.Generic;
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
             typeName == "UTCTime" ||
             typeName == "GeneralizedTime" ||
             typeName == "TeletexString" ||
             typeName == "VisibleString" ||

             typeName == "ANY" ||
             typeName == "SEQUENCE" ||
             typeName == "SET" ||
             typeName == "CHOICE";
    }
    public static int GetPrimeTypeTag(string typeName) {
      switch (typeName) {
        case "OCTET STRING":      return ASNNode.UNIVERSAL_OCTETSTRING;
        case "OBJECT IDENTIFIER": return ASNNode.UNIVERSAL_OID;
        case "BIT STRING":        return ASNNode.UNIVERSAL_BITSTRING;
        case "INTEGER":           return ASNNode.UNIVERSAL_INTEGER;
        case "BOOLEAN":           return ASNNode.UNIVERSAL_BOOLEAN;
        case "PrintableString":   return ASNNode.UNIVERSAL_PRINTABLESTRING;
        case "NumericString":     return ASNNode.UNIVERSAL_OCTETSTRING;
        case "IA5String":         return ASNNode.UNIVERSAL_IA5STRING;
        case "UTCTime":           return ASNNode.UNIVERSAL_UTCTIME;
        case "GeneralizedTime":   return ASNNode.UNIVERSAL_GENTIME;
        case "TeletexString":     return ASNNode.UNIVERSAL_T61STRING;
        case "VisibleString":     return ASNNode.UNIVERSAL_ISO646STR;

        case "SEQUENCE":          return ASNNode.UNIVERSAL_SEQ_SEQOF | ASNNode.NODE_CONSTRUCTED_MASK;
        case "SET":               return ASNNode.UNIVERSAL_SET_SETOF | ASNNode.NODE_CONSTRUCTED_MASK;
      }
      throw new Exception("Unknown prime type.");
    }
  }
}
