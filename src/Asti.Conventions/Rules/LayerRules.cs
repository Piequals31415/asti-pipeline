using Asti.Aseprite.Model;
using Asti.Conventions.Model;

namespace Asti.Conventions.Rules;

internal static class LayerRules
{
    /// <summary>Returns one definition per layer, in the same order.</summary>
    public static IReadOnlyList<LayerDefinition> Read(IReadOnlyList<AsepriteLayer> layers, List<string> errors)
    {
        var definitions = new List<LayerDefinition>();
        var dataNames = new HashSet<string>();
        foreach (var layer in layers)
        {
            // Groups come before their children, so the parent is already in the list.
            var parent = layer.Parent is null ? null : definitions[layer.Parent.Index];
            definitions.Add(Create(layer, parent, dataNames, errors));
        }
        return definitions.AsReadOnly();
    }

    private static LayerDefinition Create(AsepriteLayer layer, LayerDefinition? parent, HashSet<string> dataNames,
        List<string> errors)
    {
        if (parent?.Role == LayerRole.Ignored || layer.IsReference || HasPrefix(layer, NamingRules.IgnorePrefix))
            return new LayerDefinition(layer, LayerRole.Ignored, null);

        var isData = HasPrefix(layer, NamingRules.DataPrefix);
        if (parent?.Role == LayerRole.Data)
        {
            if (isData)
                errors.Add($"Layer '{PathOf(layer)}': a data layer can't be inside another data layer.");
            return new LayerDefinition(layer, LayerRole.Data, parent.DataName);
        }

        if (!isData)
            return new LayerDefinition(layer, LayerRole.Rendered, null);

        var name = layer.Name[NamingRules.DataPrefix.Length..];
        if (name.Length == 0)
            errors.Add($"Layer '{PathOf(layer)}': a data layer needs a name after '{NamingRules.DataPrefix}'.");
        else if (!NamingRules.IsStrictName(name))
            errors.Add($"Layer '{PathOf(layer)}': {NamingRules.StrictNameHint}");
        else if (!dataNames.Add(name))
            errors.Add($"Layer '{PathOf(layer)}': the data name '{name}' is used more than once.");
        return new LayerDefinition(layer, LayerRole.Data, name);
    }

    private static bool HasPrefix(AsepriteLayer layer, string prefix) =>
        layer.Name.StartsWith(prefix, StringComparison.Ordinal);

    private static string PathOf(AsepriteLayer layer) =>
        layer.Parent is null ? layer.Name : $"{PathOf(layer.Parent)}/{layer.Name}";
}
