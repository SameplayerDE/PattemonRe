using HxGLTF;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Survival.Rendering;
using System;
using System.IO;

namespace Survival
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            if (File.Exists("./Redirect.txt"))
            {
                File.Delete("./Redirect.txt");
            }
            try
            {
                ostrm = new FileStream ("./Redirect.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter (ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine ("Cannot open Redirect.txt for writing");
                Console.WriteLine (e.Message);
                return;
            }
            Console.SetOut (writer);

            var file = GLTFLoader.Load(@"Content\Cube");
            foreach (var mesh in file.Meshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    if (primitive.HasIndices)
                    {
                        foreach (var attribute in primitive.Attributes)
                        {
                            Console.WriteLine("-----------------------------------------------------");
                            AccessorReader.ReadDataIndexed(attribute.Value, primitive.Indices);
                        }
                    }
                    else
                    {
                        foreach (var attribute in primitive.Attributes)
                        {
                            Console.WriteLine("-----------------------------------------------------");
                            Console.WriteLine(attribute.Key);
                            AccessorReader.ReadData(attribute.Value);
                        }
                    }
                    
                }
            }

            using var game = new Game1();
            game.Run();
        }
    }
}

