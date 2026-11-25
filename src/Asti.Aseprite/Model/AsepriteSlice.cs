namespace Asti.Aseprite.Model;

/// <summary>A named rectangle on the canvas that can change per frame.</summary>
public sealed class AsepriteSlice
{
    internal AsepriteSlice(string name, IReadOnlyList<AsepriteSliceKey> keys)
    {
        Name = name;
        Keys = keys;
    }

    public string Name { get; }

    public IReadOnlyList<AsepriteSliceKey> Keys { get; }

    /// <summary><c>null</c> if the file stores no user data for this object.</summary>
    public AsepriteUserData? UserData { get; internal set; }
}
