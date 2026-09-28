using Taiga.Core.Media;
using Taiga.Core.Platform;
using Taiga.Core.Settings;
using Taiga.Sync;
using Taiga.Sync.Providers;
using Taiga.Track;

namespace Taiga.App.Services;

/// <summary>
/// Raíz de composición: cablea Core/Sync/Track con las rutas del SO.
/// </summary>
public sealed class AppServices
{
    public IPathProvider Paths { get; }
    public SettingsStore SettingsStore { get; }
    public AppSettings Settings { get; private set; }
    public AnimeLibrary Library { get; private set; }
    public SyncQueue Queue { get; }
    public RecognitionEngine Recognition { get; } = new();
    public UpdateDecider Decider { get; }
    public ProviderCredentials Credentials { get; }
    public IReadOnlyList<IPlaybackDetector> Detectors { get; }
    public List<HistoryEntry> History { get; } = [];

    public AppServices(IPathProvider? paths = null)
    {
        Paths = paths ?? new PathProvider();
        SettingsStore = new SettingsStore(Paths);
        Settings = SettingsStore.Load();

        var dataDir = Paths.GetAppDataDirectory();
        Library = AnimeLibrary.Load(Path.Combine(dataDir, "library.json"));

        Queue = new SyncQueue(Path.Combine(dataDir, "queue.json"));
        Queue.Load();

        Decider = new UpdateDecider(Library, Recognition, Queue);
        Recognition.Rebuild(Library);

        Credentials = ProviderCredentials.Load(Path.Combine(Paths.GetConfigDirectory(), "credentials.json"));

        var http = new HttpClient();
        Detectors = OperatingSystem.IsWindows()
            ? [new ProcfsDetector(Settings.MediaPlayers)]
            : (IReadOnlyList<IPlaybackDetector>)[
                new PlayerctlDetector(),
                new ProcfsDetector(Settings.MediaPlayers),
            ];
        Providers = [
            new MyAnimeListProvider(http, Credentials),
            new AniListProvider(http, Credentials),
            new KitsuProvider(http, Credentials, item => Library.FindById(item.AnimeId)?.MyId),
        ];
    }

    public List<ISyncProvider> Providers { get; }

    public ISyncProvider ActiveProvider() =>
        Providers.FirstOrDefault(p => p.Name == Settings.Service && p.IsConfigured)
        ?? Providers.FirstOrDefault(p => p.IsConfigured)
        ?? Providers[0];

    public void SaveAll()
    {
        var dataDir = Paths.GetAppDataDirectory();
        SettingsStore.Save(Settings);
        Library.Save(Path.Combine(dataDir, "library.json"));
        Credentials.Save(Path.Combine(Paths.GetConfigDirectory(), "credentials.json"));
    }

    public void RebuildIndex() => Recognition.Rebuild(Library);
}
