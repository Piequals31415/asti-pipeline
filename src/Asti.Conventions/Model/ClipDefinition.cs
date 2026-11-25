using Asti.Aseprite.Model;

namespace Asti.Conventions.Model;

/// <summary>An animation clip, made from one tag.</summary>
/// <param name="Name">Clip name without the direction.</param>
/// <param name="Direction">Direction from the tag name, or <c>null</c>.</param>
/// <param name="Tag">The tag this clip was made from.</param>
public sealed record ClipDefinition(string Name, ClipDirection? Direction, AsepriteTag Tag)
{
    /// <summary>First frame, starting at 0.</summary>
    public int From => Tag.From;

    /// <summary>Last frame (included).</summary>
    public int To => Tag.To;

    public AsepriteTagDirection Playback => Tag.Direction;

    /// <summary>How often the clip plays. 0 = infinite.</summary>
    public int Repeat => Tag.Repeat;
}
