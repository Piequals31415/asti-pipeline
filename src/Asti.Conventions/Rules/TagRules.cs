using Asti.Aseprite.Model;
using Asti.Conventions.Model;

namespace Asti.Conventions.Rules;

internal static class TagRules
{
    public static IReadOnlyList<ClipDefinition> Read(IReadOnlyList<AsepriteTag> tags, List<string> errors)
    {
        var clips = new List<ClipDefinition>();
        foreach (var tag in tags)
        {
            if (!NamingRules.IsStrictName(tag.Name))
            {
                errors.Add($"Tag '{tag.Name}': {NamingRules.StrictNameHint}");
                continue;
            }

            var clip = CreateClip(tag);
            var sameName = clips.Where(c => c.Name == clip.Name).ToList();
            if (sameName.Any(c => c.Direction == clip.Direction))
                errors.Add($"Tag '{tag.Name}': this name is used more than once.");
            else if (sameName.Find(c => c.Direction is null || clip.Direction is null) is { } other)
                errors.Add($"Tag '{tag.Name}': clip '{clip.Name}' exists with and without a direction " +
                    $"(see tag '{other.Tag.Name}').");
            clips.Add(clip);
        }
        return clips.AsReadOnly();
    }

    private static ClipDefinition CreateClip(AsepriteTag tag)
    {
        var split = tag.Name.LastIndexOf(NamingRules.WordSeparator);
        if (split > 0 && NamingRules.Directions.TryGetValue(tag.Name[(split + 1)..], out var direction))
            return new ClipDefinition(tag.Name[..split], direction, tag);
        return new ClipDefinition(tag.Name, null, tag);
    }
}
