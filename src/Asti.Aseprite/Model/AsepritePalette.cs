namespace Asti.Aseprite.Model;

/// <summary>The colors of the sprite.</summary>
/// <param name="Colors">Colors in palette order. Empty if the file has no palette.</param>
/// <param name="TransparentIndex">Index of the transparent color. Only used by indexed sprites.</param>
public sealed record AsepritePalette(IReadOnlyList<AsepriteColor> Colors, int TransparentIndex);
