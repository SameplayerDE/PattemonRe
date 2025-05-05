using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Implementation.Preprocess;

public class BinaryBuilder
{
    private readonly GraphicsDevice _graphicsDevice;

    public BinaryBuilder(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }
    
    public void Build(string gltfPath, string outputPath)
    {
        // 1. Lade GLTF/GLB-Datei
        var file = GLTFLoader.Load(gltfPath);

        // 2. Baue das GameModel
        var model = GameModel.From(_graphicsDevice, file);

        // 3. Speichere das GameModel als .gmdl
        using var stream = File.Create(outputPath);
        using var writer = new BinaryWriter(stream);
        model.Save(writer);
    }
}