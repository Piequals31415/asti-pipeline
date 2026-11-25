using Asti.Aseprite;
using Asti.Conventions.Rules;

namespace Asti.Conventions.Model;

/// <summary>What the names in an Aseprite file mean. All coordinates are canvas pixels.</summary>
public sealed class SpriteDefinition
{
    internal SpriteDefinition(AsepriteFile source, IReadOnlyList<ClipDefinition> clips,
        IReadOnlyList<LayerDefinition> layers, SliceRules slices)
    {
        Source = source;
        Clips = clips;
        Layers = layers;
        Pivot = slices.Pivot;
        Rects = slices.Rects;
        AttachPoints = slices.AttachPoints;
    }

    /// <summary>The file this was read from.</summary>
    public AsepriteFile Source { get; }

    /// <summary>One clip per tag, in file order.</summary>
    public IReadOnlyList<ClipDefinition> Clips { get; }

    /// <summary>One entry per layer, same order as <see cref="AsepriteFile.Layers"/>.</summary>
    public IReadOnlyList<LayerDefinition> Layers { get; }

    /// <summary>Origin of the sprite, or <c>null</c> if the file has no pivot slice.</summary>
    public PointDefinition? Pivot { get; }

    /// <summary>Hitboxes and all other plain slices.</summary>
    public IReadOnlyList<RectDefinition> Rects { get; }

    /// <summary>Points to attach things to, for example weapons.</summary>
    public IReadOnlyList<PointDefinition> AttachPoints { get; }
}
