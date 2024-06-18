using System;
using System.Net.Security;
using HxGLTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender
{
    public class GameNode
    {
        public GameModel Model;
        public string Name;
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
                   // Model.Nodes[child].UpdateGlobalTransform();
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
            var result = new GameNode();

            if (node.HasName)
            {
                result.Name = node.Name;
            }

            result.Index = node.Index;

            if (result.Index == 2)
            {
                int i = 0;
            }
            
            result.Translation = node.Translation;
            result.Rotation = node.Rotation;
            result.Scale = node.Scale;
            result.Matrix = node.Matrix;
            
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

    }
}