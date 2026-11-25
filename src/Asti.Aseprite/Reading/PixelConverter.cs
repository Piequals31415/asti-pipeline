using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading;

internal static class PixelConverter
{
    public static byte[] ToRgba(byte[] source, AsepriteColorDepth depth, AsepritePalette palette, bool isBackground)
    {
        if (depth == AsepriteColorDepth.Rgba)
            return source;

        var bytesPerPixel = (int)depth / 8;
        var result = new byte[source.Length / bytesPerPixel * 4];
        for (int from = 0, to = 0; from < source.Length; from += bytesPerPixel, to += 4)
        {
            var color = depth == AsepriteColorDepth.Grayscale
                ? new AsepriteColor(source[from], source[from], source[from], source[from + 1])
                : FromIndex(source[from], palette, isBackground);
            result[to] = color.R;
            result[to + 1] = color.G;
            result[to + 2] = color.B;
            result[to + 3] = color.A;
        }

        return result;
    }

    private static AsepriteColor FromIndex(int index, AsepritePalette palette, bool isBackground)
    {
        var isTransparent = index == palette.TransparentIndex && !isBackground;
        return isTransparent || index >= palette.Colors.Count ? default : palette.Colors[index];
    }
}
