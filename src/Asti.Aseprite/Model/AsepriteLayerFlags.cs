namespace Asti.Aseprite.Model;

/// <summary>Layer flags as stored in the file.</summary>
[Flags]
public enum AsepriteLayerFlags
{
    None = 0,
    Visible = 1,
    Editable = 2,
    LockMovement = 4,
    Background = 8,
    PreferLinkedCels = 16,
    Collapsed = 32,
    Reference = 64,
}
