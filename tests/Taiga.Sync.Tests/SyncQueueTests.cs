using Taiga.Core.Media;

namespace Taiga.Sync.Tests;

public sealed class SyncQueueTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public SyncQueueTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private SyncQueue NewQueue() => new(Path.Combine(_dir, "queue.json"));

    [Fact]
    public void Enqueue_MergesSameAnimeKeepingNewestEpisode()
    {
        var queue = NewQueue();
        queue.Enqueue(new SyncQueueItem { AnimeId = 1, Episode = 5 });
        queue.Enqueue(new SyncQueueItem { AnimeId = 1, Episode = 7 });

        Assert.Single(queue.Pending());
        Assert.Equal(7, queue.Pending()[0].Episode);
    }

    [Fact]
    public void Queue_SurvivesReload()
    {
        var queue = NewQueue();
        queue.Enqueue(new SyncQueueItem { AnimeId = 1, Episode = 5 });

        var reloaded = NewQueue();
        reloaded.Load();

        Assert.Single(reloaded.Pending());
    }

    [Fact]
    public async Task Flush_WithoutProvider_KeepsQueue()
    {
        var queue = NewQueue();
        queue.Enqueue(new SyncQueueItem { AnimeId = 1, Episode = 5 });
        var service = new SyncService(queue, new UnconfiguredProvider());

        var result = await service.FlushAsync();

        Assert.Equal(0, result.Sent);
        Assert.Equal(1, result.Remaining);
        Assert.NotNull(result.Note);
    }

    [Fact]
    public async Task Flush_WithWorkingProvider_DrainsQueue()
    {
        var queue = NewQueue();
        queue.Enqueue(new SyncQueueItem { AnimeId = 1, Episode = 5 });
        var service = new SyncService(queue, new AlwaysOkProvider());

        var result = await service.FlushAsync();

        Assert.Equal(1, result.Sent);
        Assert.Equal(0, queue.Count);
    }

    private sealed class UnconfiguredProvider : ISyncProvider
    {
        public string Name => "none";
        public bool IsConfigured => false;
        public Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default) =>
            Task.FromResult(false);
    }

    private sealed class AlwaysOkProvider : ISyncProvider
    {
        public string Name => "fake";
        public bool IsConfigured => true;
        public Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default) =>
            Task.FromResult(true);
    }
}
