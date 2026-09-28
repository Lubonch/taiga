using Taiga.Core.Media;
using Taiga.Sync;

namespace Taiga.Track.Tests;

public sealed class RecognitionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public RecognitionTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private static AnimeLibrary SampleLibrary()
    {
        var library = new AnimeLibrary();
        library.AddOrUpdate(new AnimeItem
        {
            Id = 1,
            Title = "Naruto",
            EnglishTitle = "Naruto",
            EpisodeCount = 220,
            IsInList = true,
            MyStatus = MyStatus.Watching,
            WatchedEpisodes = 11,
        });
        library.AddOrUpdate(new AnimeItem
        {
            Id = 2,
            Title = "Shingeki no Kyojin",
            EnglishTitle = "Attack on Titan",
            Synonyms = ["Attack on Titan"],
            EpisodeCount = 25,
            IsInList = true,
            MyStatus = MyStatus.Watching,
            WatchedEpisodes = 4,
        });
        return library;
    }

    [Fact]
    public void Identify_FindsAnimeByNormalizedTitle()
    {
        var engine = new RecognitionEngine();
        engine.Rebuild(SampleLibrary());

        var id = engine.Identify(FilenameParser.Parse("[HS] Naruto - 12 [720p].mkv"), out var score);

        Assert.Equal(1, id);
        Assert.True(score > 0.3);
    }

    [Fact]
    public void Decide_UpdatesLibraryAndQueuesSync()
    {
        var library = SampleLibrary();
        var engine = new RecognitionEngine();
        engine.Rebuild(library);
        var queue = new SyncQueue(Path.Combine(_dir, "queue.json"));
        queue.Load();
        var decider = new UpdateDecider(library, engine, queue);

        var outcome = decider.Decide(FilenameParser.Parse("[HS] Naruto - 12 [720p].mkv"));

        Assert.True(outcome.ShouldUpdate);
        Assert.Equal(12, library.FindById(1)?.WatchedEpisodes);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void Decide_IgnoresAlreadyWatchedEpisode()
    {
        var library = SampleLibrary();
        var engine = new RecognitionEngine();
        engine.Rebuild(library);
        var queue = new SyncQueue(Path.Combine(_dir, "queue.json"));
        var decider = new UpdateDecider(library, engine, queue);

        var outcome = decider.Decide(FilenameParser.Parse("[HS] Naruto - 05 [720p].mkv"));

        Assert.False(outcome.ShouldUpdate);
        Assert.Equal(0, queue.Count);
    }
}
