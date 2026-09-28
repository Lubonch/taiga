using System.Xml;

namespace Taiga.Core.Infrastructure;

/// <summary>
/// Lector RSS mínimo (titulares de feeds de anime). Port de <c>base/rss.cpp</c>.
/// </summary>
public static class RssReader
{
    public static List<string> ParseTitles(string xml)
    {
        var titles = new List<string>();
        using var reader = XmlReader.Create(new StringReader(xml));
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "title")
            {
                titles.Add(reader.ReadElementContentAsString());
            }
        }

        return titles;
    }

    public static async Task<List<string>> FetchTitlesAsync(HttpClient http, string url, CancellationToken ct = default)
    {
        var xml = await http.GetStringAsync(url, ct).ConfigureAwait(false);
        return ParseTitles(xml);
    }

    public static async Task<List<string>> FetchTitlesAsync(HttpClient http, Uri url, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
