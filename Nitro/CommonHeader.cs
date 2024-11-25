namespace Nitro;

public class CommonHeader(string magic)
{
    public string Magic { get; private set; } = magic;
    public uint Stamp { get; private set; }
    public uint FileSize { get; private set; }
    public ushort HeaderSize { get; private set; }
    public ushort BlockCount { get; private set; } 
    public List<uint> BlockOffsets { get; private set; } = [];

    public static CommonHeader ReadFromCursor(FileCursor cursor)
    {
        var header = new CommonHeader(cursor.ReadNextAs<string>(4))
        {
            Stamp = cursor.ReadNextAs<uint>(),
            FileSize = cursor.ReadNextAs<uint>(),
            HeaderSize = cursor.ReadNextAs<ushort>(),
            BlockCount = cursor.ReadNextAs<ushort>()
        };
        
        for (int i = 0; i < header.BlockCount; i++)
        {
            header.BlockOffsets.Add(cursor.ReadNextAs<uint>());
        }

        return header;
    }
}