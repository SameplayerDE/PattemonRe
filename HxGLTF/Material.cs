using Microsoft.Xna.Framework;

namespace HxGLTF
{
    public class Material
    {
        public string Name = string.Empty;
        public Texture? BaseColorTexture;
        public Texture? EmissiveTexture;
        public NormalTextureInfo? NormalTexture;
        
        public Color BasColorFactor = Color.White;
        public Color EmissiveFactor = Color.Black;
        public string AlphaMode = "OPAQUE";
        public float AlphaCutoff = 0.5f;
        public bool DoubleSided = false;
    }

    public class NormalTextureInfo
    {
        public Texture Texture;
    }
}