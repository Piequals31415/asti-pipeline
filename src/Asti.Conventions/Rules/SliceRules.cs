using Asti.Aseprite.Model;
using Asti.Conventions.Model;

namespace Asti.Conventions.Rules;

internal sealed class SliceRules
{
    private readonly List<RectDefinition> _rects = [];
    private readonly List<PointDefinition> _attachPoints = [];

    private SliceRules()
    {
    }

    public PointDefinition? Pivot { get; private set; }

    public IReadOnlyList<RectDefinition> Rects => _rects.AsReadOnly();

    public IReadOnlyList<PointDefinition> AttachPoints => _attachPoints.AsReadOnly();

    public static SliceRules Read(IReadOnlyList<AsepriteSlice> slices, int frameCount, List<string> errors)
    {
        var result = new SliceRules();
        var names = new HashSet<string>();
        foreach (var slice in slices)
        {
            var error = CheckName(slice.Name);
            if (error is null && !names.Add(slice.Name))
                error = "this name is used more than once.";
            error ??= result.Add(slice, frameCount);

            if (error is not null)
                errors.Add($"Slice '{slice.Name}': {error}");
        }
        return result;
    }

    private static string? CheckName(string name)
    {
        var strictPart = name;
        if (IsAttachPoint(name))
        {
            strictPart = name[NamingRules.AttachPrefix.Length..];
            if (strictPart.Length == 0)
                return $"an attach point needs a name after '{NamingRules.AttachPrefix}'.";
        }
        else if (name.Contains(NamingRules.PrefixSeparator))
        {
            return $"unknown prefix, only '{NamingRules.AttachPrefix}' is allowed.";
        }
        return NamingRules.IsStrictName(strictPart) ? null : NamingRules.StrictNameHint;
    }

    private string? Add(AsepriteSlice slice, int frameCount)
    {
        var timeline = new SliceTimeline(slice, frameCount);
        if (NamingRules.PivotNames.Contains(slice.Name))
        {
            if (Pivot is not null)
                return $"there is already a pivot (slice '{Pivot.Name}').";
            Pivot = new PointDefinition(slice.Name, timeline);
        }
        else if (IsAttachPoint(slice.Name))
        {
            _attachPoints.Add(new PointDefinition(slice.Name[NamingRules.AttachPrefix.Length..], timeline));
        }
        else
        {
            _rects.Add(new RectDefinition(slice.Name, timeline));
        }
        return null;
    }

    private static bool IsAttachPoint(string name) =>
        name.StartsWith(NamingRules.AttachPrefix, StringComparison.Ordinal);
}
