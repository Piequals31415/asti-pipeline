using Asti.Aseprite.Tests;
using Asti.Conventions.Model;

namespace Asti.Conventions.Tests;

public class LayerRulesTests
{
    private const LayerRole Rendered = LayerRole.Rendered;
    private const LayerRole Data = LayerRole.Data;
    private const LayerRole Ignored = LayerRole.Ignored;

    private static byte[] Group(string name, int level = 0) => AsepriteBytes.Layer(name, level, kind: 1);

    private static byte[] Layer(string name, int level = 0, int flags = 3) =>
        AsepriteBytes.Layer(name, level, flags: flags);

    private static IReadOnlyList<LayerDefinition> Read(params byte[][] layers) =>
        SpriteConventions.Read(AsepriteBytes.Read(layers), "test.aseprite").Layers;

    private static IReadOnlyList<string> Errors(params byte[][] layers) =>
        Assert.Throws<NamingException>(() => Read(layers)).Errors;

    private static IEnumerable<(LayerRole, string?)> Roles(params byte[][] layers) =>
        Read(layers).Select(l => (l.Role, l.DataName));

    [Fact]
    public void RoleIsIndependentOfVisibility()
    {
        var file = Samples.Load("layers_visibility.aseprite");
        var layers = SpriteConventions.Read(file, "a").Layers;

        Assert.Equal(file.Layers, layers.Select(l => l.Layer));
        Assert.Equal([Ignored, Rendered, Rendered], layers.Select(l => l.Role));
        Assert.Equal([(Data, "collision")], Roles(Layer("@collision", flags: 0)));
    }

    [Theory]
    [InlineData("layer_groups.aseprite", 5)]
    [InlineData("tilemap.aseprite", 1)]
    public void GroupsAndTilemapsAreRendered(string sample, int count)
    {
        Assert.Equal(Enumerable.Repeat(Rendered, count),
            SpriteConventions.Read(Samples.Load(sample), sample).Layers.Select(l => l.Role));
    }

    [Fact]
    public void NormalNamesAreFreeText()
    {
        Assert.Equal([(Rendered, null), (Rendered, null), (Rendered, null)],
            Roles(Layer("Layer 1"), Layer("Layer 1"), Layer("a@b_")));
    }

    [Fact]
    public void ReferenceLayersAreIgnored()
    {
        Assert.Equal([(Ignored, null)], Roles(Layer("photo", flags: 65)));
    }

    [Fact]
    public void ChildrenOfIgnoredGroupAreIgnoredAndNotChecked()
    {
        Assert.Equal(
            [(Ignored, null), (Ignored, null), (Ignored, null), (Ignored, null), (Data, "events")],
            Roles(Group("_old"), Group("Inner", 1), Layer("@events", 2), Layer("@", 1), Layer("@events")));
    }

    [Fact]
    public void ChildrenOfDataGroupInheritTheName()
    {
        Assert.Equal(
            [(Data, "collision"), (Data, "collision"), (Data, "collision"), (Data, "collision"), (Ignored, null),
                (Ignored, null), (Rendered, null)],
            Roles(Group("@collision"), Group("Walls", 1), Layer("Layer 1", 2), Layer("b", 1), Layer("_sketch", 1),
                Layer("photo", 1, flags: 65), Layer("art")));
    }

    [Fact]
    public void RejectsNestedDataLayer()
    {
        Assert.Equal(["Layer '@a/Inner/@b': a data layer can't be inside another data layer."],
            Errors(Group("@a"), Group("Inner", 1), Layer("@b", 2)));
    }

    [Fact]
    public void RejectsDuplicateDataName()
    {
        Assert.Equal(["Layer 'Body/@hit': the data name 'hit' is used more than once."],
            Errors(Layer("@hit"), Group("Body"), Layer("@hit", 1)));
    }

    [Fact]
    public void RejectsBadDataName()
    {
        Assert.Equal(
            [
                "Layer 'Body/@': a data layer needs a name after '@'.",
                "Layer '@Hit Box': use lower case letters, digits and single '_', like 'walk_down'.",
            ],
            Errors(Group("Body"), Layer("@", 1), Layer("@Hit Box")));
    }
}
