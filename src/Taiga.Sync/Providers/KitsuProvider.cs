using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Taiga.Core.Media;

namespace Taiga.Sync.Providers;

/// <summary>
/// Proveedor Kitsu (JSON:API + OAuth2). Port de <c>src/sync/kitsu*.cpp</c>.
/// </summary>
public sealed class KitsuProvider(HttpClient http, ProviderCredentials credentials, Func<SyncQueueItem, string?> entryId) : ISyncProvider
{
    public string Name => "Kitsu";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(credentials.KitsuToken);

    public async Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default)
    {
        var id = entryId(item);
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        var status = item.Status switch
        {
            MyStatus.Watching => "current",
            MyStatus.Completed => "completed",
            MyStatus.OnHold => "on_hold",
            MyStatus.Dropped => "dropped",
            _ => "planned",
        };

        var payload = JsonSerializer.Serialize(new
        {
            data = new
            {
                id,
                type = "libraryEntries",
                attributes = new { progress = item.Episode, status },
            },
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Patch, $"https://kitsu.app/api/edge/library-entries/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.KitsuToken);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/vnd.api+json");

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}
