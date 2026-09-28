namespace Taiga.Track.Tests;

public sealed class DetectorTests
{
    [Fact]
    public async Task ProcfsDetector_NeverThrows()
    {
        var detector = new ProcfsDetector(["mpv", "vlc"]);

        var state = await detector.DetectAsync();

        Assert.True(state is null || state.IsPlaying);
    }

    [Fact]
    public async Task PlayerctlDetector_AbsentBinary_ReturnsNull()
    {
        if (File.Exists("/usr/bin/playerctl") || File.Exists("/usr/local/bin/playerctl"))
        {
            return;
        }

        var detector = new PlayerctlDetector();
        Assert.Null(await detector.DetectAsync());
    }
}
