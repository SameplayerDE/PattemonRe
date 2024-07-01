using System;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class ChunkPlate
{
    public int X, Y, Z;
    public int Wx, Wy;
    public float Ax, Ay;

    public static ChunkPlate From(JToken jChunkPlate)
    {
        var chunkPlate = new ChunkPlate();

        var xToken = jChunkPlate["x"];
        if (xToken == null)
        {
            throw new Exception("x is missing");
        }
        chunkPlate.X = xToken.Value<int>();

        var yToken = jChunkPlate["y"];
        if (yToken == null)
        {
            throw new Exception("y is missing");
        }
        chunkPlate.Y = yToken.Value<int>();

        var zToken = jChunkPlate["z"];
        if (zToken == null)
        {
            throw new Exception("z is missing");
        }
        chunkPlate.Z = zToken.Value<int>();

        var wxToken = jChunkPlate["wx"];
        if (wxToken == null)
        {
            throw new Exception("wx is missing");
        }
        chunkPlate.Wx = wxToken.Value<int>();

        var wyToken = jChunkPlate["wy"];
        if (wyToken == null)
        {
            throw new Exception("wy is missing");
        }
        chunkPlate.Wy = wyToken.Value<int>();

        var axToken = jChunkPlate["ax"];
        if (axToken == null)
        {
            throw new Exception("ax is missing");
        }
        chunkPlate.Ax = axToken.Value<float>();

        var ayToken = jChunkPlate["ay"];
        if (ayToken == null)
        {
            throw new Exception("ay is missing");
        }
        chunkPlate.Ay = ayToken.Value<float>();

        return chunkPlate;
    }
}