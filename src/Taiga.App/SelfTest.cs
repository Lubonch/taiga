using Taiga.App.Services;
using Taiga.Core.Media;
using Taiga.Core.Platform;
using Taiga.Sync;
using Taiga.Track;

namespace Taiga.App;

/// <summary>
/// Autocomprobación sin UI: verifica persistencia, reconocimiento, decisión,
/// detectores Linux y cola offline. Uso: <c>Taiga.App --self-test</c>.
/// </summary>
internal static class SelfTest
{
    public static async Task<int> RunAsync()
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
        var services = new AppServices(new PathProvider(dir));

        // 1. Ajustes + biblioteca persistentes
        services.Settings.Username = "selftest";
        services.Library.AddOrUpdate(new AnimeItem
        {
            Id = 1,
            Title = "Naruto",
            EpisodeCount = 220,
            IsInList = true,
            MyStatus = MyStatus.Watching,
            WatchedEpisodes = 11,
        });
        services.SaveAll();
        var reloaded = new AppServices(new PathProvider(dir));
        Check(reloaded.Settings.Username == "selftest", "settings-persist");
        Check(reloaded.Library.FindById(1)?.Title == "Naruto", "library-persist");

        // 2. Reconocimiento + decisión + cola
        reloaded.RebuildIndex();
        var episode = FilenameParser.Parse("[HS] Naruto - 12 [720p].mkv");
        var outcome = reloaded.Decider.Decide(episode);
        Check(outcome.ShouldUpdate && reloaded.Queue.Count == 1, "recognize-decide-queue");

        // 3. Sync sin proveedor configurado: queda en cola
        var sync = new SyncService(reloaded.Queue, reloaded.ActiveProvider());
        var result = await sync.FlushAsync().ConfigureAwait(false);
        Check(result.Sent == 0 && result.Remaining == 1 && result.Note is not null, "offline-queue-kept");

        // 4. Detectores Linux no revientan (haya o no reproductor)
        foreach (var detector in reloaded.Detectors)
        {
            try
            {
                var state = await detector.DetectAsync().ConfigureAwait(false);
                Check(true, $"detector-{detector.Name}-ok (playing={state?.IsPlaying ?? false})");
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
