using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using Survival.HxPly;
using System;
using System.IO;

namespace Survival.Rendering
{
    public class GameModel
    {

        // Placeholder for actual data structure
        // You should define this based on your actual game model requirements
        // For example, it could contain meshes, textures, materials, etc.
        public object Data { get; set; }

        public static GameModel LoadFrom(GraphicsDevice graphicsDevice, string path)
        {
            GameModel result = null;

            // Check if file exists and is not a directory
            if (!File.Exists(path) || Directory.Exists(path))
            {
                throw new FileNotFoundException("The specified file does not exist or is a directory.");
            }

            // Get file extension
            string extension = Path.GetExtension(path).ToLower();

            // Check supported file formats
            if (extension != ".ply" && extension != ".obj" && extension != ".gltf" && extension != ".glb")
            {
                throw new NotSupportedException($"The file format {extension} is not supported.");
            }

            // Handle GLTF and GLB formats
            if (extension == ".gltf" || extension == ".glb")
            {
                // Assuming GLTFLoader.Load returns an instance of a GLTFFile
                GLTFFile gltfFile = GLTFLoader.Load(path);
                result = TransformGLTFToGameModel(gltfFile);
            }
            // Handle PLY format
            else if (extension == ".ply")
            {
                // Assuming PlyLoader.Load returns an instance of a PlyFile
                PlyFile plyFile = PlyLoader.Load(path);
                result = TransformPlyToGameModel(plyFile);
            }
            // OBJ format is currently not supported
            else if (extension == ".obj")
            {
                throw new NotSupportedException("OBJ files are currently not supported.");
            }

            return result;
        }

        private static GameModel TransformGLTFToGameModel(GLTFFile gltfFile)
        {
            GameModel model = new GameModel();
            // TODO: Implement transformation from GLTFFile to GameModel
            // This is a placeholder. You should implement the actual conversion logic.
            model.Data = gltfFile; // Replace with actual transformation logic
            return model;
        }

        private static GameModel TransformPlyToGameModel(PlyFile plyFile)
        {
            GameModel model = new GameModel();
            // TODO: Implement transformation from PlyFile to GameModel
            // This is a placeholder. You should implement the actual conversion logic.
            model.Data = plyFile; // Replace with actual transformation logic
            return model;
        }
    }
}
