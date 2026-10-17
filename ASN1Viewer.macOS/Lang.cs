using Avalonia.Platform;

namespace ASN1Viewer.macOS;

public sealed class Lang
{
    private readonly Dictionary<string, string> strings = new();
    public string Language { get; private set; } = "en_US";
    public Lang() => Select("en_US");
    public string this[string key] => strings.TryGetValue(key, out var value) ? value.Replace("&", "") : key;
    public void Select(string language)
    {
        Language = language == "zh_CN" ? "zh_CN" : "en_US";
        strings.Clear();
        using var reader = new StreamReader(AssetLoader.Open(new Uri($"avares://ASN1Viewer.macOS/lang/{Language}.txt")));
        while (reader.ReadLine() is { } line)
        {
            var colon = line.IndexOf(':');
            if (colon > 0) strings[line[..colon].Trim()] = line[(colon + 1)..].Trim().Replace("%VERSION%", "2.0");
        }
    }
}
