namespace Asti.Conventions;

/// <summary>One or more names in an Aseprite file break the naming rules.</summary>
public sealed class NamingException : Exception
{
    internal NamingException(string path, IReadOnlyList<string> errors)
        : base(CreateMessage(path, errors))
    {
        Path = path;
        Errors = errors;
    }

    /// <summary>The file with the wrong names.</summary>
    public string Path { get; }

    /// <summary>All errors of the file, one line each.</summary>
    public IReadOnlyList<string> Errors { get; }

    private static string CreateMessage(string path, IReadOnlyList<string> errors)
    {
        var title = $"{path}: {errors.Count} naming {(errors.Count == 1 ? "error" : "errors")}";
        return string.Join('\n', errors.Select(e => $"  {e}").Prepend(title));
    }
}
