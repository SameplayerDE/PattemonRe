using System;
using System.Collections.Generic;
using System.IO;
using HxGLTF.Implementation;

namespace TestRender;

public class Chunk
{
    public ChunkHeader Header;
    public string Id;
    public int X, Y;
    public int Height;
    public GameModel Terrain;
    public List<GameModel> Buildings = [];

    public static Dictionary<string, Chunk> Load(string path)
    {
        return null;
    }
}