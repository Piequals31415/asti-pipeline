using Asti.Aseprite.Tests;

namespace Asti.Conventions.Tests;

public class SampleTests
{
    public static TheoryData<string> ValidSamples() =>
    [
        .. Directory.GetFiles(Samples.PathOf(""), "*.aseprite")
            .Select(p => Path.GetFileName(p))
            .Where(n => n != "unicode_names.aseprite")
            .Order(StringComparer.Ordinal),
    ];

    [Fact]
    public void AllSamplesAreCopied()
    {
        Assert.Equal(15, ValidSamples().Count);
    }

    [Theory]
    [MemberData(nameof(ValidSamples))]
    public void SampleHasNoNamingErrors(string name)
    {
        var file = Samples.Load(name);
        var sprite = SpriteConventions.Read(file, name);

        Assert.Same(file, sprite.Source);
        Assert.Equal(file.Layers.Count, sprite.Layers.Count);
        Assert.Equal(file.Tags.Count, sprite.Clips.Count);
    }

    [Fact]
    public void TagUserDataIsNotChecked()
    {
        var clips = SpriteConventions.Read(Samples.Load("user_data.aseprite"), "a").Clips;

        Assert.Equal(["with_data", "without_data"], clips.Select(c => c.Name));
        Assert.Equal("tag note", clips[0].Tag.UserData!.Text);
    }

    [Fact]
    public void UnicodeTagAndSliceAreErrors()
    {
        var error = Assert.Throws<NamingException>(() =>
            SpriteConventions.Read(Samples.Load("unicode_names.aseprite"), "Art/unicode_names.aseprite"));

        Assert.Equal("Art/unicode_names.aseprite: 2 naming errors\n" +
            "  Tag 'Größe_走る': use lower case letters, digits and single '_', like 'walk_down'.\n" +
            "  Slice 'Füße': use lower case letters, digits and single '_', like 'walk_down'.", error.Message);
    }

    [Fact]
    public void ErrorsAreOrderedByKind()
    {
        var file = AsepriteBytes.Read(AsepriteBytes.Slice("attach:", 0, 0), AsepriteBytes.Layer("@"),
            AsepriteBytes.Tags("Walk-Down"));
        var error = Assert.Throws<NamingException>(() => SpriteConventions.Read(file, "a"));

        Assert.Equal(["Tag", "Layer", "Slice"], error.Errors.Select(e => e.Split(' ')[0]));
    }
}
