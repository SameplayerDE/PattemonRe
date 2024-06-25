using System.Collections.Generic;
using HxGLTF.Implementation;

namespace TestRender;

public class Chunk
{
    public string Id;
    public int X, Y;
    public int Height;
    public GameModel Terrain;
    public List<GameModel> Buildings = [];
}