namespace Taiga.Core.Platform;

/// <summary>
/// Implementación multiplataforma de <see cref="IPathProvider"/> sin dependencias Win32.
/// </summary>
public sealed class PathProvider : IPathProvider
{
    private readonly string _baseDirectory;
    private readonly string? _portableMarker;

    public PathProvider(string? baseDirectory = null, string? portableMarker = null)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
        _portableMarker = portableMarker;
    }

    public bool IsPortableMode()
    {
        var marker = _portableMarker ?? Path.Combine(_baseDirectory, ".portable");
        return File.Exists(marker) || Directory.Exists(Path.Combine(_baseDirectory, "portable"));
    }

    public string GetAppDataDirectory()
    {
        if (IsPortableMode())
        {
            return Path.Combine(_baseDirectory, "data");
        }

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(string.IsNullOrEmpty(appData) ? _baseDirectory : appData, "Taiga");
        }

        var xdgData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdgData))
        {
            return Path.Combine(xdgData, "taiga");
        }

        var home = Environment.GetEnvironmentVariable("HOME") ?? _baseDirectory;
        return Path.Combine(home, ".local", "share", "taiga");
    }

    public string GetConfigDirectory()
    {
        if (IsPortableMode())
        {
            return Path.Combine(_baseDirectory, "data");
        }

        if (OperatingSystem.IsWindows())
        {
            return GetAppDataDirectory();
        }

        var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrWhiteSpace(xdgConfig))
        {
            return Path.Combine(xdgConfig, "taiga");
        }

        var home = Environment.GetEnvironmentVariable("HOME") ?? _baseDirectory;
        return Path.Combine(home, ".config", "taiga");
    }
}
