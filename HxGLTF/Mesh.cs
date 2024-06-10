namespace HxGLTF
{
    public class MeshPrimitive
    {
        public Dictionary<string, Accessor> Attributes;
        public Accessor? Indices;
        public Material? Material;
        public int Mode = 4; //TODO Create Mode Class
    }

    public class Mesh
    {
        public string? Name;
        public MeshPrimitive[] Primitives;
    }
}
