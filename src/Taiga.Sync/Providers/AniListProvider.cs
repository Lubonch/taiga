using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Taiga.Core.Media;

namespace Taiga.Sync.Providers;

/// <summary>
/// Proveedor AniList (GraphQL + OAuth2). Port de <c>src/sync/anilist*.cpp</c>.
/// </summary>
public sealed class AniListProvider(HttpClient http, ProviderCredentials credentials) : ISyncProvider
{
    public string Name => "AniList";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(credentials.AniListToken);

    public async Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default)
    {
        var status = item.Status switch
        {
            MyStatus.Watching => "CURRENT",
            MyStatus.Completed => "COMPLETED",
            MyStatus.OnHold => "PAUSED",
            MyStatus.Dropped => "DROPPED",
            _ => "PLANNING",
        };

        var payload = JsonSerializer.Serialize(new
        {
            query = "mutation ($id: Int, $progress: Int, $status: MediaListStatus, $score: Float) " +
                    "{ SaveMediaListEntry (mediaId: $id, progress: $progress, status: $status, score: $score) { id } }",
            variables = new { id = item.AnimeId, progress = item.Episode, status, score = (float)item.Score },
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://graphql.anilist.co");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AniListToken);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}
