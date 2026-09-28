using Taiga.Core.Platform;

namespace Taiga.Core.Settings;

/// <summary>
/// Ajustes de la aplicación, persistidos como JSON versionado.
/// Reemplaza <c>base/settings.cpp</c> + <c>taiga/settings*.cpp</c> (INI/XML) y
/// <c>TAIGA_PORTABLE</c>: si hay modo portable se guarda junto al binario,
/// si no en XDG (<c>~/.config/taiga</c>) o <c>%AppData%\Taiga</c>.
/// </summary>
public sealed class AppSettings
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public string Service { get; set; } = "MyAnimeList";
    public string Username { get; set; } = string.Empty;
    public bool AutoSync { get; set; } = true;
    public bool UpdateOutOfRange { get; set; }
    public int PollIntervalSeconds { get; set; } = 5;
    public List<string> LibraryFolders { get; set; } = [];
    public List<string> MediaPlayers { get; set; } = ["mpv", "vlc", "mplayer", "totem", "celluloid", "haruna"];
    public bool StartMinimized { get; set; }
    public string Theme { get; set; } = "Dark";
}

/// <summary>
/// Carga/guardado versionado de <see cref="AppSettings"/>.
/// </summary>
public sealed class SettingsStore(IPathProvider paths)
{
    public string SettingsPath =>
        Path.Combine(paths.GetConfigDirectory(), "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                if (settings is not null)
                {
                    return Migrate(settings);
                }
            }
        }
        catch
        {
            // Ajustes corruptos: se arranca con valores por defecto.
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        settings.Version = AppSettings.CurrentVersion;
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath) ?? ".");
        var json = System.Text.Json.JsonSerializer.Serialize(
            settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    private static AppSettings Migrate(AppSettings settings)
    {
        if (settings.Version < 1)
        {
            settings.Version = AppSettings.CurrentVersion;
        }

        return settings;
    }
}
