using System.Text.Json;
using Taiga.Core.Text;

namespace Taiga.Core.Media;

/// <summary>
/// Biblioteca del usuario: colección de <see cref="AnimeItem"/> con persistencia JSON.
/// Reemplaza <c>media/library/list.cpp</c> + <c>db\anime.xml</c> / <c>user\*\anime.xml</c>.
/// </summary>
public sealed class AnimeLibrary
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Dictionary<int, AnimeItem> _items = [];

    public int Count => _items.Count;

    public void AddOrUpdate(AnimeItem item) => _items[item.Id] = item;

    public bool Remove(int id) => _items.Remove(id);

    public AnimeItem? FindById(int id) => _items.TryGetValue(id, out var item) ? item : null;

    public IEnumerable<AnimeItem> FindByTitle(string title)
    {
        var normalized = TitleNormalizer.Normalize(title);
        return _items.Values.Where(a =>
            TitleNormalizer.Normalize(a.Title) == normalized ||
            TitleNormalizer.Normalize(a.EnglishTitle) == normalized ||
            a.Synonyms.Any(s => TitleNormalizer.Normalize(s) == normalized) ||
            a.UserSynonyms.Any(s => TitleNormalizer.Normalize(s) == normalized));
    }

    public IEnumerable<AnimeItem> InList() => _items.Values.Where(a => a.IsInList);

    public IEnumerable<AnimeItem> All() => _items.Values;

    public IEnumerable<AnimeItem> CurrentlyWatching() =>
        _items.Values.Where(a => a.IsInList && a.MyStatus == MyStatus.Watching);

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(_items.Values.ToList(), JsonOptions);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, json);
    }

    public static AnimeLibrary Load(string path)
    {
        var library = new AnimeLibrary();
        if (!File.Exists(path))
        {
            return library;
        }

        var items = JsonSerializer.Deserialize<List<AnimeItem>>(File.ReadAllText(path), JsonOptions);
        if (items is not null)
        {
            foreach (var item in items)
            {
                library.AddOrUpdate(item);
            }
        }

        return library;
    }
}
