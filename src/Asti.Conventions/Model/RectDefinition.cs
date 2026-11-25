using Asti.Aseprite.Model;

namespace Asti.Conventions.Model;

/// <summary>A named rectangle that can change per frame.</summary>
public sealed class RectDefinition
{
    private readonly SliceTimeline _timeline;

    internal RectDefinition(string name, SliceTimeline timeline)
    {
        Name = name;
        _timeline = timeline;
    }

    public string Name { get; }

    /// <summary>The rectangle on the canvas, or <c>null</c> if there is none in this frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sprite does not have this frame.</exception>
    public AsepriteRect? GetRect(int frame) => _timeline.GetKey(frame)?.Bounds;
}
