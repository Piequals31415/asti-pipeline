namespace Asti.Aseprite.Reading;

internal enum ChunkType : ushort
{
    OldPalette = 0x0004,
    OldPalette64 = 0x0011,
    Layer = 0x2004,
    Palette = 0x2019,
}
