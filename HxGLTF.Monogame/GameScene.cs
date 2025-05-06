using Microsoft.Xna.Framework.Graphics;
using HxGLTF.Core;

namespace HxGLTF.Monogame;

public class GameScene
{
    public int[]? Nodes;

    public void Save(BinaryWriter writer)
    {
        if (Nodes != null)
        {
            writer.Write(Nodes.Length);
            foreach (var nodeIndex in Nodes)
            {
                writer.Write(nodeIndex);
            }
        }
        else
        {
            writer.Write(0);
        }
    }
        
    public static GameScene Load(BinaryReader reader)
    {
        var scene = new GameScene();

        var count = reader.ReadInt32();
        scene.Nodes = new int[count];
        for (var i = 0; i < count; i++)
        {
            scene.Nodes[i] = reader.ReadInt32();
        }
        return scene;
    }
        
    public static GameScene From(GraphicsDevice graphicsDevice, GLTFFile file, Scene scene)
    {
        var result = new GameScene();

        if (scene.HasNodes)
        {
            result.Nodes = scene.NodesIndices;
        }

        return result;
    }
}