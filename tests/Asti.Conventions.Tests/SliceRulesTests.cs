using Asti.Aseprite.Model;
using Asti.Aseprite.Tests;
using Asti.Conventions.Model;

namespace Asti.Conventions.Tests;

public class SliceRulesTests
{
    private static byte[] Slice(string name, params int[][] keys) =>
        AsepriteBytes.Slice(name, (uint)keys.Length, 0, keys);

    private static SpriteDefinition Read(params byte[][] chunks) =>
        SpriteConventions.Read(AsepriteBytes.Read(chunks), "test.aseprite");

    /// <summary>Reads slices in a sprite with 5 frames.</summary>
    private static SpriteDefinition ReadFrames(params byte[][] chunks) =>
        SpriteConventions.Read(AsepriteBytes.ReadFrames(32, chunks, [], [], [], []), "test.aseprite");

    private static IReadOnlyList<string> Errors(params string[] slices) => Assert.Throws<NamingException>(
        () => Read([.. slices.Select(s => Slice(s, [0, 0, 0, 1, 1]))])).Errors;

    [Fact]
    public void ReadsPivotAndRects()
    {
        var sprite = SpriteConventions.Read(Samples.Load("slices.aseprite"), "slices.aseprite");

        Assert.Equal(new AsepritePoint(8, 14), sprite.Pivot!.GetPoint(0));
        Assert.Equal(
            [("nine", new AsepriteRect(4, 4, 24, 24)), ("moving", new AsepriteRect(20, 20, 10, 6))],
            sprite.Rects.Select(r => (r.Name, r.GetRect(0)!.Value)).OrderByDescending(r => r.Name));
        Assert.Empty(sprite.AttachPoints);
    }

    [Theory]
    [InlineData("pivot")]
    [InlineData("origin")]
    public void PivotHasTwoNames(string name)
    {
        var sprite = Read(Slice(name, [0, 3, 4, 1, 1]));

        Assert.Equal(new AsepritePoint(3, 4), sprite.Pivot!.GetPoint(0));
        Assert.Empty(sprite.Rects);
    }

    [Fact]
    public void ReadsAttachPointAndRectNames()
    {
        var sprite = Read(Slice("attach:hand_r", [0, 3, 4, 1, 1]), Slice("hitbox", [0, 1, 2, 3, 4]),
            Slice("hurtbox", [0, 1, 2, 3, 4]), Slice("hand_r", [0, 1, 2, 3, 4]));

        Assert.Equal("hand_r", Assert.Single(sprite.AttachPoints).Name);
        Assert.Equal(["hitbox", "hurtbox", "hand_r"], sprite.Rects.Select(r => r.Name));
        Assert.Null(sprite.Pivot);
    }

    [Theory]
    [InlineData(1, 1, 10, 20)]
    [InlineData(3, 5, 11, 22)]
    [InlineData(4, 2, 11, 20)]
    public void PointWithoutPivotIsTheMiddle(int width, int height, int x, int y)
    {
        Assert.Equal(new AsepritePoint(x, y), Read(Slice("pivot", [0, 10, 20, width, height])).Pivot!.GetPoint(0));
    }

    [Fact]
    public void PointUsesPivotOfTheKey()
    {
        var sprite = Read(AsepriteBytes.Slice("pivot", 1, 2, [0, 10, 20, 4, 4, -1, 7]));

        Assert.Equal(new AsepritePoint(9, 27), sprite.Pivot!.GetPoint(0));
    }

    [Fact]
    public void ValuesChangePerFrame()
    {
        // Not sorted on purpose: visible from 1, hidden at 3, visible at 4, frame 9 does not exist.
        int[][] keys = [[4, 5, 5, 1, 1], [1, 2, 2, 1, 1], [3, 0, 0, 0, 0], [9, 8, 8, 1, 1]];
        var sprite = ReadFrames(Slice("hitbox", keys), Slice("pivot", keys));

        Assert.Equal([null, new(2, 2, 1, 1), new(2, 2, 1, 1), null, new(5, 5, 1, 1)],
            Enumerable.Range(0, 5).Select(sprite.Rects[0].GetRect));
        Assert.Equal([null, new(2, 2), new(2, 2), null, new(5, 5)],
            Enumerable.Range(0, 5).Select(sprite.Pivot!.GetPoint));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void RejectsFrameOutsideTheSprite(int frame)
    {
        var sprite = ReadFrames(Slice("hitbox", [0, 0, 0, 1, 1]), Slice("pivot", [0, 0, 0, 1, 1]));

        Assert.Throws<ArgumentOutOfRangeException>(() => sprite.Rects[0].GetRect(frame));
        Assert.Throws<ArgumentOutOfRangeException>(() => sprite.Pivot!.GetPoint(frame));
    }

    [Theory]
    [InlineData("attach:", "an attach point needs a name after 'attach:'.")]
    [InlineData("attach:Hand", "use lower case letters, digits and single '_', like 'walk_down'.")]
    [InlineData("foo:bar", "unknown prefix, only 'attach:' is allowed.")]
    [InlineData("Hitbox", "use lower case letters, digits and single '_', like 'walk_down'.")]
    public void RejectsBadName(string name, string error)
    {
        Assert.Equal([$"Slice '{name}': {error}"], Errors(name));
    }

    [Theory]
    [InlineData("hitbox")]
    [InlineData("pivot")]
    [InlineData("attach:hand")]
    public void RejectsDuplicates(string name)
    {
        Assert.Equal([$"Slice '{name}': this name is used more than once."], Errors(name, name));
    }

    [Fact]
    public void RejectsPivotAndOrigin()
    {
        Assert.Equal(["Slice 'origin': there is already a pivot (slice 'pivot')."], Errors("pivot", "origin"));
    }
}
