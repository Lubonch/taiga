namespace Taiga.Core.Media;

/// <summary>
/// Proveedor de sincronización. Port de <c>sync::ServiceId</c> (src/sync/service.h).
/// </summary>
public enum ServiceId
{
    Unknown = 0,
    AniList = 1,
    Kitsu = 2,
    MyAnimeList = 3,
}

/// <summary>
/// Tipo de serie. Port de <c>anime::SeriesType</c> (src/media/anime.h).
/// </summary>
public enum SeriesType
{
    Unknown = 0,
    Tv = 1,
    Ova = 2,
    Movie = 3,
    Special = 4,
    Ona = 5,
    Music = 6,
}

/// <summary>
/// Estado en la lista del usuario. Port de <c>anime::MyStatus</c>.
/// </summary>
public enum MyStatus
{
    NotInList = 0,
    Watching = 1,
    Completed = 2,
    OnHold = 3,
    Dropped = 4,
    PlanToWatch = 5,
}

/// <summary>
/// Ficha de anime: metadatos de la base de datos + datos de lista del usuario + datos locales.
/// Port de <c>anime::Item</c> (src/media/anime_item.h).
/// </summary>
public sealed class AnimeItem
{
    public int Id { get; set; }
    public Dictionary<ServiceId, string> ExternalIds { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public ServiceId Source { get; set; } = ServiceId.Unknown;
    public SeriesType Type { get; set; } = SeriesType.Unknown;
    public int EpisodeCount { get; set; }
    public int EpisodeLength { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EnglishTitle { get; set; } = string.Empty;
    public string JapaneseTitle { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = [];
    public List<string> Genres { get; set; } = [];
    public double Score { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Synopsis { get; set; } = string.Empty;
    public DateTimeOffset LastModified { get; set; }

    // Datos de lista del usuario (MyInformation)
    public bool IsInList { get; set; }
    public string MyId { get; set; } = string.Empty;
    public MyStatus MyStatus { get; set; } = MyStatus.NotInList;
    public int WatchedEpisodes { get; set; }
    public int MyScore { get; set; }
    public bool IsPrivate { get; set; }
    public int RewatchedTimes { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;

    // Datos locales
    public string Folder { get; set; } = string.Empty;
    public List<string> UserSynonyms { get; set; } = [];
    public HashSet<int> AvailableEpisodes { get; set; } = [];

    public bool IsEpisodeAvailable(int episode) => AvailableEpisodes.Contains(episode);

    public int NextEpisodeToWatch => WatchedEpisodes + 1;

    public bool IsNextEpisodeAvailable() => IsEpisodeAvailable(NextEpisodeToWatch);
}
