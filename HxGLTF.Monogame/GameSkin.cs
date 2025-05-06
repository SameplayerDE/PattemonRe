using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HxGLTF.Core;

namespace HxGLTF.Monogame
{
    public class GameSkin
    {
        public GameModel Model;
        public int[] Joints;
        public int? Skeleton;
        public Matrix[] InverseBindMatrices;
        public string Name = string.Empty;
        public Matrix[] JointMatrices;
        
        
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Joints.Length; i++)
            {
                int nodeIndex = Joints[i];
                GameNode tnode = Model.Nodes[nodeIndex];
                tnode.UpdateGlobalTransform();

                // Get the global transform of the joint node
                Matrix globalTransform = tnode.GlobalTransform;

                // Get the inverse bind matrix for the joint from the skin
                Matrix inverseBindMatrix = InverseBindMatrices[i];

                // Compute the joint matrix
                Matrix jointMatrix = inverseBindMatrix * globalTransform; // Reihenfolge der Matrixmultiplikation beachten

                // Store the joint matrix in the array
                JointMatrices[i] = jointMatrix;
            }
        }
        
        public static GameSkin From(GraphicsDevice graphicsDevice, GLTFFile file, Skin skin)
        {
            var result = new GameSkin();
            
            result.Joints = skin.JointsIndices;
            result.JointMatrices = new Matrix[result.Joints.Length];
            result.Skeleton = skin.SkeletonIndex;
            result.InverseBindMatrices = skin.InverseBindMatrices.Select(Convert.ToMatrix).ToArray();
            result.Name = skin.Name;
            
            return result;
        }
    }
}