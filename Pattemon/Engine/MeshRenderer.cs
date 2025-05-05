using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Engine;

public class MeshRenderer
{
    private GraphicsDevice _graphicsDevice;
    private BasicEffect _basicEffect;

    public MeshRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _basicEffect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            VertexColorEnabled = false,
            LightingEnabled = false,  
            DiffuseColor = Vector3.One,
            AmbientLightColor = Vector3.One,
            EmissiveColor = Vector3.Zero,
            SpecularColor = Vector3.Zero,
            SpecularPower = 1f,
            Alpha = 0.0f
        };
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        _basicEffect.EnableDefaultLighting();
    }
    
    public void DrawModel(GameModel gameModel, Matrix world, Matrix view, Matrix projection, Effect effect = null)
    {
        foreach (var gameScene in gameModel.Scenes)
        {
            DrawScene(gameModel, gameScene, world, view, projection, effect);
        }
    }
    
    private void DrawScene(GameModel gameModel, GameScene gameScene, Matrix world, Matrix view, Matrix projection, Effect effect = null)
    {
        foreach (var nodeIndex in gameScene.Nodes)
        {
            DrawNode(gameModel, gameModel.Nodes[nodeIndex], world, view, projection, effect);
        }
    }
    
    private void DrawNode(GameModel gameModel, GameNode gameNode, Matrix world, Matrix view, Matrix projection, Effect effect = null)
    {
        if (gameNode.HasMesh)
        {
            var nodeTransform = gameNode.GlobalTransform * world;
            DrawMesh(gameModel, gameNode.Mesh, nodeTransform, view, projection, effect);
        }

        if (gameNode.HasChildren)
        {
            foreach (var childIndex in gameNode.Children)
            {
                DrawNode(gameModel, gameModel.Nodes[childIndex], world, view, projection, effect);
            }
        }
    }
    
    private void DrawMesh(GameModel gameModel, GameMesh gameMesh, Matrix world, Matrix view, Matrix projection, Effect effect = null)
    {
        if (effect is not null)
        {
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            
            foreach (var primitive in gameMesh.Primitives)
            {
                _graphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

                if (primitive.Material != null) { 
                    effect.Parameters["Texture"]?.SetValue(primitive.Material.BaseTexture.Texture);
                }


                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
                }
            }
            return;
        }
        
        foreach (var primitive in gameMesh.Primitives)
        {
            _graphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

            _basicEffect.World = world;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;

            if (primitive.Material is { HasTexture: true })
            {
                _basicEffect.TextureEnabled = true;
                _basicEffect.Texture = primitive.Material.BaseTexture.Texture;
                _basicEffect.DiffuseColor = Vector3.One;
            }
            else
            {
                _basicEffect.TextureEnabled = false;
                _basicEffect.Texture = null;
                _basicEffect.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            }

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitive.VertexBuffer.VertexCount / 3);
            }
        }
    }
}