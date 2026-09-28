using Taiga.Core.Platform;

namespace Taiga.Core.Tests;

public sealed class PathProviderTests
{
    [Fact]
    public void PortableMode_UsesLocalDataFolder()
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, ".portable"), string.Empty);
        var provider = new PathProvider(dir.Path);

        Assert.True(provider.IsPortableMode());
        Assert.Equal(Path.Combine(dir.Path, "data"), provider.GetAppDataDirectory());
        Assert.Equal(Path.Combine(dir.Path, "data"), provider.GetConfigDirectory());
    }

    [Fact]
    public void NonPortable_ResolvesSeparately()
    {
        using var dir = new TempDir();
        var provider = new PathProvider(dir.Path);

        Assert.False(provider.IsPortableMode());
        Assert.NotEqual(provider.GetAppDataDirectory(), string.Empty);
        Assert.NotEqual(provider.GetConfigDirectory(), string.Empty);
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
