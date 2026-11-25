namespace Asti.Aseprite.Model;

/// <summary>A named range of frames.</summary>
public sealed class AsepriteTag
{
    private readonly AsepriteColor _oldColor;

    internal AsepriteTag(string name, int from, int to, AsepriteTagDirection direction, int repeat, AsepriteColor color)
    {
        Name = name;
        From = from;
        To = to;
        Direction = direction;
        Repeat = repeat;
        _oldColor = color;
    }

    public string Name { get; }

    /// <summary>First frame, starting at 0.</summary>
    public int From { get; }

    /// <summary>Last frame (included).</summary>
    public int To { get; }

    public AsepriteTagDirection Direction { get; }

    /// <summary>How often the tag plays. 0 = not set.</summary>
    public int Repeat { get; }

    /// <summary>Color of the tag in the timeline.</summary>
    public AsepriteColor Color => UserData?.Color ?? _oldColor;

    /// <summary><c>null</c> if the file stores no user data for this object.</summary>
    public AsepriteUserData? UserData { get; internal set; }
}
