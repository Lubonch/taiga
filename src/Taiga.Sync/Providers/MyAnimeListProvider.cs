using System.Net.Http.Headers;
using Taiga.Core.Media;

namespace Taiga.Sync.Providers;

/// <summary>
/// Proveedor MyAnimeList (API REST v2 + OAuth2).
/// Port de <c>src/sync/myanimelist*.cpp</c>.
/// </summary>
public sealed class MyAnimeListProvider(HttpClient http, ProviderCredentials credentials) : ISyncProvider
{
    public string Name => "MyAnimeList";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(credentials.MyAnimeListToken);

    public async Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default)
    {
        var status = item.Status switch
        {
            MyStatus.Watching => "watching",
            MyStatus.Completed => "completed",
            MyStatus.OnHold => "on_hold",
            MyStatus.Dropped => "dropped",
            _ => "plan_to_watch",
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Put, $"https://api.myanimelist.net/v2/anime/{item.AnimeId}/my_list_status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.MyAnimeListToken);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["num_watched_episodes"] = item.Episode.ToString(),
            ["status"] = status,
            ["score"] = item.Score.ToString(),
        });

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}
