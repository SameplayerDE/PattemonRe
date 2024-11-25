using System;
using System.IO;
using System.Text;

namespace Nitro;

public class FileCursor
{
    private readonly BinaryReader _reader;

    public long Position => _reader.BaseStream.Position; // Aktuelle Position
    public long Length => _reader.BaseStream.Length;     // Gesamtlänge

    public FileCursor(Stream stream)
    {
        _reader = new BinaryReader(stream);
    }

    public T ReadNextAs<T>(int byteCount = 0)
    {
        if (typeof(T) == typeof(string))
        {
            if (byteCount <= 0) throw new ArgumentException("byteCount muss > 0 sein, um einen String zu lesen.");
            byte[] stringBytes = _reader.ReadBytes(byteCount);
            return (T)(object)Encoding.ASCII.GetString(stringBytes); // Cast zu T
        }
        else if (typeof(T) == typeof(uint))
        {
            return (T)(object)_reader.ReadUInt32();
        }
        else if (typeof(T) == typeof(ushort))
        {
            return (T)(object)_reader.ReadUInt16();
        }
        else if (typeof(T) == typeof(byte))
        {
            return (T)(object)_reader.ReadByte();
        }
        else
        {
            throw new NotSupportedException($"Der Typ {typeof(T).Name} wird nicht unterstützt.");
        }
    }

    public void Skip(int byteCount)
    {
        _reader.BaseStream.Seek(byteCount, SeekOrigin.Current);
    }

    public void SetPosition(long position)
    {
        _reader.BaseStream.Seek(position, SeekOrigin.Begin);
    }
}

public class NitroHeader
{
    public string FileIdentifier { get; private set; }  // 4 Bytes (z. B. 'SDAT', 'BMD0')
    public uint MagicStamp { get; private set; }        // 4 Bytes (z. B. 0x0001FEFF)
    public uint FileSize { get; private set; }          // 4 Bytes (Dateigröße inklusive Header)
    public ushort HeaderSize { get; private set; }      // 2 Bytes (Größe dieser Struktur, immer 16)
    public ushort BlockCount { get; private set; }      // 2 Bytes (Anzahl der Blöcke)
    public List<uint> BlockOffsets { get; private set; } = new List<uint>(); // Dynamische Block-Offets

    public static NitroHeader ReadFromCursor(FileCursor cursor)
    {
        // Lies die Felder des Headers
        var header = new NitroHeader
        {
            FileIdentifier = cursor.ReadNextAs<string>(4), // File identifier
            MagicStamp = cursor.ReadNextAs<uint>(),        // Magic stamp
            FileSize = cursor.ReadNextAs<uint>(),          // File size
            HeaderSize = cursor.ReadNextAs<ushort>(),      // Header size (always 16)
            BlockCount = cursor.ReadNextAs<ushort>()       // Amount of blocks
        };

        // Lies die Block-Offets basierend auf der Blockanzahl
        for (int i = 0; i < header.BlockCount; i++)
        {
            header.BlockOffsets.Add(cursor.ReadNextAs<uint>());
        }

        return header;
    }
}

// Repräsentiert den Header einer einzelnen Nitro-Datei
public struct NitroFileHeader
{
    public string Magic;      // 4 Byte: Magic String
    public uint FileSize;     // 4 Byte: Dateigröße
    public uint Unknown0;     // 4 Byte: Unbekannt
    public uint Unknown1;     // 4 Byte: Unbekannt
    public uint Unknown2;     // 4 Byte: Unbekannt
    public uint Unknown3;     // 4 Byte: Unbekannt
    public uint Unknown4;     // 4 Byte: Unbekannt
    public uint Unknown5;     // 4 Byte: Unbekannt
}

public class NitroFileReader
{
    
    
}