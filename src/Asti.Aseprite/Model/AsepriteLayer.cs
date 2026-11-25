namespace Asti.Aseprite.Model;

/// <summary>A layer or layer group. Name and flags are reported exactly as they are in the file.</summary>
public sealed class AsepriteLayer
{
    private readonly List<AsepriteLayer> _children = [];

    internal AsepriteLayer(int index, string name, AsepriteLayerKind kind, AsepriteLayerFlags flags,
        AsepriteBlendMode blendMode, byte opacity, AsepriteLayer? parent)
    {
        Index = index;
        Name = name;
        Kind = kind;
        Flags = flags;
        BlendMode = blendMode;
        Opacity = opacity;
        Parent = parent;
        Children = _children.AsReadOnly();
        parent?._children.Add(this);
    }

    /// <summary>Position in <see cref="AsepriteFile.Layers"/>, bottom layer first.</summary>
    public int Index { get; }

    public string Name { get; }

    public AsepriteLayerKind Kind { get; }

    public AsepriteLayerFlags Flags { get; }

    /// <summary>The layer's own visibility. Hidden parent groups are not taken into account.</summary>
    public bool IsVisible => Flags.HasFlag(AsepriteLayerFlags.Visible);

    public bool IsBackground => Flags.HasFlag(AsepriteLayerFlags.Background);

    /// <summary>True for Aseprite's own reference layers.</summary>
    public bool IsReference => Flags.HasFlag(AsepriteLayerFlags.Reference);

    public AsepriteBlendMode BlendMode { get; }

    /// <summary>0 = transparent, 255 = opaque. Not multiplied with parent groups.</summary>
    public byte Opacity { get; }

    /// <summary>The group this layer is in, or <c>null</c> at the top level.</summary>
    public AsepriteLayer? Parent { get; }

    /// <summary>Direct children of a group, bottom layer first.</summary>
    public IReadOnlyList<AsepriteLayer> Children { get; }

    /// <summary><c>null</c> if the file stores no user data for this object.</summary>
    public AsepriteUserData? UserData { get; internal set; }
}
