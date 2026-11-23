namespace Asti.Aseprite.Model;

/// <summary>One frame of the sprite.</summary>
/// <param name="Duration">How long the frame is shown.</param>
public sealed record AsepriteFrame(TimeSpan Duration);
