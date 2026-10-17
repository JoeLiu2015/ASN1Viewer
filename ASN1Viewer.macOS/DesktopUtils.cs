using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ASN1Viewer.macOS;

internal static class DesktopUtils
{
    private static readonly Dictionary<string, Bitmap> icons = new();
    public static string GetFullPath(string path) => Path.Combine(AppContext.BaseDirectory, path);
    public static DateTime GetAssembyBuildTime() => File.GetLastWriteTime(typeof(DesktopUtils).Assembly.Location);
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
