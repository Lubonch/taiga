using System.Net;
using Taiga.Core.Media;
using Taiga.Sync.Providers;

namespace Taiga.Sync.Tests;

public sealed class ProviderTests
{
    private static HttpClient StubClient(HttpStatusCode status, Action<HttpRequestMessage>? inspect = null) =>
        new(new InspectHandler(status, inspect));

    [Fact]
    public async Task Mal_SendsPutToListStatusEndpoint()
    {
        HttpRequestMessage? sent = null;
        var provider = new MyAnimeListProvider(
            StubClient(HttpStatusCode.OK, m => sent = m),
            new ProviderCredentials { MyAnimeListToken = "token" });

        var ok = await provider.UpdateAsync(new SyncQueueItem { AnimeId = 123, Episode = 5 });

        Assert.True(ok);
        Assert.NotNull(sent);
        Assert.Equal(HttpMethod.Put, sent!.Method);
        Assert.Contains("/v2/anime/123/my_list_status", sent.RequestUri!.ToString());
        Assert.Equal("Bearer", sent.Headers.Authorization?.Scheme);
    }

    [Fact]
    public async Task AniList_SendsGraphqlMutation()
    {
        HttpRequestMessage? sent = null;
        var provider = new AniListProvider(
            StubClient(HttpStatusCode.OK, m => sent = m),
            new ProviderCredentials { AniListToken = "token" });

        var ok = await provider.UpdateAsync(new SyncQueueItem { AnimeId = 21, Episode = 2, Status = MyStatus.Completed });

        Assert.True(ok);
        Assert.NotNull(sent);
        Assert.Equal("https://graphql.anilist.co/", sent!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Kitsu_WithoutEntryId_Fails()
    {
        var provider = new KitsuProvider(
            StubClient(HttpStatusCode.OK),
            new ProviderCredentials { KitsuToken = "token" },
            _ => null);

        Assert.False(await provider.UpdateAsync(new SyncQueueItem { AnimeId = 1 }));
    }

    [Fact]
    public async Task Provider_ServerError_ReturnsFalse()
    {
        var provider = new MyAnimeListProvider(
            StubClient(HttpStatusCode.TooManyRequests),
            new ProviderCredentials { MyAnimeListToken = "token" });

        Assert.False(await provider.UpdateAsync(new SyncQueueItem { AnimeId = 1 }));
    }

    private sealed class InspectHandler(HttpStatusCode status, Action<HttpRequestMessage>? inspect) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            inspect?.Invoke(request);
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}
