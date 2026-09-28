using System.Diagnostics;

namespace Taiga.Track;

/// <summary>
/// Detector Linux vía <c>playerctl</c> (MPRIS D-Bus) cuando está instalado.
/// Cubre MPV, VLC, navegadores y cualquier reproductor MPRIS en X11/Wayland.
/// </summary>
public sealed class PlayerctlDetector : IPlaybackDetector
{
    public string Name => "playerctl/MPRIS";

    public async Task<PlaybackState?> DetectAsync(CancellationToken ct = default)
    {
        var player = await RunAsync("playerctl", "metadata --format '{{playerName}}\t{{status}}\t{{xesam:url}}\t{{title}}'", ct)
            .ConfigureAwait(false);
        if (player is null)
        {
            return null;
        }

        var parts = player.Split('\t');
        if (parts.Length < 2 || !parts[1].Equals("Playing", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var url = parts.Length > 2 ? parts[2] : string.Empty;
        var title = parts.Length > 3 ? parts[3] : string.Empty;
        var path = url.StartsWith("file://", StringComparison.OrdinalIgnoreCase)
            ? Uri.UnescapeDataString(new Uri(url).LocalPath)
            : string.Empty;

        return new PlaybackState
        {
            IsPlaying = true,
            Player = parts[0],
            MediaPath = path,
            MediaTitle = string.IsNullOrEmpty(path) ? title : string.Empty,
        };
    }

    private static async Task<string?> RunAsync(string file, string args, CancellationToken ct)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false);
            await process.WaitForExitAsync(ct).ConfigureAwait(false);
            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}
