using Asti.Aseprite.Model;

namespace Asti.Aseprite.Tests;

public class UserDataTests
{
    private static byte[] Text(string text) => AsepriteBytes.Chunk(0x2020, w =>
    {
        w.Write(1u);
        AsepriteBytes.WriteString(w, text);
    });

    private static byte[] Slice() => AsepriteBytes.Chunk(0x2022, w =>
    {
        w.Write(0u);
        w.Write(0u);
        w.Write(0u);
        AsepriteBytes.WriteString(w, "slice");
    });

    private static byte[] OldPalette() => AsepriteBytes.Chunk(0x0004, w => w.Write((ushort)0));

    [Fact]
    public void ReadsUserDataOfEveryObject()
    {
        var file = Samples.Load("user_data.aseprite");

        Assert.Equal(new AsepriteUserData("sprite note", new(255, 0, 255)), file.UserData);
        Assert.Equal(new AsepriteUserData("layer note", new(255, 0, 0)), file.Layers[0].UserData);
        Assert.Equal(new AsepriteUserData("cel note", new(0, 255, 0)), file.Frames[0].Cels[0].UserData);
        Assert.Null(file.Frames[1].Cels[0].UserData);
    }

    [Fact]
    public void TagsGetTheirOwnUserData()
    {
        var tags = Samples.Load("user_data.aseprite").Tags;

        Assert.Equal(["with_data", "without_data"], tags.Select(t => t.Name));
        Assert.Equal(new AsepriteUserData("tag note", new(0, 0, 255)), tags[0].UserData);
        Assert.Equal(new AsepriteColor(0, 0, 255), tags[0].Color);
        Assert.Null(tags[1].UserData!.Text);
    }

    [Fact]
    public void UserDataColorWinsOverOldTagColor()
    {
        var color = AsepriteBytes.Chunk(0x2020, w =>
        {
            w.Write(2u);
            w.Write(new byte[] { 7, 8, 9, 10 });
        });

        Assert.Equal(new AsepriteColor(7, 8, 9, 10), AsepriteBytes.Read(AsepriteBytes.Tags("a"), color).Tags[0].Color);
        Assert.Equal(new AsepriteColor(1, 2, 3), AsepriteBytes.Read(AsepriteBytes.Tags("a"), Text("x")).Tags[0].Color);
    }

    [Fact]
    public void TileUserDataIsNotGivenToOtherObjects()
    {
        var file = Samples.Load("tilemap.aseprite");

        Assert.Null(file.UserData);
        Assert.Null(file.Layers[0].UserData);
        Assert.Null(file.Frames[0].Cels[0].UserData);
    }

    [Fact]
    public void TilesetUserDataDoesNotReachTheSprite()
    {
        var file = AsepriteBytes.Read(OldPalette(), AsepriteBytes.Chunk(0x2023, _ => { }), Text("tileset"), Text("tile"));

        Assert.Null(file.UserData);
    }

    [Fact]
    public void ReadsSliceUserData()
    {
        Assert.Equal("note", AsepriteBytes.Read(Slice(), Text("note")).Slices[0].UserData!.Text);
    }

    [Fact]
    public void CelExtraChunkDoesNotChangeTheTarget()
    {
        var file = AsepriteBytes.Read(AsepriteBytes.Layer("a"), AsepriteBytes.RawCel(1, 1, 0, 0, 0, 0),
            AsepriteBytes.Chunk(0x2006, w => w.Write(new byte[36])), Text("cel"));

        Assert.Equal("cel", file.Frames[0].Cels[0].UserData!.Text);
        Assert.Null(file.Layers[0].UserData);
    }

    [Fact]
    public void PaletteInLaterFrameDoesNotSetSpriteUserData()
    {
        var file = AsepriteBytes.ReadFrames(32, [OldPalette(), Text("sprite")], [OldPalette(), Text("other")]);

        Assert.Equal("sprite", file.UserData!.Text);
    }

    [Fact]
    public void IgnoresProperties()
    {
        var chunk = AsepriteBytes.Chunk(0x2020, w =>
        {
            w.Write(5u);
            AsepriteBytes.WriteString(w, "text");
            w.Write(new byte[20]);
        });
        var layers = AsepriteBytes.Read(AsepriteBytes.Layer("a"), chunk, AsepriteBytes.Layer("b")).Layers;

        Assert.Equal(new AsepriteUserData("text", null), layers[0].UserData);
        Assert.Null(layers[1].UserData);
    }

    [Fact]
    public void UserDataWithoutTargetIsIgnored()
    {
        Assert.Null(AsepriteBytes.Read(Text("lost"), AsepriteBytes.Layer("a")).Layers[0].UserData);
    }

    [Fact]
    public void RejectsTruncatedText()
    {
        Assert.Throws<InvalidDataException>(() => AsepriteBytes.Read(AsepriteBytes.Chunk(0x2020, w =>
        {
            w.Write(1u);
            w.Write((ushort)10);
        })));
    }
}
