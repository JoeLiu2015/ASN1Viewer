using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ASN1Viewer {
  internal static class DesktopUtils {
    public static DateTime GetAssembyBuildTime() {
      Assembly assembly = Assembly.GetExecutingAssembly();

      // Retrieve all custom attributes of the assembly
      object[] attributes = assembly.GetCustomAttributes(false);

      // Iterate over the attributes to find the AssemblyFileVersionAttribute
      foreach (object attribute in attributes) {
        if (attribute is AssemblyFileVersionAttribute) {
          AssemblyFileVersionAttribute fileVersionAttribute = (AssemblyFileVersionAttribute)attribute;
          String s = fileVersionAttribute.Version;
          int pos = s.LastIndexOf(".");
          if (pos > 0) {
            s = s.Substring(pos + 1);
            return DateTime.ParseExact(s, "yyyyMMdd", null);
          }
        }
      }

      return DateTime.MinValue;
    }

    public static String GetFullPath(String path) {
      return Path.Combine(new FileInfo(Application.ExecutablePath).DirectoryName, path);
    }
  }
}
