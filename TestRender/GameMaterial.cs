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

        public static GameMaterial Default = new GameMaterial()
        {
            AlphaMode = "OPAQUE",
            AlphaCutoff = 0.5f,
            BaseTexture = null,
            BaseColorFactor = Color.White
        };
        
        public string AlphaMode;
        public float AlphaCutoff;
        public GameTexture BaseTexture;
        public GameTexture EmissiveTexture;
        public Color BaseColorFactor;
        public Color EmissiveFactor;
        
        public bool HasTexture => BaseTexture != null;
        public bool HasEmissiveTexture => EmissiveTexture != null;
        
        public static GameMaterial From(GraphicsDevice graphicsDevice, GLTFFile file, Material material)
        {
            if (material == null)
            {
                return Default;
            }

            var result = new GameMaterial();

            result.AlphaCutoff = material.AlphaCutoff;
            result.AlphaMode = material.AlphaMode;
            result.BaseColorFactor = material.BasColorFactor;
            result.EmissiveFactor = material.EmissiveFactor;

            if (material.BaseColorTexture != null)
            {
                if (material.BaseColorTexture.Source == null)
                {
                    throw new Exception();
                }

                Texture2D texture;
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
                
                GameTextureSampler sampler;
                
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
                else
                {
                    sampler = new GameTextureSampler
                    {
                        SamplerState = new SamplerState()
                        {
                            AddressU = TextureAddressMode.Wrap,
                            AddressV = TextureAddressMode.Wrap,
                            Filter = TextureFilter.Point
                        }
                    };
                }

                result.BaseTexture = new GameTexture
                {
                    Texture = texture,
                    Sampler = sampler
                };
            }
            
            if (material.EmissiveTexture != null)
            {
                if (material.EmissiveTexture.Source == null)
                {
                    throw new Exception();
                }

                Texture2D texture;
                var image = material.EmissiveTexture.Source;
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
                
                GameTextureSampler sampler;
                
                if (material.EmissiveTexture.Sampler != null)
                {
                    sampler = new GameTextureSampler
                    {
                        SamplerState = new SamplerState()
                        {
                            AddressU = ConvertWrapMode(material.EmissiveTexture.Sampler.WrapS),
                            AddressV = ConvertWrapMode(material.EmissiveTexture.Sampler.WrapT),
                            Filter = TextureFilter.Point
                        }
                    };
                }
                else
                {
                    sampler = new GameTextureSampler
                    {
                        SamplerState = new SamplerState()
                        {
                            AddressU = TextureAddressMode.Wrap,
                            AddressV = TextureAddressMode.Wrap,
                            Filter = TextureFilter.Point
                        }
                    };
                }

                result.EmissiveTexture = new GameTexture
                {
                    Texture = texture,
                    Sampler = sampler
                };
            }
            
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
