using Asti.Aseprite.Model;
using Asti.Aseprite.Tests;
using Asti.Conventions.Model;

namespace Asti.Conventions.Tests;

public class TagRulesTests
{
    private static SpriteDefinition Read(params string[] tags) =>
        SpriteConventions.Read(AsepriteBytes.Read(AsepriteBytes.Tags(tags)), "test.aseprite");

    private static IReadOnlyList<string> Errors(params string[] tags) =>
        Assert.Throws<NamingException>(() => Read(tags)).Errors;

    [Fact]
    public void ReadsClipsWithPlaybackAndRepeat()
    {
        var clips = SpriteConventions.Read(Samples.Load("tags.aseprite"), "tags.aseprite").Clips;

        Assert.Equal(
            [
                ("idle", null, 0, 1, AsepriteTagDirection.Forward, 0),
                ("talk", null, 2, 3, AsepriteTagDirection.Reverse, 0),
                ("attack", null, 4, 5, AsepriteTagDirection.PingPong, 3),
                ("silent", (ClipDirection?)null, 6, 7, AsepriteTagDirection.PingPongReverse, 1),
            ],
            clips.Select(c => (c.Name, c.Direction, c.From, c.To, c.Playback, c.Repeat)));
    }

    [Theory]
    [InlineData("idle", "idle", null)]
    [InlineData("walk_down", "walk", ClipDirection.Down)]
    [InlineData("walk_left", "walk", ClipDirection.Left)]
    [InlineData("walk_up", "walk", ClipDirection.Up)]
    [InlineData("walk_right", "walk", ClipDirection.Right)]
    [InlineData("sword_attack_left", "sword_attack", ClipDirection.Left)]
    [InlineData("attack_combo", "attack_combo", null)]
    [InlineData("walk_side", "walk_side", null)]
    [InlineData("down", "down", null)]
    [InlineData("attack2", "attack2", null)]
    public void SplitsNameAndDirection(string tag, string name, ClipDirection? direction)
    {
        var clip = Assert.Single(Read(tag).Clips);

        Assert.Equal((name, direction, tag), (clip.Name, clip.Direction, clip.Tag.Name));
    }

    [Theory]
    [InlineData("Walk-Down")]
    [InlineData("walk__down")]
    [InlineData("walk_")]
    [InlineData("_down")]
    [InlineData("walk down")]
    [InlineData("walk\n")]
    [InlineData("größe")]
    [InlineData("")]
    public void RejectsBadName(string tag)
    {
        Assert.Equal([$"Tag '{tag}': use lower case letters, digits and single '_', like 'walk_down'."], Errors(tag));
    }

    [Fact]
    public void RejectsDuplicatesOncePerExtraTag()
    {
        Assert.Equal(
            Enumerable.Repeat("Tag 'walk_up': this name is used more than once.", 2),
            Errors("walk_up", "walk_up", "walk_down", "walk_up"));
    }

    [Theory]
    [InlineData("walk", "walk_down")]
    [InlineData("walk_down", "walk")]
    public void RejectsClipWithAndWithoutDirection(string first, string second)
    {
        Assert.Equal(
            [$"Tag '{second}': clip 'walk' exists with and without a direction (see tag '{first}')."],
            Errors(first, second));
    }

    [Fact]
    public void BadNamesAreNotCheckedForDuplicates()
    {
        Assert.Equal(2, Errors("Walk", "Walk").Count);
    }

    [Fact]
    public void AllowsFileWithoutTags()
    {
        Assert.Empty(SpriteConventions.Read(AsepriteBytes.Read(AsepriteBytes.Layer("a")), "a").Clips);
    }

    [Fact]
    public void MessageHasPathAndAllErrors()
    {
        var error = Assert.Throws<NamingException>(() => Read("A", "b", "C"));

        Assert.Equal("test.aseprite", error.Path);
        Assert.Equal("test.aseprite: 2 naming errors\n" +
            "  Tag 'A': use lower case letters, digits and single '_', like 'walk_down'.\n" +
            "  Tag 'C': use lower case letters, digits and single '_', like 'walk_down'.", error.Message);
        Assert.StartsWith("test.aseprite: 1 naming error\n", Assert.Throws<NamingException>(() => Read("A")).Message);
    }
}
