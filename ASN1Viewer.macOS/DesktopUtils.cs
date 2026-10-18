using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ASN1Viewer.macOS;

internal static class DesktopUtils
{
    public static string ApplicationVersion => System.Reflection.CustomAttributeExtensions
        .GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>(typeof(DesktopUtils).Assembly)?.Version ?? "—";
    private static readonly Dictionary<string, Bitmap> icons = new();
    public static string GetFullPath(string path) => Path.Combine(AppContext.BaseDirectory, path);
    public static DateTime GetAssembyBuildTime() => new DateTime(2000, 1, 1)
        .AddDays(Version.Parse(ApplicationVersion).Revision);
    public static Bitmap GetIcon(string name)
    {
        if (!icons.TryGetValue(name, out var bitmap))
        {
            using var stream = AssetLoader.Open(new Uri("avares://ASN1Viewer.macOS/files/Icons/" + name));
            icons[name] = bitmap = new Bitmap(stream);
        }
        return bitmap;
    }
}
