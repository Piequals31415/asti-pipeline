namespace Asti.Aseprite.Tests;

public class ChunkTests
{
    public static TheoryData<string> AllSamples => new(
        Directory.GetFiles(Samples.PathOf(""), "*.aseprite").Select(p => Path.GetFileName(p)));

    [Theory]
    [MemberData(nameof(AllSamples))]
    public void LoadsEverySample(string sample)
    {
        var file = Samples.Load(sample);

        Assert.NotEmpty(file.Layers);
        Assert.NotEmpty(file.Palette.Colors);
    }

    [Fact]
    public void SkipsUnknownChunks()
    {
        var file = AsepriteBytes.Read(
            AsepriteBytes.Chunk(0x7777, w => w.Write(new byte[50])), AsepriteBytes.Layer("a"));

        Assert.Equal("a", Assert.Single(file.Layers).Name);
    }

    [Fact]
    public void UsesOldChunkCountWhenNewOneIsZero()
    {
        var bytes = AsepriteBytes.File(1, [AsepriteBytes.Layer("a")], oldChunkCount: true);

        Assert.Single(AsepriteFile.FromStream(new MemoryStream(bytes)).Layers);
    }

    [Theory]
    [InlineData(5u)]
    [InlineData(1000u)]
    [InlineData(uint.MaxValue)]
    public void RejectsBadChunkSize(uint size)
    {
        var chunk = AsepriteBytes.Layer("a");
        BitConverter.GetBytes(size).CopyTo(chunk, 0);

        var error = Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(chunk));
        Assert.Contains("Frame 0, chunk 0 (0x2004)", error.Message);
    }

    [Fact]
    public void RejectsChunkThatEndsTooEarly()
    {
        var chunk = AsepriteBytes.Chunk(0x2004, w => w.Write(new byte[4]));

        var error = Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(chunk, AsepriteBytes.Layer("b")));
        Assert.Contains("Unexpected end of chunk", error.Message);
    }
}
