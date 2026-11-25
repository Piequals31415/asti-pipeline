using Asti.Aseprite.Model;

namespace Asti.Conventions.Model;

/// <summary>A named point that can change per frame.</summary>
public sealed class PointDefinition
{
    private readonly SliceTimeline _timeline;

    internal PointDefinition(string name, SliceTimeline timeline)
    {
        Name = name;
        _timeline = timeline;
    }

    public string Name { get; }

    /// <summary>The point on the canvas, or <c>null</c> if there is none in this frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sprite does not have this frame.</exception>
    public AsepritePoint? GetPoint(int frame)
    {
        if (_timeline.GetKey(frame) is not { } key)
            return null;

        var bounds = key.Bounds;
        var offset = key.Pivot ?? new AsepritePoint((bounds.Width - 1) / 2, (bounds.Height - 1) / 2);
        return new AsepritePoint(bounds.X + offset.X, bounds.Y + offset.Y);
    }
}
