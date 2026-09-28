using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Taiga.Core.Text;

/// <summary>
/// Normalización de títulos para búsqueda y reconocimiento.
/// Port simplificado de <c>track::recognition::Engine::Normalize</c>
/// (src/track/recognition_normalize.cpp): Unicode → minúsculas → sin puntuación.
/// </summary>
public static partial class TitleNormalizer
{
    public static string Normalize(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var folded = string.Concat(title.Normalize(NormalizationForm.FormKD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        var lower = folded.ToLowerInvariant();
        var noPunct = PunctuationRegex().Replace(lower, " ");
        return WhitespaceRegex().Replace(noPunct, " ").Trim();
    }

    [GeneratedRegex(@"[\p{P}\p{S}]+")]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
