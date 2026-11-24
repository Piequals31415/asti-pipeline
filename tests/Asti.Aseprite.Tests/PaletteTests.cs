using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class PaletteTests
{
    [Fact]
    public void ReadsOldPaletteChunk()
    {
        var palette = Samples.Load("rgba_single_frame.aseprite").Palette;

        Assert.Equal(32, palette.Colors.Count);
        Assert.Equal(new AsepriteColor(0x00, 0x00, 0x00), palette.Colors[0]);
        Assert.Equal(new AsepriteColor(0x22, 0x20, 0x34), palette.Colors[1]);
        Assert.Equal(new AsepriteColor(0x99, 0xE5, 0x50), palette.Colors[9]);
    }

    [Fact]
    public void OldPaletteCountOfZeroMeans256()
    {
        var colors = Samples.Load("grayscale.aseprite").Palette.Colors;

        Assert.Equal(256, colors.Count);
        Assert.Equal(new AsepriteColor(2, 2, 2), colors[2]);
    }

    [Fact]
    public void ReadsNewPaletteChunk()
    {
        Assert.Equal(2, Samples.Load("slices.aseprite").Palette.Colors.Count);
    }

    [Fact]
    public void OldPaletteSkipsEntriesBetweenPackets()
    {
        var colors = AsepriteBytes.Read(AsepriteBytes.Chunk(0x0004, w =>
        {
            w.Write((ushort)2);
            w.Write(new byte[] { 0, 1, 10, 20, 30 });
            w.Write(new byte[] { 2, 1, 40, 50, 60 });
        })).Palette.Colors;

        Assert.Equal(4, colors.Count);
        Assert.Equal(new AsepriteColor(10, 20, 30), colors[0]);
        Assert.Equal(new AsepriteColor(40, 50, 60), colors[3]);
    }

    [Fact]
    public void ScalesSixBitPalette()
    {
        var colors = AsepriteBytes.Read(AsepriteBytes.Chunk(0x0011, w =>
        {
            w.Write((ushort)1);
            w.Write(new byte[] { 0, 1, 0, 21, 63 });
        })).Palette.Colors;

        Assert.Equal([new AsepriteColor(0, 85, 255)], colors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NewPaletteWinsOverOldOne(bool newFirst)
    {
        var oldChunk = AsepriteBytes.Chunk(0x0004, w =>
        {
            w.Write((ushort)1);
            w.Write(new byte[] { 0, 1, 9, 9, 9 });
        });
        var newChunk = NewPalette(2, 0, 1);

        var colors = (newFirst ? AsepriteBytes.Read(newChunk, oldChunk) : AsepriteBytes.Read(oldChunk, newChunk))
            .Palette.Colors;

        Assert.Equal([new AsepriteColor(0, 2, 3, 4), new AsepriteColor(1, 2, 3, 4)], colors);
    }

    [Fact]
    public void RejectsRangeOutsideOfPalette()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(NewPalette(2, 0, 2)));
    }

    [Fact]
    public void RejectsOldPaletteWithTooManyColors()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Chunk(0x0004, w =>
        {
            w.Write((ushort)1);
            w.Write(new byte[2 + 3 * 256]);
            w.Seek(2, SeekOrigin.Begin);
            w.Write((byte)1);
        })));
    }

    private static byte[] NewPalette(uint size, uint first, uint last) => AsepriteBytes.Chunk(0x2019, w =>
    {
        w.Write(size);
        w.Write(first);
        w.Write(last);
        w.Write(new byte[8]);
        for (var i = first; i <= last; i++)
        {
            w.Write((ushort)1);
            w.Write(new byte[] { (byte)i, 2, 3, 4 });
            AsepriteBytes.WriteString(w, "color name");
        }
    });
}
