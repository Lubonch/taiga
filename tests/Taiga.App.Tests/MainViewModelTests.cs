using Taiga.App.Services;
using Taiga.App.ViewModels;
using Taiga.Core.Media;
using Taiga.Core.Platform;

namespace Taiga.App.Tests;

public sealed class MainViewModelTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public MainViewModelTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private MainViewModel NewVm()
    {
        var services = new AppServices(new PathProvider(_dir));
        services.Library.AddOrUpdate(new AnimeItem
        {
            Id = 1,
            Title = "Naruto",
            IsInList = true,
            MyStatus = MyStatus.Watching,
            WatchedEpisodes = 3,
        });
        return new MainViewModel(services);
    }

    [Fact]
    public void Constructor_LoadsWatchingAnime()
    {
        var vm = NewVm();

        Assert.Single(vm.Library);
        Assert.Contains("1", vm.Status, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SyncNow_WithoutProvider_ReportsQueued()
    {
        var vm = NewVm();

        await vm.SyncNowCommand.ExecuteAsync(null);

        Assert.Contains("cola", vm.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScanNow_WithoutPlayer_ReportsNothing()
    {
        var vm = NewVm();

        await vm.ScanNowCommand.ExecuteAsync(null);

        Assert.Contains("Sin reproducci", vm.NowPlaying, StringComparison.Ordinal);
    }
}
