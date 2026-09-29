using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Taiga.Core.Media;
using Taiga.Sync;
using Taiga.Track;

namespace Taiga.Server;

/// <summary>
/// Endpoints de la API local + bus de eventos WS.
/// </summary>
public static class Endpoints
{
    private static readonly Channel<string> Events = Channel.CreateUnbounded<string>();

    public static void Broadcast(string type, object payload) =>
        Events.Writer.TryWrite(JsonSerializer.Serialize(new { type, payload }));

    public static void Map(WebApplication app, AppState? state = null)
    {
        state ??= app.Services.GetRequiredService<AppState>();
        app.MapGet("/api/health", () => Results.Ok(new
        {
            status = "ok",
            version = Taiga.Core.Taiga.AppInfo.Version,
            queue = state.Queue.Count,
        }));

        app.MapGet("/api/library", () => Results.Ok(state.Library.All().OrderBy(a => a.Title)));

        app.MapGet("/api/library/{id:int}", (int id) =>
            state.Library.FindById(id) is { } anime ? Results.Ok(anime) : Results.NotFound());

        app.MapPut("/api/library/{id:int}", (int id, UpdateListEntry body) =>
        {
            var anime = state.Library.FindById(id);
            if (anime is null)
            {
                return Results.NotFound();
            }

            anime.WatchedEpisodes = body.Episode ?? anime.WatchedEpisodes;
            anime.MyStatus = body.Status ?? anime.MyStatus;
            anime.MyScore = body.Score ?? anime.MyScore;
            anime.Notes = body.Notes ?? anime.Notes;
            anime.IsInList = true;
            state.Queue.Enqueue(new SyncQueueItem
            {
                AnimeId = id,
                Episode = anime.WatchedEpisodes,
                Status = anime.MyStatus,
                Score = anime.MyScore,
            });
            state.SaveAll();
            Broadcast("library", new { id });
            return Results.Ok(anime);
        });

        app.MapGet("/api/now-playing", async () =>
        {
            foreach (var detector in state.Detectors)
            {
                var s = await detector.DetectAsync().ConfigureAwait(false);
                if (s?.IsPlaying == true)
                {
                    return Results.Ok(s);
                }
            }

            return Results.Ok(new { isPlaying = false });
        });

        app.MapPost("/api/scan", async () =>
        {
            foreach (var detector in state.Detectors)
            {
                var s = await detector.DetectAsync().ConfigureAwait(false);
                if (s is null || !s.IsPlaying)
                {
                    continue;
                }

                var source = string.IsNullOrEmpty(s.MediaPath) ? s.MediaTitle : s.MediaPath;
                var episode = FilenameParser.Parse(source, fromPath: !string.IsNullOrEmpty(s.MediaPath));
                episode.Player = s.Player;
                var outcome = state.Decider.Decide(episode);
                if (outcome.ShouldUpdate)
                {
                    state.SaveAll();
                }

                Broadcast("now-playing", new { player = s.Player, reason = outcome.Reason });
                return Results.Ok(new
                {
                    playing = true,
                    player = s.Player,
                    updated = outcome.ShouldUpdate,
                    reason = outcome.Reason,
                });
            }

            return Results.Ok(new { playing = false });
        });

        app.MapGet("/api/queue", () => Results.Ok(state.Queue.Pending()));

        app.MapPost("/api/sync", async () =>
        {
            var service = new SyncService(state.Queue, state.ActiveProvider());
            var result = await service.FlushAsync().ConfigureAwait(false);
            Broadcast("queue", new { result.Remaining });
            return Results.Ok(result);
        });

        app.MapGet("/api/settings", () => Results.Ok(new
        {
            state.Settings.Service,
            state.Settings.Username,
            state.Settings.AutoSync,
            state.Settings.PollIntervalSeconds,
            state.Settings.LibraryFolders,
            state.Settings.MediaPlayers,
            state.Settings.Theme,
        }));

        app.MapPut("/api/settings", (UpdateSettings body) =>
        {
            if (body.Service is not null) state.Settings.Service = body.Service;
            if (body.Username is not null) state.Settings.Username = body.Username;
            if (body.PollIntervalSeconds is { } p) state.Settings.PollIntervalSeconds = p;
            if (body.LibraryFolders is not null) state.Settings.LibraryFolders = body.LibraryFolders;
            if (body.MediaPlayers is not null) state.Settings.MediaPlayers = body.MediaPlayers;
            if (body.Theme is not null) state.Settings.Theme = body.Theme;
            if (body.Tokens is not null)
            {
                if (body.Tokens.TryGetValue("myanimelist", out var mal)) state.Credentials.MyAnimeListToken = mal;
                if (body.Tokens.TryGetValue("anilist", out var al)) state.Credentials.AniListToken = al;
                if (body.Tokens.TryGetValue("kitsu", out var k)) state.Credentials.KitsuToken = k;
            }

            state.SaveAll();
            return Results.Ok();
        });

        app.MapGet("/api/history", () => Results.Ok(state.History));

        app.MapGet("/ws/events", async (HttpContext ctx) =>
        {
            if (!ctx.WebSockets.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var ws = await ctx.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await foreach (var msg in Events.Reader.ReadAllAsync(ctx.RequestAborted).ConfigureAwait(false))
            {
                await ws.SendAsync(
                    Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, ctx.RequestAborted)
                    .ConfigureAwait(false);
            }
        });
    }

    public sealed record UpdateListEntry(int? Episode, MyStatus? Status, int? Score, string? Notes);

    public sealed record UpdateSettings(
        string? Service,
        string? Username,
        int? PollIntervalSeconds,
        List<string>? LibraryFolders,
        List<string>? MediaPlayers,
        string? Theme,
        Dictionary<string, string>? Tokens);
}
