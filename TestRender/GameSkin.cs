using HxGLTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender
{
    public class GameSkin
    {
        public int[] Joints;
        public int? Skeleton;
        public Matrix[] InverseBindMatrices;
        public string Name = string.Empty;
        
        public static GameSkin From(GraphicsDevice graphicsDevice, GLTFFile file, Skin skin)
        {
            var result = new GameSkin();
            
            result.Joints = skin.JointsIndices;
            result.Skeleton = skin.SkeletonIndex;
            result.InverseBindMatrices = skin.InverseBindMatrices;
            result.Name = skin.Name;
            
            return result;
        }
    }
}