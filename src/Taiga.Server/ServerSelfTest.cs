using Taiga.Core.Media;
using Taiga.Core.Platform;
using Taiga.Sync;

namespace Taiga.Server;

/// <summary>
/// Autocomprobación del server sin UI ni red: persistencia, reconocimiento,
/// decisión, detectores y cola offline. Uso: <c>Taiga.Server --self-test</c>.
/// </summary>
public static class ServerSelfTest
{
    public static int Run()
    {
        var failures = 0;
        void Check(bool ok, string name)
        {
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}");
            if (!ok)
            {
                failures++;
            }
        }

        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var state = new AppState(new PathProvider(dir));

        state.Settings.Username = "selftest";
        state.Library.AddOrUpdate(new AnimeItem
        {
            Id = 1,
            Title = "Naruto",
            EpisodeCount = 220,
            IsInList = true,
            MyStatus = MyStatus.Watching,
            WatchedEpisodes = 11,
        });
        state.SaveAll();

        var reloaded = new AppState(new PathProvider(dir));
        Check(reloaded.Settings.Username == "selftest", "settings-persist");
        Check(reloaded.Library.FindById(1)?.Title == "Naruto", "library-persist");

        var episode = Track.FilenameParser.Parse("[HS] Naruto - 12 [720p].mkv");
        var outcome = reloaded.Decider.Decide(episode);
        Check(outcome.ShouldUpdate && reloaded.Queue.Count == 1, "recognize-decide-queue");

        var sync = new SyncService(reloaded.Queue, reloaded.ActiveProvider());
        var result = sync.FlushAsync().GetAwaiter().GetResult();
        Check(result.Sent == 0 && result.Remaining == 1, "offline-queue-kept");

        foreach (var detector in reloaded.Detectors)
        {
            try
            {
                detector.DetectAsync().GetAwaiter().GetResult();
                Check(true, $"detector-{detector.Name}-ok");
            }
            catch
            {
                Check(false, $"detector-{detector.Name}-ok");
            }
        }

        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch
        {
        }

        Console.WriteLine(failures == 0 ? "SELF-TEST OK" : $"SELF-TEST FAILED ({failures})");
        return failures == 0 ? 0 : 1;
    }
}
