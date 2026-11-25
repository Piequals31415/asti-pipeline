using Asti.Aseprite;
using Asti.Conventions.Model;

namespace Asti.Conventions;

/// <summary>Turns the names in an Aseprite file into meaning.</summary>
public static class SpriteConventions
{
    /// <summary>Applies the naming rules to a file.</summary>
    /// <param name="file">The file to read.</param>
    /// <param name="path">Path of the file, only used in error messages.</param>
    /// <exception cref="NamingException">At least one name is wrong. Contains all errors of the file.</exception>
    public static SpriteDefinition Read(AsepriteFile file, string path)
    {
        var errors = new List<string>();
        var sprite = new SpriteDefinition(file);

        if (errors.Count > 0)
            throw new NamingException(path, errors.AsReadOnly());
        return sprite;
    }
}
