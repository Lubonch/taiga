using System.Text.RegularExpressions;

namespace Taiga.Track;

/// <summary>
/// Episodio detectado en un reproductor o fichero.
/// Port de <c>anime::Episode</c> (src/track/episode.h).
/// </summary>
public sealed class EpisodeInfo
{
    public string Title { get; set; } = string.Empty;
    public int Number { get; set; } = -1;
    public string FilePath { get; set; } = string.Empty;
    public string Player { get; set; } = string.Empty;
    public bool IsStreaming { get; set; }
}

/// <summary>
/// Extrae título y número de episodio de nombres de fichero/títulos de ventana.
/// Cubre los patrones más comunes de <c>recognition.cpp</c>:
/// <c>S02E05</c>, <c>- 05</c>, <c>[Grupo] Titulo - 05 [1080p]</c>, <c>Titulo E05</c>, etc.
/// </summary>
public static partial class FilenameParser
{
    private static readonly string[] VideoExtensions =
        [".mkv", ".mp4", ".avi", ".m4v", ".ts", ".m2ts", ".ogm", ".webm"];

    public static bool IsVideoFile(string path) =>
        VideoExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());

    public static EpisodeInfo Parse(string input, bool fromPath = true)
    {
        var episode = new EpisodeInfo { FilePath = fromPath ? input : string.Empty };
        var name = fromPath
            ? Path.GetFileNameWithoutExtension(input)
            : input;

        // [Grupo] al inicio
        name = ReleaseGroupRegex().Replace(name, string.Empty).Trim();

        // Grupos [..] / (..) y etiquetas de calidad fuera: solo estorban al matching.
        var work = ResolutionRegex().Replace(BracketsRegex().Replace(name, " "), " ").Trim(' ', '_', '.', '-');

        // S02E05 / S2E5
        var m = SeasonEpisodeRegex().Match(work);
        if (m.Success)
        {
            episode.Number = int.Parse(m.Groups["ep"].Value);
            episode.Title = CleanTitle(work[..m.Index]);
            return episode;
        }

        // - 05 / E05 / EP05 / #05 / Ep. 5 al final o en medio
        m = EpisodeMarkerRegex().Match(work);
        if (m.Success)
        {
            episode.Number = int.Parse(m.Groups["ep"].Value);
            episode.Title = CleanTitle(work[..m.Index]);
            return episode;
        }

        // Último número suelto (p. ej. "Naruto 12 [1080p]")
        m = TrailingNumberRegex().Match(work);
        if (m.Success && int.TryParse(m.Groups["ep"].Value, out var n) && n < 2000)
        {
            episode.Number = n;
            episode.Title = CleanTitle(work[..m.Index]);
            return episode;
        }

        episode.Title = CleanTitle(work);
        return episode;
    }

    private static string CleanTitle(string raw)
    {
        var s = BracketsRegex().Replace(raw, " ");
        s = ResolutionRegex().Replace(s, " ");
        s = UnderscoresRegex().Replace(s, " ");
        return WhitespaceRegex().Replace(s, " ").Trim(' ', '-', '_', '.');
    }

    [GeneratedRegex(@"^\s*\[[^\]]*\]\s*")]
    private static partial Regex ReleaseGroupRegex();
    [GeneratedRegex(@"[Ss](?<s>\d{1,2})[Ee](?<ep>\d{1,4})")]
    private static partial Regex SeasonEpisodeRegex();
    [GeneratedRegex(@"(?:^|[\s_\.\-])(?:[Ee](?:p(?:isode)?)?\.?\s*#?\s*|#)(?<ep>\d{1,4})(?:\s*(?:v\d+))?(?:[\s_\.\-\[\(]|$)")]
    private static partial Regex EpisodeMarkerRegex();
    [GeneratedRegex(@"[\s_\.\-]+(?<ep>\d{1,4})(?:\s*v\d+)?\s*(\[[^\]]*\]|\([^\)]*\))?\s*$")]
    private static partial Regex TrailingNumberRegex();
    [GeneratedRegex(@"\[[^\]]*\]|\([^\)]*\)")]
    private static partial Regex BracketsRegex();
    [GeneratedRegex(@"\b\d{3,4}p\b|\b[xh]\.?264\b|\b[xh]\.?265\b|\bHEVC\b|\bBlu-?ray\b|\bWEB-?DL\b", RegexOptions.IgnoreCase)]
    private static partial Regex ResolutionRegex();
    [GeneratedRegex(@"[_\.]+")]
    private static partial Regex UnderscoresRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
