using Microsoft.Xna.Framework;

namespace HxGLTF
{
    public class Material
    {
        public string Name;
        public Texture BaseColorTexture;
        public Color BasColorFactor;
        public Texture NormalTexture;
        public int MetallicFactor;
        public string AlphaMode;
        public float? AlphaCutoff = 0.5f;
        public bool DoubleSided;
    }
}