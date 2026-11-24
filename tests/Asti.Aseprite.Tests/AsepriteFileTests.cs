using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class AsepriteFileTests
{
    [Fact]
    public void ReadsHeaderOfSingleFrameFile()
    {
        var file = Samples.Load("rgba_single_frame.aseprite");

        Assert.Equal(8, file.Width);
        Assert.Equal(8, file.Height);
        Assert.Equal(AsepriteColorDepth.Rgba, file.ColorDepth);
        Assert.Single(file.Frames);
    }

    [Fact]
    public void ReadsFrameDurations()
    {
        var file = Samples.Load("frames_durations.aseprite");

        Assert.Equal(
            [TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(50)],
            file.Frames.Select(f => f.Duration));
    }

    [Theory]
    [InlineData("rgba_single_frame.aseprite", AsepriteColorDepth.Rgba)]
    [InlineData("grayscale.aseprite", AsepriteColorDepth.Grayscale)]
    [InlineData("indexed.aseprite", AsepriteColorDepth.Indexed)]
    public void ReadsColorDepth(string sample, AsepriteColorDepth expected)
    {
        Assert.Equal(expected, Samples.Load(sample).ColorDepth);
    }

    [Fact]
    public void RejectsFileThatIsNotAseprite()
    {
        using var stream = new MemoryStream(new byte[128]);

        var error = Assert.Throws<InvalidDataException>(() => AsepriteFile.FromStream(stream));
        Assert.Contains("Not an Aseprite file", error.Message);
    }

    [Fact]
    public void RejectsTruncatedFile()
    {
        var bytes = File.ReadAllBytes(Samples.PathOf("frames_durations.aseprite"));
        using var stream = new MemoryStream(bytes[..140]);

        Assert.Throws<InvalidDataException>(() => AsepriteFile.FromStream(stream));
    }
}
