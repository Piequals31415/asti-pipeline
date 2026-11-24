using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class LayerTests
{
    [Fact]
    public void ReadsNestedGroups()
    {
        var layers = Samples.Load("layer_groups.aseprite").Layers;

        Assert.Equal(["Body", "Torso", "Layer 2", "Head", "Layer 1"], layers.Select(l => l.Name));
        Assert.Equal([0, 1, 2, 3, 4], layers.Select(l => l.Index));
        Assert.Null(layers[0].Parent);
        Assert.Equal([layers[1], layers[3]], layers[0].Children);
        Assert.Same(layers[1], layers[2].Parent);
        Assert.Same(layers[3], layers[4].Parent);
        Assert.Equal(AsepriteLayerKind.Group, layers[0].Kind);
        Assert.Equal(AsepriteLayerKind.Image, layers[2].Kind);
    }

    [Fact]
    public void GroupsAreOpaqueWhenTheFileHasNoGroupOpacity()
    {
        var groups = Samples.Load("layer_groups.aseprite").Layers.Where(l => l.Kind == AsepriteLayerKind.Group);

        Assert.All(groups, g => Assert.Equal(255, g.Opacity));
    }

    [Fact]
    public void ReadsGroupOpacityAndBlendModeWhenValid()
    {
        var layer = AsepriteBytes.Read(3, AsepriteBytes.Layer("g", kind: 1, blend: 1, opacity: 100)).Layers[0];

        Assert.Equal(100, layer.Opacity);
        Assert.Equal(AsepriteBlendMode.Multiply, layer.BlendMode);
    }

    [Fact]
    public void IgnoresLayerOpacityWhenNotValid()
    {
        Assert.Equal(255, AsepriteBytes.Read(0, AsepriteBytes.Layer("a", opacity: 0)).Layers[0].Opacity);
    }

    [Fact]
    public void ReadsVisibilityAndKeepsNamesUnchanged()
    {
        var layers = Samples.Load("layers_visibility.aseprite").Layers;

        Assert.Equal(["_reference", "visible", "hidden"], layers.Select(l => l.Name));
        Assert.Equal([true, true, false], layers.Select(l => l.IsVisible));
        Assert.All(layers, l => Assert.False(l.IsReference));
    }

    [Fact]
    public void ReadsBackgroundAndReferenceFlags()
    {
        var layers = AsepriteBytes.Read(
            AsepriteBytes.Layer("bg", flags: 9), AsepriteBytes.Layer("ref", flags: 65)).Layers;

        Assert.True(layers[0].IsBackground);
        Assert.False(layers[0].IsReference);
        Assert.True(layers[1].IsReference);
    }

    [Fact]
    public void ReadsBlendModeAndOpacity()
    {
        var layers = Samples.Load("blend_opacity.aseprite").Layers;

        Assert.Equal(
            [AsepriteBlendMode.Luminosity, AsepriteBlendMode.Saturation, AsepriteBlendMode.Normal],
            layers.Select(l => l.BlendMode));
        Assert.Equal([230, 179, 51], layers.Select(l => (int)l.Opacity));
    }

    [Fact]
    public void ReadsTilemapLayer()
    {
        var layer = Assert.Single(Samples.Load("tilemap.aseprite").Layers);

        Assert.Equal("tiles", layer.Name);
        Assert.Equal(AsepriteLayerKind.Tilemap, layer.Kind);
    }

    [Fact]
    public void ReadsUnicodeNames()
    {
        Assert.Equal(["Kopf_ä", "🔥fire"], Samples.Load("unicode_names.aseprite").Layers.Select(l => l.Name));
    }

    [Fact]
    public void RejectsLayerWithoutParent()
    {
        var error = Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Layer("lost", level: 1)));
        Assert.Contains("'lost'", error.Message);
    }

    [Fact]
    public void RejectsParentThatIsNotAGroup()
    {
        Assert.Throws<InvalidDataException>(() =>
            AsepriteBytes.Read(AsepriteBytes.Layer("a"), AsepriteBytes.Layer("b", level: 1)));
    }

    [Fact]
    public void RejectsUnknownKindAndBlendMode()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Layer("a", kind: 9)));
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Layer("a", blend: 99)));
    }
}
