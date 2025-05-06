using HxGLTF.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Monogame;

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

    public static GameMaterial Default = new()
    {
        AlphaMode = "OPAQUE",
        AlphaCutoff = 0.5f,
        BaseTexture = null,
        BaseColorFactor = Color.White
    };

    public string Name;
    public string AlphaMode;
    public float AlphaCutoff;
    public GameTexture? BaseTexture;
    public GameTexture? NormalMap;
    public GameTexture? EmissiveTexture;
    public GameTexture? DiffuseTexture;
    public Color BaseColorFactor;
    public Color EmissiveFactor;
    
    public Effect? Effect = null;
    public Action<Effect, GameMaterial>? ApplyEffectParameters;
    
    public bool HasTexture => BaseTexture != null;
    public bool HasDiffuseTexture => DiffuseTexture != null;
    public bool HasNormalMap => NormalMap != null;
    public bool HasEmissiveTexture => EmissiveTexture != null;
    
    public static GameMaterial From(GraphicsDevice graphicsDevice, GLTFFile file, Material material)
    {
        if (material == null)
        {
            return Default;
        }

        var result = new GameMaterial();

        result.Name = material.Name;
        result.AlphaCutoff = material.AlphaCutoff;
        result.AlphaMode = material.AlphaMode;
        result.BaseColorFactor = new Color(material.BasColorFactor.PackedValue);
        result.EmissiveFactor = new Color(material.EmissiveFactor.PackedValue);

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
                var span = bufferView.Buffer.Bytes.Span.Slice(bufferView.ByteOffset, bufferView.ByteLength);
                byte[] imageData = span.ToArray();

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
        
        if (material.DiffuseTexture != null)
        {
            if (material.DiffuseTexture.Source == null)
            {
                throw new Exception();
            }

            Texture2D texture;
            var image = material.DiffuseTexture.Source;
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
                var span = bufferView.Buffer.Bytes.Span.Slice(bufferView.ByteOffset, bufferView.ByteLength);
                byte[] imageData = span.ToArray();

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
            
            if (material.DiffuseTexture.Sampler != null)
            {
                sampler = new GameTextureSampler
                {
                    SamplerState = new SamplerState()
                    {
                        AddressU = ConvertWrapMode(material.DiffuseTexture.Sampler.WrapS),
                        AddressV = ConvertWrapMode(material.DiffuseTexture.Sampler.WrapT),
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

            result.DiffuseTexture = new GameTexture
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

                using var stream = File.OpenRead(imagePath);
                texture = Texture2D.FromStream(graphicsDevice, stream);
            }
            else if (image.BufferView != null)
            {
                var bufferView = image.BufferView;
                var span = bufferView.Buffer.Bytes.Span.Slice(bufferView.ByteOffset, bufferView.ByteLength);
                byte[] imageData = span.ToArray();

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
        
        if (material.NormalTexture != null)
        {
            if (material.NormalTexture.Source == null)
            {
                throw new Exception();
            }

            Texture2D texture;
            var image = material.NormalTexture.Source;
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
                var span = bufferView.Buffer.Bytes.Span.Slice(bufferView.ByteOffset, bufferView.ByteLength);
                byte[] imageData = span.ToArray();

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
            
            if (material.NormalTexture.Sampler != null)
            {
                sampler = new GameTextureSampler
                {
                    SamplerState = new SamplerState()
                    {
                        AddressU = ConvertWrapMode(material.NormalTexture.Sampler.WrapS),
                        AddressV = ConvertWrapMode(material.NormalTexture.Sampler.WrapT),
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

            result.NormalMap = new GameTexture
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
    
    public void Save(BinaryWriter writer)
    {
    }
}