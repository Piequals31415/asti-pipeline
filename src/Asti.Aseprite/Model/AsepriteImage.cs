namespace Asti.Aseprite.Model;

/// <summary>The pixels of a cel, converted to RGBA for every color mode.</summary>
public sealed class AsepriteImage
{
    internal AsepriteImage(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }

    public int Height { get; }

    /// <summary>4 bytes per pixel (R, G, B, A), row by row from the top left. Alpha is not premultiplied.</summary>
    public ReadOnlyMemory<byte> Pixels { get; }

    public AsepriteColor GetPixel(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height);

        var p = Pixels.Span.Slice((y * Width + x) * 4, 4);
        return new AsepriteColor(p[0], p[1], p[2], p[3]);
    }
}
