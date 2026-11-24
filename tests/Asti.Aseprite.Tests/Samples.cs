namespace Asti.Aseprite.Tests;

internal static class Samples
{
    public static string PathOf(string name) => Path.Combine(AppContext.BaseDirectory, "samples", name);

    public static AsepriteFile Load(string name) => AsepriteFile.FromFile(PathOf(name));
}
