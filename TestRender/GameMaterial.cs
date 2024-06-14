using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace TestRender
{
    public class GameTextureSampler
    {
        public SamplerState SamplerState;
    }

    public class GameTexture
    {
        public Texture2D Texture;
        public GameTextureSampler Sampler;
    }

    public class GameMaterial
    {
        public string AlphaMode;
        public float AlphaCutoff = 0.5f;
        public GameTexture BaseTexture;
        public Color BaseColorFactor;
        public bool HasTexture;
        
        public static GameMaterial From(GraphicsDevice graphicsDevice, GLTFFile file, Material material)
        {
            if (material == null)
            {
                return new GameMaterial { HasTexture = false };
            }

            var result = new GameMaterial();

            result.AlphaCutoff = material.AlphaCutoff ?? 0.5f;
            result.AlphaMode = material.AlphaMode ?? "OPAQUE";
            Texture2D texture = null;
            var sampler = new GameTextureSampler();
            sampler = new GameTextureSampler
            {
                SamplerState = new SamplerState()
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    Filter = TextureFilter.Point
                }
            };
            if (material.BaseColorTexture != null)
            {
                var image = material.BaseColorTexture.Source;
                

                if (!string.IsNullOrEmpty(image.Uri))
                {
                    string imagePath = Path.IsPathRooted(image.Uri) ? image.Uri : Path.Combine(Path.GetDirectoryName(file.Path), image.Uri);

                    using (var stream = File.OpenRead(imagePath))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, stream);
                        
                    }
                }
                else if (image.BufferView != null)
                {
                    var bufferView = image.BufferView;
                    var buffer = bufferView.Buffer;
                    byte[] imageData = new byte[bufferView.ByteLength];
                    Array.Copy(buffer.Bytes, bufferView.ByteOffset, imageData, 0, bufferView.ByteLength);

                    using (var stream = new MemoryStream(imageData))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, stream);
                    }
                }
                else
                {
                    throw new Exception("Image URI and BufferView are both null.");
                }

                result.HasTexture = true;

                if (material.BaseColorTexture.Sampler != null)
                {

                    sampler = new GameTextureSampler
                    {
                        SamplerState = new SamplerState()
                        {
                            AddressU = ConvertWrapMode(material.BaseColorTexture.Sampler.WrapS),
                            AddressV = ConvertWrapMode(material.BaseColorTexture.Sampler.WrapT),
                            Filter = TextureFilter.Point
                        }
                    };

                }
                
            }
            

            

            
            result.BaseTexture = new GameTexture()
            {
                Texture = texture,
                Sampler = sampler
            };

            result.BaseColorFactor = material.BasColorFactor;
            
            
            return result;
        }

        private static TextureAddressMode ConvertWrapMode(int wrapMode)
        {
            return wrapMode switch
            {
                33071 => TextureAddressMode.Clamp,
                33648 => TextureAddressMode.Mirror,
                10497 => TextureAddressMode.Wrap,
                _ => TextureAddressMode.Wrap
            };
        }
    }
}
