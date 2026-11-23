namespace Asti.Aseprite.Model;

/// <summary>Color mode of the sprite. The value is the bits per pixel stored in the file.</summary>
public enum AsepriteColorDepth
{
    Indexed = 8,
    Grayscale = 16,
    Rgba = 32,
}
