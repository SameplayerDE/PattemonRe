using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HxGLTF.Core;

namespace HxGLTF.Monogame;

public class GameNode
{
    public GameModel Model;
    public string? Name;
    public int[] Children;
    public bool HasChildren;
    public int MeshIndex;
    public bool HasMesh;
    public GameMesh Mesh;
    public int Index;
        
    public GameNode Parent;
        
    public Matrix LocalTransform;
    public Matrix GlobalTransform;
        
    public Matrix Matrix;
    public Vector3 Translation;
    public Vector3 Scale;
    public Quaternion Rotation;

    public void UpdateGlobalTransform()
    {
        // Berechne die globale Transformation basierend auf der lokalen und der Eltern-Transformation
        GlobalTransform = LocalTransform;
        if (Parent != null)
        {
            GlobalTransform *= Parent.GlobalTransform;
        }

        // Rekursive Aktualisierung der globalen Transformationen der Kind-Knoten
        if (HasChildren)
        {
            foreach (var child in Children)
            {
                //Model.Nodes[child].UpdateGlobalTransform();
            }
        }
    }

    public void Rotate(Quaternion rotation)
    {
        // Aktualisiere die Rotation
        Rotation = rotation; 
        // Aktualisiere die lokale Transformationsmatrix
        LocalTransform = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);

        // Aktualisiere die globale Transformation
        UpdateGlobalTransform();
    }

    public void Translate(Vector3 translation)
    {
        // Aktualisiere die Translation
        Translation = translation;
        // Aktualisiere die lokale Transformationsmatrix
        LocalTransform = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);

        // Aktualisiere die globale Transformation
        UpdateGlobalTransform();
    }

    public void Resize(Vector3 scale)
    {
        // Aktualisiere die Skalierung
        Scale = scale;
        // Aktualisiere die lokale Transformationsmatrix
        LocalTransform = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);

        // Aktualisiere die globale Transformation
        UpdateGlobalTransform();
    }

    public static GameNode From(GraphicsDevice graphicsDevice, GLTFFile file, Node node)
    {
        var result = new GameNode
        {
            Name = node.Name,
            Index = node.Index,
            Translation = Convert.ToVector3(node.Translation),
            Rotation = Convert.ToQuaternion(node.Rotation),
            Scale = Convert.ToVector3(node.Scale),
            Matrix = Convert.ToMatrix(node.Matrix),
        };

        result.LocalTransform = Matrix.CreateScale(result.Scale) *
                                Matrix.CreateFromQuaternion(result.Rotation) *
                                Matrix.CreateTranslation(result.Translation);
            
        result.GlobalTransform = result.LocalTransform;
            
        if (node.HasMesh)
        {
            result.HasMesh = true;
            result.MeshIndex = node.Mesh!.Index;
            result.Mesh = GameMesh.From(graphicsDevice, file, node.Mesh);
        }

        if (node.HasChildren)
        {
            result.HasChildren = true;
            result.Children = new int[node.Children!.Length];
            for (int i = 0; i < node.Children.Length; i++)
            {
                var child = node.Children[i];
                result.Children[i] = child.Index;
            }
        }

        return result;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(Name != null);
        if (Name != null)
        {
            writer.Write(Name);
        }
            
        writer.Write(Index);
            
        writer.Write(Translation.X);
        writer.Write(Translation.Y);
        writer.Write(Translation.Z);
        writer.Write(Scale.X);
        writer.Write(Scale.Y);
        writer.Write(Scale.Z);
        writer.Write(Rotation.X);
        writer.Write(Rotation.Y);
        writer.Write(Rotation.Z);
        writer.Write(Rotation.W);
            
        writer.Write(HasMesh);
        writer.Write(MeshIndex);
            
        writer.Write(HasChildren);
        if (!HasChildren)
        {
            return;
        }
        writer.Write(Children.Length);
        foreach (var child in Children)
        {
            writer.Write(child);
        }
    }

    public static GameNode Load(BinaryReader reader)
    {
        var result = new GameNode();
        if (reader.ReadBoolean())
        {
            result.Name = reader.ReadString();
        }
        result.Index = reader.ReadInt32();
        
        result.Translation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        result.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        result.Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        
        result.HasMesh = reader.ReadBoolean();
        result.MeshIndex = reader.ReadInt32();

        if (reader.ReadBoolean())
        {
            result.HasChildren = true;
            var childCount = reader.ReadInt32();
            result.Children = new int[childCount];
            for (var i = 0; i < childCount; i++)
            {
                result.Children[i] = reader.ReadInt32();
            }
        }

        return result;
    }
}