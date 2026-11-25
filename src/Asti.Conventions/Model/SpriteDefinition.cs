using Asti.Aseprite;

namespace Asti.Conventions.Model;

/// <summary>What the names in an Aseprite file mean. All coordinates are canvas pixels.</summary>
public sealed class SpriteDefinition
{
    internal SpriteDefinition(AsepriteFile source)
    {
        Source = source;
    }

    /// <summary>The file this was read from.</summary>
    public AsepriteFile Source { get; }
}
