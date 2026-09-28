using Taiga.Core.Media;
using Taiga.Sync;

namespace Taiga.Track;

/// <summary>
/// Decide si lo detectado debe actualizar la lista y encola el cambio.
/// Port de <c>track/update_decision.cpp</c> + <c>track/update_session.cpp</c>.
/// </summary>
public sealed class UpdateDecider(AnimeLibrary library, RecognitionEngine recognition, SyncQueue queue)
{
    public UpdateOutcome Decide(EpisodeInfo episode)
    {
        if (episode.Number < 0)
        {
            return UpdateOutcome.Ignored("Sin número de episodio.");
        }

        var id = recognition.Identify(episode, out var score);
        if (id < 0)
        {
            return UpdateOutcome.Ignored($"Sin coincidencia fiable (score {score:F2}).");
        }

        var anime = library.FindById(id);
        if (anime is null || !anime.IsInList)
        {
            return UpdateOutcome.Ignored("El anime no está en la lista del usuario.");
        }

        if (episode.Number <= anime.WatchedEpisodes)
        {
            return UpdateOutcome.Ignored("Episodio ya registrado.");
        }

        if (anime.EpisodeCount > 0 && episode.Number > anime.EpisodeCount)
        {
            return UpdateOutcome.Ignored("Episodio fuera de rango.");
        }

        anime.WatchedEpisodes = episode.Number;
        if (anime.EpisodeCount > 0 && episode.Number >= anime.EpisodeCount)
        {
            anime.MyStatus = MyStatus.Completed;
        }
        else if (anime.MyStatus == MyStatus.PlanToWatch || anime.MyStatus == MyStatus.NotInList)
        {
            anime.MyStatus = MyStatus.Watching;
        }

        queue.Enqueue(new SyncQueueItem
        {
            AnimeId = anime.Id,
            Episode = episode.Number,
            Status = anime.MyStatus,
            Score = anime.MyScore,
        });

        return UpdateOutcome.Updated(anime, episode.Number, score);
    }
}

public sealed class UpdateOutcome
{
    public bool ShouldUpdate { get; init; }
    public AnimeItem? Anime { get; init; }
    public int Episode { get; init; }
    public double Score { get; init; }
    public string Reason { get; init; } = string.Empty;

    public static UpdateOutcome Ignored(string reason) => new() { Reason = reason };

    public static UpdateOutcome Updated(AnimeItem anime, int episode, double score) => new()
    {
        ShouldUpdate = true,
        Anime = anime,
        Episode = episode,
        Score = score,
        Reason = $"{anime.Title} ep. {episode}",
    };
}
