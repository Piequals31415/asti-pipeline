namespace Asti.Aseprite.Reading;

internal enum ChunkType : ushort
{
    OldPalette = 0x0004,
    OldPalette64 = 0x0011,
    Layer = 0x2004,
    Cel = 0x2005,
    Tags = 0x2018,
    Palette = 0x2019,
    UserData = 0x2020,
    Slice = 0x2022,
    Tileset = 0x2023,
}
