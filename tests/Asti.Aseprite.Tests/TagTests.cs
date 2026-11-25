using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class TagTests
{
    [Fact]
    public void ReadsRangesDirectionsAndRepeat()
    {
        var tags = Samples.Load("tags.aseprite").Tags;

        Assert.Equal(
            [
                ("idle", 0, 1, AsepriteTagDirection.Forward, 0),
                ("talk", 2, 3, AsepriteTagDirection.Reverse, 0),
                ("attack", 4, 5, AsepriteTagDirection.PingPong, 3),
                ("silent", 6, 7, AsepriteTagDirection.PingPongReverse, 1),
            ],
            tags.Select(t => (t.Name, t.From, t.To, t.Direction, t.Repeat)));
    }

    [Fact]
    public void ReadsUnicodeName()
    {
        Assert.Equal("Größe_走る", Assert.Single(Samples.Load("unicode_names.aseprite").Tags).Name);
    }

    [Fact]
    public void FileWithoutTagsHasEmptyList()
    {
        Assert.Empty(Samples.Load("rgba_single_frame.aseprite").Tags);
    }

    [Fact]
    public void ReadsColorStoredInTheTag()
    {
        var tag = Assert.Single(AsepriteBytes.Read(AsepriteBytes.Tags("old")).Tags);

        Assert.Equal(new AsepriteColor(1, 2, 3), tag.Color);
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 0, 4)]
    public void RejectsBadRangeOrDirection(int from, int to, int direction)
    {
        var error = Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Chunk(0x2018, w =>
        {
            w.Write((ushort)1);
            w.Write(new byte[8]);
            AsepriteBytes.WriteTag(w, "bad", from, to, direction);
        })));
        Assert.Contains("'bad'", error.Message);
    }
}
