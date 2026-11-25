using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class SliceTests
{
    private static AsepriteSliceKey SingleKey(string slice) => Assert.Single(
        Assert.Single(Samples.Load("slices.aseprite").Slices, s => s.Name == slice).Keys);

    [Fact]
    public void ReadsPivot()
    {
        Assert.Equal(new AsepriteSliceKey(0, new(0, 0, 16, 16), null, new(8, 14)), SingleKey("pivot"));
    }

    [Fact]
    public void ReadsNineSliceCenter()
    {
        Assert.Equal(new AsepriteSliceKey(0, new(4, 4, 24, 24), new(4, 4, 16, 16), null), SingleKey("nine"));
    }

    [Fact]
    public void ReadsPlainSlice()
    {
        var key = SingleKey("moving");

        Assert.Equal(new AsepriteSliceKey(0, new(20, 20, 10, 6), null, null), key);
        Assert.False(key.IsHidden);
    }

    [Fact]
    public void ReadsUnicodeName()
    {
        Assert.Equal("Füße", Assert.Single(Samples.Load("unicode_names.aseprite").Slices).Name);
    }

    [Fact]
    public void ReadsSeveralKeys()
    {
        var keys = AsepriteBytes.Read(Slice(2, 3, [0, -1, -2, 8, 9, 1, 2, 3, 4, -5, 6], [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]))
            .Slices[0].Keys;

        Assert.Equal(new AsepriteSliceKey(0, new(-1, -2, 8, 9), new(1, 2, 3, 4), new(-5, 6)), keys[0]);
        Assert.Equal(2, keys[1].Frame);
        Assert.True(keys[1].IsHidden);
    }

    [Fact]
    public void RejectsNegativeSizeAndMissingKeys()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(Slice(1, 0, [0, 0, 0, -1, 1])));
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(Slice(uint.MaxValue, 0, [0, 0, 0, 1, 1])));
    }

    private static byte[] Slice(uint count, uint flags, params int[][] keys) => AsepriteBytes.Chunk(0x2022, w =>
    {
        w.Write(count);
        w.Write(flags);
        w.Write(0u);
        AsepriteBytes.WriteString(w, "slice");
        foreach (var value in keys.SelectMany(k => k))
            w.Write(value);
    });
}
