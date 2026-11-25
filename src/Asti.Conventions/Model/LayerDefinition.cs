using Asti.Aseprite.Model;

namespace Asti.Conventions.Model;

/// <summary>A layer and what it is used for.</summary>
/// <param name="Layer">The layer in the file.</param>
/// <param name="Role">What the layer is used for. Children of ignored and data groups inherit it.</param>
/// <param name="DataName">Name without the prefix, or <c>null</c> if this is not a data layer.</param>
public sealed record LayerDefinition(AsepriteLayer Layer, LayerRole Role, string? DataName);
