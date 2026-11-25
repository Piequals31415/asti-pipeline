namespace Asti.Aseprite.Model;

/// <summary>One frame of the sprite.</summary>
/// <param name="Duration">How long the frame is shown.</param>
/// <param name="Cels">The cels of this frame in file order. Layers without content have no cel.</param>
public sealed record AsepriteFrame(TimeSpan Duration, IReadOnlyList<AsepriteCel> Cels);
