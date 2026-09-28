namespace Taiga.Track.Tests;

public sealed class FilenameParserTests
{
    [Theory]
    [InlineData("[HorribleSubs] Naruto - 12 [720p].mkv", "Naruto", 12)]
    [InlineData("[Erai-raws] One Piece - 1005 [1080p].mkv", "One Piece", 1005)]
    [InlineData("Attack on Titan S02E05.mkv", "Attack on Titan", 5)]
    [InlineData("Death Note - Episode 3.avi", "Death Note", 3)]
    [InlineData("Bleach_366_[1080p].mp4", "Bleach", 366)]
    [InlineData("Jujutsu Kaisen #5 [CR].mkv", "Jujutsu Kaisen", 5)]
    public void Parse_ExtractsTitleAndEpisode(string file, string title, int episode)
    {
        var result = FilenameParser.Parse(file);

        Assert.Equal(title, result.Title);
        Assert.Equal(episode, result.Number);
    }

    [Fact]
    public void Parse_UnknownFormat_KeepsTitleWithoutEpisode()
    {
        var result = FilenameParser.Parse("[Sub] Some Anime Movie [BD].mkv");

        Assert.Equal(-1, result.Number);
        Assert.NotEmpty(result.Title);
    }

    [Theory]
    [InlineData("video.mkv", true)]
    [InlineData("movie.MP4", true)]
    [InlineData("subs.srt", false)]
    [InlineData("cover.jpg", false)]
    public void IsVideoFile_ClassifiesExtensions(string path, bool expected)
    {
        Assert.Equal(expected, FilenameParser.IsVideoFile(path));
    }
}
