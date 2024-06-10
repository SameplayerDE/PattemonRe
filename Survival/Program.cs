using HxGLTF;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.IO;

namespace Survival
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var gltfFile = GLTFLoader.Load(@"Content\Fox.glb");
            Console.WriteLine(gltfFile.Asset.Version);
            
            foreach (var image in gltfFile.Images)
            {
                if (image != null)
                {
                    if (!string.IsNullOrEmpty(image.Uri))
                    {
                        if (Path.IsPathRooted(image.Uri))
                        {
                            //loaded.Add(image.Uri, LoadFormFile(image.Uri));
                            Console.WriteLine("URI");
                        }
                        else
                        {
                            var combinedPath = Path.Combine(Path.GetDirectoryName(gltfFile.Path) ?? string.Empty, image.Uri);
                            //loaded.Add(image.Uri, LoadFormFile(combinedPath));
                            Console.WriteLine("URI Combined");
                        }
                    }
                    else
                    {
                        // bufferview
                        Console.WriteLine("Bufferview");
                    }
                }
            }

            using var game = new Game1();
            game.Run();
        }
    }
}

