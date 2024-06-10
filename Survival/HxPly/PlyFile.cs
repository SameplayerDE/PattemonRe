using Microsoft.Xna.Framework.Graphics;
using Survival.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survival.HxPly
{
    public class PlyFile
    {
        public Header Header;

        public List<VertexPositionColorNormalTexture> VertexData = new List<VertexPositionColorNormalTexture>();
        public List<int[]> IndexData = new List<int[]>();

        public PlyFile()
        {
            Header.ElementDictionary = new Dictionary<ElementType, Element>();
            Header.ElementList = new List<Element>();
        }

        public void PrepareIndices()
        {
            var copy = new List<int[]>(IndexData);
            IndexData.Clear();
            var dataStore = new List<int>();
            foreach(var face in copy)
            {
                int length = face.Length;
                for (int i = 0; i < length; i++)
                {
                    dataStore.Add(face[i]);
                }
            }
            IndexData.Add(dataStore.ToArray());
        }
    }
}
