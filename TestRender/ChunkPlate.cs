using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class ChunkPlate
{
    public int X, Y, Z; //X TopLeft, y TopLeft, z height
    public int Wx, Wy; //wdith x , heigh y
    public float Ax, Ay; // angle alon x, angle along y

    public float GetHeightAt(float x, float y)
    {
        if (x < 0 || x >= Wx || y < 0 || y >= Wy)
        {
            return -1;
        }

        if (Ax == 0 && Ay == 0)
        {
            return Z;
        }

        if (Ax == 0 && Ay != 0)
        {
            var maxHeight = Wy * (float)Math.Sin(Ay);
            var percentage = y / (float)Wy;
            return Z + (percentage * maxHeight);
        }

        if (Ay == 0 && Ax != 0)
        {
            var maxHeight = Wx * (float)Math.Sin(Ax);
            var percentage = x / (float)Wx;
            return Z + (percentage * maxHeight);
        }
        
        if (Ax != 0 && Ay != 0)
        {
            var maxHeightX = Wx * (float)Math.Sin(Ax);
            var maxHeightY = Wy * (float)Math.Sin(Ay);
            var percentageX = x / (float)Wx;
            var percentageY = y / (float)Wy;
            return Z + (percentageX * maxHeightX) + (percentageY * maxHeightY);
        }

        return Z;
    }
    
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
        chunkPlate.Ax = MathHelper.ToRadians(chunkPlate.Ax);

        var ayToken = jChunkPlate["ay"];
        if (ayToken == null)
        {
            throw new Exception("ay is missing");
        }
        chunkPlate.Ay = ayToken.Value<float>();
        chunkPlate.Ay = MathHelper.ToRadians(chunkPlate.Ay);

        return chunkPlate;
    }
}