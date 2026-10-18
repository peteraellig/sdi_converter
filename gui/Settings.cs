using System.Text.Json;

namespace SdiConverter;

public sealed class AppSettings
{
    public string? CaptureDevice { get; set; }
    public string SenderName { get; set; } = "SDI Converter";
    public string Output { get; set; } = "OMT";
    public string Quality { get; set; } = "Normal";
    public string? PlayDevice { get; set; }
    public string PlayKind { get; set; } = "OMT";
    public string? OmtSource { get; set; }
    public int BufferFrames { get; set; } = 3;
    public string AudioIn { get; set; } = "Stereo 1&2";
    public string AudioOut { get; set; } = "Stereo 1&2";
    public string Duplicate { get; set; } = "Abort";

    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SDI Converter", "settings.json");

    public static AppSettings Load()
    {
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
