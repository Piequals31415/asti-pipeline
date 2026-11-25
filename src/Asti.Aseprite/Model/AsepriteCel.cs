namespace Asti.Aseprite.Model;

/// <summary>The content of one layer in one frame.</summary>
public sealed class AsepriteCel
{
    internal AsepriteCel(AsepriteLayer layer, int x, int y, byte opacity, int zIndex, int? linkedFrame)
    {
        Layer = layer;
        X = x;
        Y = y;
        Opacity = opacity;
        ZIndex = zIndex;
        LinkedFrame = linkedFrame;
    }

    public AsepriteLayer Layer { get; }

    /// <summary>Left edge on the canvas. Can be negative.</summary>
    public int X { get; }

    /// <summary>Top edge on the canvas. Can be negative.</summary>
    public int Y { get; }

    /// <summary>0 = transparent, 255 = opaque. Not multiplied with the layer opacity.</summary>
    public byte Opacity { get; }

    /// <summary>Moves the cel up or down by this many layers when drawing.</summary>
    public int ZIndex { get; }

    /// <summary>The frame this cel shares its image with, or <c>null</c> if it is not linked.</summary>
    public int? LinkedFrame { get; }

    /// <summary>The pixels, shared between linked cels. <c>null</c> for tilemap cels.</summary>
    public AsepriteImage? Image { get; internal set; }

    /// <summary><c>null</c> if the file stores no user data for this object.</summary>
    public AsepriteUserData? UserData { get; internal set; }
}
