namespace Asti.Aseprite.Model;

/// <summary>The state of a slice from one frame on, until the next key.</summary>
/// <param name="Frame">First frame this key is valid for.</param>
/// <param name="Bounds">Rectangle on the canvas.</param>
/// <param name="Center">9-slice center relative to <paramref name="Bounds"/>, or <c>null</c>.</param>
/// <param name="Pivot">Pivot relative to <paramref name="Bounds"/>, or <c>null</c>.</param>
public sealed record AsepriteSliceKey(int Frame, AsepriteRect Bounds, AsepriteRect? Center, AsepritePoint? Pivot)
{
    /// <summary>True if the slice is hidden from this frame on.</summary>
    public bool IsHidden => Bounds.Width == 0;
}
