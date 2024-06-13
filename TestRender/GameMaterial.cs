using HxGLTF;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestRender
{
    public class GameTextureSampler
    {
        public TextureAddressMode WrapS;
        public TextureAddressMode WrapT;
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
        public bool HasTexture;

        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

        public static GameMaterial From(GraphicsDevice graphicsDevice, GLTFFile file, Material material)
        {
            if (material == null || material.BaseColorTexture == null)
            {
                return new GameMaterial { HasTexture = false };
            }

            var image = material.BaseColorTexture.Source;
            Texture2D texture = null;

            if (!string.IsNullOrEmpty(image.Uri))
            {
                string imagePath = Path.IsPathRooted(image.Uri) ? image.Uri : Path.Combine(Path.GetDirectoryName(file.Path), image.Uri);

                if (!loadedTextures.TryGetValue(imagePath, out texture))
                {
                    using (var stream = File.OpenRead(imagePath))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, stream);
                        loadedTextures[imagePath] = texture;
                    }
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
                    loadedTextures[image.Name] = texture;
                }
            }
            else
            {
                throw new Exception("Image URI and BufferView are both null.");
            }

            var sampler = material.BaseColorTexture.Sampler != null ? new GameTextureSampler
            {
                WrapS = ConvertWrapMode(material.BaseColorTexture.Sampler.WrapS),
                WrapT = ConvertWrapMode(material.BaseColorTexture.Sampler.WrapT)
            } : new GameTextureSampler { WrapS = TextureAddressMode.Wrap, WrapT = TextureAddressMode.Wrap };

            return new GameMaterial
            {
                AlphaCutoff = material.AlphaCutoff ?? 0.5f,
                AlphaMode = material.AlphaMode ?? "OPAQUE",
                BaseTexture = new GameTexture { Texture = texture, Sampler = sampler },
                HasTexture = true
            };
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
