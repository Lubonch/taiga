using Taiga.Core.Media;
using Taiga.Core.Platform;
using Taiga.Core.Settings;
using Taiga.Core.Text;

namespace Taiga.Core.Tests;

public sealed class LibrarySettingsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public LibrarySettingsTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Normalizer_IgnoresCasePunctuationAndAccents()
    {
        Assert.Equal(
            TitleNormalizer.Normalize("Shingeki no Kyojin"),
            TitleNormalizer.Normalize("SHINGEKI NO KYOJIN!"));
    }

    [Fact]
    public void Library_RoundTripsThroughJson()
    {
        var library = new AnimeLibrary();
        library.AddOrUpdate(new AnimeItem { Id = 7, Title = "Naruto", IsInList = true });
        var path = Path.Combine(_dir, "library.json");

        library.Save(path);
        var loaded = AnimeLibrary.Load(path);

        Assert.Equal("Naruto", loaded.FindById(7)?.Title);
    }

    [Fact]
    public void Settings_RoundTripsThroughStore()
    {
        var store = new SettingsStore(new PathProvider(_dir));
        var settings = new AppSettings { Username = "tester", PollIntervalSeconds = 10 };

        store.Save(settings);
        var loaded = new SettingsStore(new PathProvider(_dir)).Load();

        Assert.Equal("tester", loaded.Username);
        Assert.Equal(10, loaded.PollIntervalSeconds);
    }
}
