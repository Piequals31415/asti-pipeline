using System.Text.RegularExpressions;
using Asti.Conventions.Model;

namespace Asti.Conventions.Rules;

/// <summary>The only place where prefixes, special names and directions are defined.</summary>
internal static partial class NamingRules
{
    public const char WordSeparator = '_';
    public const string IgnorePrefix = "_";
    public const string DataPrefix = "@";
    public const char PrefixSeparator = ':';
    public const string AttachPrefix = "attach:";

    public const string StrictNameHint = "use lower case letters, digits and single '_', like 'walk_down'.";

    public static readonly IReadOnlyList<string> PivotNames = ["pivot", "origin"];

    public static readonly IReadOnlyDictionary<string, ClipDirection> Directions =
        new Dictionary<string, ClipDirection>
        {
            ["down"] = ClipDirection.Down,
            ["left"] = ClipDirection.Left,
            ["up"] = ClipDirection.Up,
            ["right"] = ClipDirection.Right,
        };

    public static bool IsStrictName(string name) => StrictName().IsMatch(name);

    [GeneratedRegex(@"\A[a-z0-9]+(_[a-z0-9]+)*\z")]
    private static partial Regex StrictName();
}
