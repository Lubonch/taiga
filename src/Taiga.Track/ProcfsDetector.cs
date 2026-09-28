namespace Taiga.Track;

/// <summary>
/// Detector Linux de respaldo: inspecciona <c>/proc</c> buscando reproductores
/// conocidos y extrae la ruta del fichero reproducido de su línea de comandos.
/// Funciona sin MPRIS ni <c>playerctl</c>, en X11 y Wayland.
/// </summary>
public sealed class ProcfsDetector(IReadOnlyList<string> playerNames) : IPlaybackDetector
{
    public string Name => "/proc fallback";

    public Task<PlaybackState?> DetectAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists("/proc"))
        {
            return Task.FromResult<PlaybackState?>(null);
        }

        foreach (var pid in Directory.EnumerateDirectories("/proc", "[0-9]*"))
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var state = Inspect(pid);
            if (state is not null)
            {
                return Task.FromResult<PlaybackState?>(state);
            }
        }

        return Task.FromResult<PlaybackState?>(null);
    }

    private PlaybackState? Inspect(string pidDir)
    {
        string comm;
        try
        {
            comm = File.ReadAllText(Path.Combine(pidDir, "comm")).Trim();
        }
        catch
        {
            return null;
        }

        var player = playerNames.FirstOrDefault(p =>
            comm.Equals(p, StringComparison.OrdinalIgnoreCase));
        if (player is null)
        {
            return null;
        }

        string cmdline;
        try
        {
            cmdline = File.ReadAllText(Path.Combine(pidDir, "cmdline")).Replace('\0', ' ').Trim();
        }
        catch
        {
            return null;
        }

        var path = cmdline.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .FirstOrDefault(a => !a.StartsWith('-') && FilenameParser.IsVideoFile(a));

        if (path is null)
        {
            return null;
        }

        return new PlaybackState { IsPlaying = true, Player = player, MediaPath = path };
    }
}
