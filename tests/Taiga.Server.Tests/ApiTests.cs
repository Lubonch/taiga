using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Taiga.Server.Tests;

public sealed class ApiTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Taiga-Token", ApiFactory.Token);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Health_Ok()
    {
        var res = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Library_WithoutToken_Unauthorized()
    {
        using var anon = new WebApplicationFactory<Program>().CreateClient();
        var res = await anon.GetAsync("/api/library");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Library_ListsSeededAnime()
    {
        var items = await _client.GetFromJsonAsync<List<SeededAnime>>("/api/library");
        Assert.NotNull(items);
        Assert.Contains(items, a => a.Title == "Naruto");
    }

    [Fact]
    public async Task UpdateEpisode_QueuesSync()
    {
        var update = new { episode = 12 };
        var res = await _client.PutAsJsonAsync("/api/library/1", update);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var queue = await _client.GetFromJsonAsync<List<QueuedItem>>("/api/queue");
        Assert.NotNull(queue);
        Assert.Contains(queue, q => q.AnimeId == 1 && q.Episode >= 12);
    }

    [Fact]
    public async Task Sync_WithoutProvider_KeepsQueue()
    {
        await _client.PutAsJsonAsync("/api/library/1", new { episode = 13 });
        var res = await _client.PostAsync("/api/sync", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<SyncResultBody>();
        Assert.NotNull(body);
        Assert.True(body.Remaining >= 1);
    }

    [Fact]
    public async Task Scan_WithoutPlayer_ReportsNotPlaying()
    {
        var res = await _client.PostAsync("/api/scan", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<ScanBody>();
        Assert.NotNull(body);
        Assert.False(body.Playing);
    }

    [Fact]
    public async Task NowPlaying_Ok()
    {
        var res = await _client.GetAsync("/api/now-playing");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Settings_RoundTrip()
    {
        var res = await _client.PutAsJsonAsync("/api/settings", new { username = "tester" });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    private sealed record SeededAnime(int Id, string Title);
    private sealed record QueuedItem(int AnimeId, int Episode);
    private sealed record SyncResultBody(int Sent, int Failed, int Remaining, string? Note);
    private sealed record ScanBody(bool Playing);
}
