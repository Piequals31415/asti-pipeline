namespace Asti.Conventions.Model;

/// <summary>What a layer is used for. Says nothing about its visibility.</summary>
public enum LayerRole
{
    /// <summary>Normal art.</summary>
    Rendered,

    /// <summary>Read by the tools or the game, never drawn.</summary>
    Data,

    /// <summary>Never exported.</summary>
    Ignored,
}
