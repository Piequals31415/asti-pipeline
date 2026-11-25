using System.IO.Compression;
using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class CelTests
{
    private static readonly byte[] Red = [255, 0, 0, 255];

    private static AsepriteImage SingleImage(AsepriteFile file) =>
        Assert.Single(Assert.Single(file.Frames).Cels).Image!;

    [Fact]
    public void ReadsRgbaPixels()
    {
        var image = SingleImage(Samples.Load("rgba_single_frame.aseprite"));

        Assert.Equal((8, 8), (image.Width, image.Height));
        Assert.Equal(new AsepriteColor(0xAC, 0x32, 0x32), image.GetPixel(0, 0));
        Assert.Equal(0, image.GetPixel(7, 0).A);
        Assert.Equal(new AsepriteColor(0x99, 0xE5, 0x50), image.GetPixel(6, 1));
        Assert.Equal(new AsepriteColor(0x5B, 0x6E, 0xE1), image.GetPixel(0, 6));
    }

    [Fact]
    public void ConvertsIndexedPixels()
    {
        var file = Samples.Load("indexed.aseprite");
        var image = SingleImage(file);

        Assert.Equal(0, file.Palette.TransparentIndex);
        Assert.Equal(0, image.GetPixel(0, 0).A);
        Assert.Equal(file.Palette.Colors[1], image.GetPixel(5, 0));
        Assert.Equal(file.Palette.Colors[27], image.GetPixel(2, 1));
    }

    [Fact]
    public void ConvertsGrayscalePixels()
    {
        var image = SingleImage(Samples.Load("grayscale.aseprite"));

        Assert.Equal(new AsepriteColor(0xC4, 0xC4, 0xC4), image.GetPixel(2, 0));
        Assert.Equal(0, image.GetPixel(0, 0).A);
    }

    [Theory]
    [InlineData(9, 255)]
    [InlineData(1, 0)]
    public void TransparentIndexIsOpaqueOnBackgroundLayer(int layerFlags, int alpha)
    {
        var palette = AsepriteBytes.Chunk(0x0004, w =>
        {
            w.Write((ushort)1);
            w.Write(new byte[] { 0, 1, 1, 2, 3 });
        });
        var file = AsepriteBytes.ReadFrames(8,
            [AsepriteBytes.Layer("a", flags: layerFlags), AsepriteBytes.RawCel(1, 1, 0), palette]);

        Assert.Equal(alpha, SingleImage(file).GetPixel(0, 0).A);
    }

    [Fact]
    public void IndexOutsideOfPaletteIsTransparent()
    {
        var file = AsepriteBytes.ReadFrames(8, [AsepriteBytes.Layer("a"), AsepriteBytes.RawCel(1, 1, 200)]);

        Assert.Equal(default, SingleImage(file).GetPixel(0, 0));
    }

    [Fact]
    public void ReadsCelPositions()
    {
        var cels = Samples.Load("frames_durations.aseprite").Frames.Select(f => Assert.Single(f.Cels)).ToArray();

        Assert.Equal((0, 2, 4, 5), (cels[0].X, cels[0].Y, cels[0].Image!.Width, cels[0].Image!.Height));
        Assert.Equal((1, 2, 5, 5), (cels[2].X, cels[2].Y, cels[2].Image!.Width, cels[2].Image!.Height));
    }

    [Fact]
    public void ReadsNegativePosition()
    {
        var file = Samples.Load("cel_offset.aseprite");
        var cel = file.Frames[0].Cels[1];

        Assert.Same(file.Layers[1], cel.Layer);
        Assert.Equal((3, -3, 8, 7), (cel.X, cel.Y, cel.Image!.Width, cel.Image.Height));
        Assert.Equal(0, cel.Image.GetPixel(0, 0).A);
        Assert.Equal(new AsepriteColor(0x63, 0x9B, 0xFF), cel.Image.GetPixel(3, 0));
    }

    [Fact]
    public void ReadsOpacityAndZIndex()
    {
        var cel = AsepriteBytes.Read(AsepriteBytes.Layer("a", opacity: 100), AsepriteBytes.Cel(0, w =>
        {
            w.Write((ushort)1);
            w.Write((ushort)1);
            w.Write(Red);
        }, opacity: 128, zIndex: -2)).Frames[0].Cels[0];

        Assert.Equal(128, cel.Opacity);
        Assert.Equal(100, cel.Layer.Opacity);
        Assert.Equal(-2, cel.ZIndex);
        Assert.Equal(new AsepriteColor(255, 0, 0), cel.Image!.GetPixel(0, 0));
    }

    [Fact]
    public void LinkedCelsShareOneImage()
    {
        var frames = Samples.Load("linked_cels.aseprite").Frames;

        Assert.Equal([null, 0, 0], frames.Select(f => f.Cels[0].LinkedFrame));
        Assert.NotNull(frames[0].Cels[0].Image);
        Assert.All(frames, f => Assert.Same(frames[0].Cels[0].Image, f.Cels[0].Image));
        Assert.Equal(3, frames.Select(f => f.Cels[1].Image).Distinct().Count());
    }

    [Fact]
    public void LinkedCelKeepsItsOwnPositionAndOpacity()
    {
        var frames = AsepriteBytes.ReadFrames(32,
            [AsepriteBytes.Layer("a"), AsepriteBytes.RawCel(1, 1, Red)],
            [AsepriteBytes.LinkedCel(0, x: 5, opacity: 10)],
            [AsepriteBytes.LinkedCel(1)]).Frames;

        Assert.Equal((5, 10), (frames[1].Cels[0].X, frames[1].Cels[0].Opacity));
        Assert.Same(frames[0].Cels[0].Image, frames[2].Cels[0].Image);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(50)]
    public void RejectsLinkThatDoesNotPointToAnEarlierCel(int target)
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.ReadFrames(32,
            [AsepriteBytes.Layer("a"), AsepriteBytes.RawCel(1, 1, Red)],
            [AsepriteBytes.LinkedCel(target)],
            [AsepriteBytes.RawCel(1, 1, Red)]));
    }

    [Fact]
    public void RejectsLinkToFrameWithoutCel()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.ReadFrames(32,
            [AsepriteBytes.Layer("a")],
            [AsepriteBytes.LinkedCel(0)]));
    }

    [Fact]
    public void DecompressesLargeImage()
    {
        Assert.Equal(512 * 512 * 4, SingleImage(Samples.Load("large_canvas.aseprite")).Pixels.Length);
    }

    [Fact]
    public void TilemapCelHasNoImage()
    {
        Assert.Null(Assert.Single(Samples.Load("tilemap.aseprite").Frames[0].Cels).Image);
    }

    [Fact]
    public void FrameCanBeEmpty()
    {
        Assert.Empty(AsepriteBytes.Read(AsepriteBytes.Layer("a")).Frames[0].Cels);
        Assert.All(
            SingleImage(Samples.Load("empty_frame.aseprite")).Pixels.ToArray(), b => Assert.Equal(0, b));
    }

    [Fact]
    public void RejectsBadPixelData()
    {
        AssertRejected(AsepriteBytes.RawCel(2, 2, Red));
        AssertRejected(AsepriteBytes.CompressedCel(1, 1, 1, 2, 3, 4, 5, 6, 7, 8));
        AssertRejected(AsepriteBytes.CompressedCel(2, 2, Compress(Red)));
        AssertRejected(AsepriteBytes.CompressedCel(1, 1, Compress(Red)[..4]));
        AssertRejected(AsepriteBytes.RawCel(0, 1));
        AssertRejected(AsepriteBytes.CompressedCel(65535, 65535, Compress(Red)));
    }

    [Fact]
    public void RejectsUnknownLayerTypeAndDuplicates()
    {
        AssertRejected(AsepriteBytes.Cel(0, _ => { }, layer: 1));
        AssertRejected(AsepriteBytes.Cel(9, _ => { }));
        AssertRejected(AsepriteBytes.RawCel(1, 1, Red), AsepriteBytes.RawCel(1, 1, Red));
    }

    private static void AssertRejected(params byte[][] cels)
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            AsepriteBytes.Read([AsepriteBytes.Layer("a"), .. cels]));
        Assert.Contains("(0x2005)", error.Message);
    }

    private static byte[] Compress(byte[] data)
    {
        using var stream = new MemoryStream();
        using (var zlib = new ZLibStream(stream, CompressionLevel.Optimal))
            zlib.Write(data);
        return stream.ToArray();
    }
}
