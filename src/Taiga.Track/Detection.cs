namespace Taiga.Track;

/// <summary>
/// Estado de reproducción observado. Port de <c>track::media</c> / <c>track::play</c>.
/// </summary>
public sealed class PlaybackState
{
    public bool IsPlaying { get; set; }
    public string Player { get; set; } = string.Empty;
    public string MediaPath { get; set; } = string.Empty;
    public string MediaTitle { get; set; } = string.Empty;
}

/// <summary>
/// Contrato de detección por plataforma.
/// Windows: títulos Win32. Linux: MPRIS/playerctl + <c>/proc</c>.
/// Port de <c>track/media_player.cpp</c> + <c>track/scanner.cpp</c>.
/// </summary>
public interface IPlaybackDetector
{
    string Name { get; }
    Task<PlaybackState?> DetectAsync(CancellationToken ct = default);
}
