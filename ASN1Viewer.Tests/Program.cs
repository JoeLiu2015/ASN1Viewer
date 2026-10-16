using System;
using System.Collections.Generic;
using System.IO;
using ASN1Viewer.schema;

namespace ASN1Viewer.Tests {
  internal class Program {
    private delegate void Test();
    private static int passed;
    private static int failed;
    private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

    private static int Main() {
      Run("input encodings", TestInputs);
      Run("structure and absolute offsets", TestStructure);
      Run("embedded ASN.1 and indefinite length", TestEmbedded);
      Run("multi-byte tag", TestMultiByteTag);
      Run("empty BOOLEAN regression", TestEmptyBoolean);
      Run("reject incomplete content", TestInvalid);
      Run("edit same length and no-op", TestEditSameLength);
      Run("edit growing and shrinking content", TestEditResize);
      Run("edit across long-form length boundary", TestEditLongLength);
      Run("schema matching and field annotations", TestSchemas);
      Run("fixture inventory", TestFixtureInventory);
      foreach (string line in File.ReadAllLines(Path.Combine(BasePath, "samples.tsv"))) {
        if (line.Length == 0 || line.StartsWith("#")) continue;
        string[] row = line.Split('|');
        Run(row[0], delegate { TestFixture(row); });
      }
      Console.WriteLine("{0} passed, {1} failed", passed, failed);
      return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Test test) {
      try {
        test();
        passed++;
        Console.WriteLine("PASS " + name);
      } catch (Exception ex) {
        failed++;
        Console.Error.WriteLine("FAIL " + name + ": " + ex);
      }
    }

    private static void Check(bool condition, string message) {
      if (!condition) throw new Exception(message);
    }

    private static void Equal(object expected, object actual, string message) {
      Check(object.Equals(expected, actual), message + ": expected " + expected + ", actual " + actual);
    }

    private static byte[] Hex(string text) { return Utils.ParseHexBytes(text); }

    private static void Bytes(byte[] expected, byte[] actual) {
      Check(Utils.BytesEqual(expected, actual), "Byte sequence differs");
    }

    private static byte[] ReadFixture(string name) {
      string file = Path.Combine(Path.Combine(BasePath, "fixtures"), name);
      string text = File.ReadAllText(file);
      return Utils.ParseHexBytes(text) ?? Utils.ParsePEM(text) ?? File.ReadAllBytes(file);
    }

    private static void TestInputs() {
      byte[] expected = new byte[] { 0x30, 3, 2, 1, 1 };
      Bytes(expected, Hex("30 03\r\n02\t01 01"));
      Bytes(expected, Utils.ParsePEM("MAMCAQE="));
      Bytes(expected, Utils.ParsePEM("-----BEGIN CERTIFICATE-----\r\nMAMCAQE=\r\n-----END CERTIFICATE-----"));
      Bytes(expected, Hex(Utils.HexDump(expected, 0)));
      Check(Hex("0Z") == null && Hex("123") == null, "Invalid HEX must be rejected");
      Check(Utils.ParsePEM("%%invalid%%") == null, "Invalid Base64 must be rejected");
    }

    private static void TestStructure() {
      byte[] data = Hex("FF 30 08 02 01 2A 04 03 61 62 63 EE");
      ASNNode root = ASNNode.Parse(data, 1, 10, false);
      Equal(2, root.ChildCount, "Root children");
      Offsets(root, 1, 11, 3, 11);
      Offsets(root[0], 3, 6, 5, 6);
      Offsets(root[1], 6, 11, 8, 11);
      Equal("42", root[0].GetValue(), "INTEGER value");
      Equal("abc", root[1].GetValue(), "OCTETSTRING value");
      Bytes(Hex("02 01 2A"), root[0].Bytes);
      Bytes(Hex("2A"), root[0].Value);
      Check(root[0].Parent == root && root[1].Root == root, "Parent/root links");
      Equal(3, CheckTree(root), "Node count");
    }

    private static void Offsets(ASNNode node, int start, int end, int contentStart, int contentEnd) {
      Equal(start, node.Start, "Start");
      Equal(end, node.End, "End");
      Equal(contentStart, node.ContentStart, "ContentStart");
      Equal(contentEnd, node.ContentEnd, "ContentEnd");
    }

