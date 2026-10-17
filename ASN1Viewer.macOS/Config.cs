using System.Text.Json;

namespace ASN1Viewer.macOS;

public sealed class Config
{
    public string Language { get; set; } = "en_US";
    public bool TopMost { get; set; }
    public List<string> History { get; set; } = new();
    private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ASN1Viewer", "config.json");
    public static Config Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var result = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
                if (result != null)
                {
                    result.Language = result.Language == "zh_CN" ? "zh_CN" : "en_US";
                    result.History = (result.History ?? new()).Where(p => !string.IsNullOrEmpty(p) && Path.IsPathFullyQualified(p)).Distinct().Take(10).ToList();
                    return result;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        return new Config();
    }
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
