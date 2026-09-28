using Taiga.Core.Compat;
using Taiga.Core.Infrastructure;
using Taiga.Core.Settings;

namespace Taiga.Core.Tests;

public sealed class InfraCompatTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public InfraCompatTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Logger_WritesLevelsToFile()
    {
        var logger = new FileLogger(Path.Combine(_dir, "taiga.log"));
        logger.Info("hello");
        logger.Error("boom");

        var text = File.ReadAllText(Path.Combine(_dir, "taiga.log"));
        Assert.Contains("hello", text, StringComparison.Ordinal);
        Assert.Contains("ERROR", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Rss_ParsesItemTitles()
    {
        const string Xml = """
            <rss><channel>
            <title>Feed</title>
            <item><title>Ep 1</title></item>
            <item><title>Ep 2</title></item>
            </channel></rss>
            """;

        var titles = RssReader.ParseTitles(Xml);

        Assert.Contains("Ep 1", titles);
        Assert.Contains("Ep 2", titles);
    }

    [Fact]
    public void V1Importer_ReadsLegacyIni()
    {
        var ini = Path.Combine(_dir, "settings.ini");
        File.WriteAllText(ini, "# legacy\nusername = tester\nservice = AniList\nlibrary_folder = /anime\n");
        var settings = new AppSettings();

        var count = V1Importer.Import(ini, settings);

        Assert.Equal(3, count);
        Assert.Equal("tester", settings.Username);
        Assert.Contains("/anime", settings.LibraryFolders);
    }

    [Fact]
    public void V1Importer_MissingFile_ReturnsZero()
    {
        Assert.Equal(0, V1Importer.Import(Path.Combine(_dir, "nope.ini"), new AppSettings()));
    }
}