    private static void TestEmbedded() {
      ASNNode octets = ASNNode.Parse(Hex("04 03 02 01 2A"));
      Equal(1, octets.ChildCount, "Embedded OCTETSTRING");
      Offsets(octets[0], 2, 5, 4, 5);
      ASNNode bits = ASNNode.Parse(Hex("03 04 00 02 01 2A"));
      Equal(1, bits.ChildCount, "Embedded BITSTRING");
      Offsets(bits[0], 3, 6, 5, 6);
      ASNNode indefinite = ASNNode.Parse(Hex("30 80 02 01 2A 00 00"));
      Check(indefinite.IsIndefinite, "Indefinite flag");
      Equal(1, indefinite.ChildCount, "EOC is not a child");
      Offsets(indefinite, 0, 7, 2, 5);
      Equal(2, CheckTree(indefinite), "Indefinite tree");
    }

    private static void TestMultiByteTag() {
      ASNNode node = ASNNode.Parse(Hex("BF 85 3D 03 02 01 01"));
      Equal(701, node.TagNum, "High tag number");
      Equal(3, node.TagLen, "Tag length");
      Equal(ASNNode.NODE_CLASS_CONTEXT, node.TagClass, "Tag class");
      Offsets(node, 0, 7, 4, 7);
      Offsets(node[0], 4, 7, 6, 7);
    }

    private static void TestEmptyBoolean() {
      // Deliberate viewer tolerance for malformed BOOLEANs in captured data.
      ASNNode node = ASNNode.Parse(Hex("30 02 01 00"));
      Equal(" - NO VALUE", node[0].GetValue(), "Empty BOOLEAN display");
      Equal("BOOLEAN  - NO VALUE", node[0].ToString(), "Empty BOOLEAN label");
    }

    private static void TestInvalid() {
      bool rejected = false;
      try { ASNNode.Parse(Hex("30 03 02 01")); }
      catch (Exception) { rejected = true; }
      Check(rejected, "Declared content beyond input must be rejected");
    }

    private static void TestEditSameLength() {
      ASNNode root = ASNNode.Parse(Hex("30 06 02 01 01 04 01 41"));
      Check(!root[0].SetContent(Hex("01")), "Identical edit should return false");
      Check(root[0].SetContent(Hex("02")), "Changed edit should return true");
      Bytes(Hex("30 06 02 01 02 04 01 41"), root.Data);
      Offsets(root[1], 5, 8, 7, 8);
      Equal("2", root[0].GetValue(), "Updated value");
    }

    private static void TestEditResize() {
      ASNNode root = ASNNode.Parse(Hex("30 08 30 03 04 01 41 02 01 01"));
      Check(root[0][0].SetContent(Hex("41 42 43")), "Grow content");
      Bytes(Hex("30 0A 30 05 04 03 41 42 43 02 01 01"), root.Data);
      Offsets(root[0][0], 4, 9, 6, 9);
      Offsets(root[1], 9, 12, 11, 12);
      Equal(4, CheckTree(root), "Edited tree");
      Check(root[0][0].SetContent(new byte[0]), "Shrink content");
      Bytes(Hex("30 07 30 02 04 00 02 01 01"), root.Data);
      Offsets(root[1], 6, 9, 8, 9);
      Equal(4, CheckTree(ASNNode.Parse(root.Data)), "Reparsed edited tree");
    }

    private static void TestEditLongLength() {
      ASNNode root = ASNNode.Parse(Hex("30 06 04 01 41 02 01 01"));
      byte[] content = new byte[128];
      for (int i = 0; i < content.Length; i++) content[i] = 0x41;
      root[0].SetContent(content);
      List<byte> expected = new List<byte>(Hex("30 81 86 04 81 80"));
      expected.AddRange(content);
      expected.AddRange(Hex("02 01 01"));
      Bytes(expected.ToArray(), root.Data);
      Offsets(root[0], 3, 134, 6, 134);
      Offsets(root[1], 134, 137, 136, 137);
      Equal(3, CheckTree(ASNNode.Parse(root.Data)), "Long-length reparse");
      root[0].SetContent(Hex("41"));
      Bytes(Hex("30 06 04 01 41 02 01 01"), root.Data);
    }

