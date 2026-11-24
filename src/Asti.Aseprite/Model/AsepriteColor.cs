namespace Asti.Aseprite.Model;

/// <summary>An RGBA color with 8 bits per channel.</summary>
public readonly record struct AsepriteColor(byte R, byte G, byte B, byte A = 255);
