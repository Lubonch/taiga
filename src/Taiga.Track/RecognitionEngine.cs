using Taiga.Core.Media;
using Taiga.Core.Text;

namespace Taiga.Track;

/// <summary>
/// Identifica el anime de un <see cref="EpisodeInfo"/> contra la biblioteca.
/// Port simplificado del <c>track::recognition::Engine</c>:
/// normalización + índice de títulos + puntuación por trigramas.
/// </summary>
public sealed class RecognitionEngine
{
    private readonly Dictionary<string, List<int>> _titleIndex = new(StringComparer.Ordinal);
    private readonly Dictionary<int, List<string>> _trigrams = [];

    public void Rebuild(AnimeLibrary library)
    {
        _titleIndex.Clear();
        _trigrams.Clear();

        foreach (var anime in library.All())
        {
            Index(anime);
        }
    }

    private void Index(AnimeItem anime)
    {
        foreach (var title in TitlesOf(anime))
        {
            var normalized = TitleNormalizer.Normalize(title);
            if (string.IsNullOrEmpty(normalized))
            {
                continue;
            }

            if (!_titleIndex.TryGetValue(normalized, out var ids))
            {
                ids = [];
                _titleIndex[normalized] = ids;
            }

            if (!ids.Contains(anime.Id))
            {
                ids.Add(anime.Id);
            }
        }

        _trigrams[anime.Id] = Trigrams(TitleNormalizer.Normalize(anime.Title));
    }

    private static IEnumerable<string> TitlesOf(AnimeItem anime)
    {
        yield return anime.Title;
        yield return anime.EnglishTitle;
        yield return anime.JapaneseTitle;
        foreach (var s in anime.Synonyms) yield return s;
        foreach (var s in anime.UserSynonyms) yield return s;
    }

    /// <returns>ID del anime identificado o -1 si no hay coincidencia fiable.</returns>
    public int Identify(EpisodeInfo episode, out double score)
    {
        score = 0;
        var normalized = TitleNormalizer.Normalize(episode.Title);
        if (string.IsNullOrEmpty(normalized))
        {
            return -1;
        }

        if (_titleIndex.TryGetValue(normalized, out var exact) && exact.Count == 1)
        {
            score = 1.0;
            return exact[0];
        }

        var query = Trigrams(normalized);
        var bestId = -1;
        var best = 0.0;

        foreach (var (id, tg) in _trigrams)
        {
            var s = Dice(query, tg);
            if (s > best)
            {
                best = s;
                bestId = id;
            }
        }

        score = best;
        return best >= 0.35 ? bestId : -1;
    }

    private static List<string> Trigrams(string s)
    {
        var padded = $"  {s}  ";
        var result = new List<string>(padded.Length);
        for (var i = 0; i + 3 <= padded.Length; i++)
        {
            result.Add(padded.Substring(i, 3));
        }

        return result;
    }

    private static double Dice(List<string> a, List<string> b)
    {
        if (a.Count == 0 || b.Count == 0)
        {
            return 0;
        }

        var set = new HashSet<string>(b);
        var common = a.Count(t => set.Contains(t));
        return 2.0 * common / (a.Count + b.Count);
    }
}