    private static void TestSchemas() {
      SchemaFile.ReloadSchemas();
      Check(SchemaFile.Schemas.Count > 0, "Schemas must be deployed beside the test executable");
      Check(SchemaFile.KNOWN_TYPES.Contains("Certificate"), "Known types loaded");
      Equal("(rsaEncryption)", SchemaFile.GetOIDName("1.2.840.113549.1.1.1"), "OID lookup");
      string[,] cases = {
        { "X509.cer", "Certificate", "tbsCertificate" },
        { "PKCS12.pfx", "PFX", "version" },
        { "PKCS1_RSA_PRIVATE.pem", "RSAPrivateKey", "version" },
        { "PKCS1_RSA_PUB.pub", "RSAPublicKey", "modulus" },
        { "PKCS7_Certs.p7b", "ContentInfo", "contentType" }
      };
      for (int i = 0; i < cases.GetLength(0); i++) {
        ASNNode root = ASNNode.Parse(ReadFixture("samples/" + cases[i, 0]));
        TypeDef type = null;
        foreach (SchemaFile schema in SchemaFile.Schemas.Values) {
          if (schema.Types.ContainsKey(cases[i, 1])) { type = schema.Types[cases[i, 1]]; break; }
        }
        Check(type != null, "Missing type " + cases[i, 1]);
        string label = root.ToString();
        Check(type.Match(root, false), "Schema recognition: " + cases[i, 0]);
        Equal(label, root.ToString(), "Recognition must not annotate");
        Check(type.Match(root, true), "Schema annotation: " + cases[i, 0]);
        Check(root.ToString().StartsWith(cases[i, 1] + " "), "Root schema label");
        Check(root[0].ToString().StartsWith(cases[i, 2] + " "), "Child field label");
        Check(!type.Match(ASNNode.Parse(Hex("05 00")), false), "Schema must reject NULL");
      }
    }

    private static void TestFixtureInventory() {
      int rows = 0;
      foreach (string line in File.ReadAllLines(Path.Combine(BasePath, "samples.tsv"))) {
        if (line.Length > 0 && !line.StartsWith("#")) rows++;
      }
      Equal(rows, Directory.GetFiles(Path.Combine(BasePath, "fixtures"), "*", SearchOption.AllDirectories).Length,
        "Every fixture must have a baseline entry");
    }

    private static void TestFixture(string[] row) {
      byte[] data = ReadFixture(row[0]);
      ASNNode root = ASNNode.Parse(data);
      Equal(int.Parse(row[1]), data.Length, "Decoded length");
      Equal(int.Parse(row[2]), root.Tag, "Root tag");
      Equal(int.Parse(row[3]), root.ChildCount, "Root children");
      Equal(int.Parse(row[4]), CheckTree(root), "Total nodes");
      Equal(0, root.Start, "Root start");
      Equal(int.Parse(row[5]), root.End, "Root end");
      // PKCS12.pfx contains zero padding after its ASN.1 element.
      for (int i = root.End; i < data.Length; i++) Equal((byte)0, data[i], "Trailing padding");
    }

    private static int CheckTree(ASNNode node) {
      Check(node.Start >= 0 && node.Start < node.ContentStart && node.ContentStart <= node.ContentEnd
        && node.ContentEnd <= node.End && node.End <= node.Data.Length, "Invalid node offsets");
      Check(node.ToString().Length > 0, "Node display must succeed");
      int count = 1;
      int previousEnd = node.ContentStart;
      for (int i = 0; i < node.ChildCount; i++) {
        ASNNode child = node[i];
        Check(child.Parent == node && child.Root == node.Root, "Invalid parent/root");
        Check(object.ReferenceEquals(child.Data, node.Data), "Children must share document bytes");
        Check(child.Start >= previousEnd && child.End <= node.ContentEnd, "Child outside parent or overlaps sibling");
        previousEnd = child.End;
        count += CheckTree(child);
      }
      return count;
    }
  }
}
