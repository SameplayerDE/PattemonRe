using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace HxGLTF
{
    public class Skin
    {
        //public Accessor? InverseBindMatrices;
        public Matrix[]? InverseBindMatrices;
        public Node? Skeleton;
        public Node[]? Joints;
        public string? Name;

        public int? SkeletonIndex;
        public int[]? JointsIndices;

        public bool HasSkeleton => Skeleton != null;
        public bool HasName => Name != null;
    }
}
