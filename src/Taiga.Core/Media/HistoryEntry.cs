namespace Taiga.Core.Media;

/// <summary>
/// Entrada del historial de vistos. Port de <c>media/library/history.cpp</c>.
/// </summary>
public sealed class HistoryEntry
{
    public int AnimeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Episode { get; set; }
    public DateTimeOffset Time { get; set; }
    public string Service { get; set; } = string.Empty;
}
