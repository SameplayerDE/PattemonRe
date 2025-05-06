using HxGLTF.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HxGLTF.Monogame;

public class PreImpGameModelRenderer
{
    private GraphicsDevice _graphicsDevice;
    private BasicEffect _basicEffect;
    private BasicEffect _wireframeEffect;
    private Effect? _preCompiledEffect;
    
    public bool EnableWireframe = false;
    
    public PreImpGameModelRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _basicEffect = new BasicEffect(graphicsDevice);
        _wireframeEffect = new BasicEffect(graphicsDevice)
        {
            LightingEnabled = false,
            VertexColorEnabled = true,
        };
        _basicEffect.EnableDefaultLighting();
    }
    
    public void LoadPreCompiledEffect(ContentManager contentManager)
    {
        _preCompiledEffect = contentManager.Load<Effect>("HxGLTFPredefinedAssets/SkinShader");
    }
    
    public void DrawModel(GameModel gameModel, Matrix world, Matrix view, Matrix projection, Effect? effect = null)
    {
        foreach (var gameScene in gameModel.Scenes)
        {
            DrawScene(gameModel, gameScene, world, view, projection, effect);
        }
    }
    
    private void DrawScene(GameModel gameModel, GameScene gameScene, Matrix world, Matrix view, Matrix projection, Effect? effect = null)
    {
        foreach (var nodeIndex in gameScene.Nodes)
        {
            DrawNode(gameModel, gameModel.Nodes[nodeIndex], world, view, projection, effect);
        }
    }
    
    private void DrawNode(GameModel gameModel, GameNode gameNode, Matrix world, Matrix view, Matrix projection, Effect? effect = null)
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

    private void DrawMesh(GameModel gameModel, GameMesh gameMesh, Matrix world, Matrix view, Matrix projection, Effect? overrideEffect = null)
    {
        var globalEffect = overrideEffect ?? _preCompiledEffect;
        if (globalEffect == null)
        {
            return;
        }

        // Set view/projection/skinning once per mesh
        globalEffect.Parameters["View"]?.SetValue(view);
        globalEffect.Parameters["Projection"]?.SetValue(projection);
        globalEffect.Parameters["World"]?.SetValue(world);

        bool skinningEnabled = false;
        Matrix[]? joints = null;

        if (gameModel.IsPlaying && gameModel.Skins is { Length: > 0 })
        {
            var skin = gameModel.Skins[0];
            if (skin.JointMatrices.Length <= 180)
            {
                joints = skin.JointMatrices;
                skinningEnabled = true;
                globalEffect.Parameters["Bones"]?.SetValue(joints);
                globalEffect.Parameters["NumberOfBones"]?.SetValue(joints.Length);
            }
        }

        globalEffect.Parameters["SkinningEnabled"]?.SetValue(skinningEnabled);

        foreach (var primitive in gameMesh.Primitives)
        {
            _graphicsDevice.SetVertexBuffer(primitive.VertexBuffer);
            if (primitive.IsIndexed)
            {
                _graphicsDevice.Indices = primitive.IndexBuffer;
            }
            
            globalEffect.Parameters["TextureEnabled"]?.SetValue(primitive.Material.HasTexture);
            if (primitive.Material is { HasTexture: true })
            {
                globalEffect.Parameters["Texture"]?.SetValue(primitive.Material.BaseTexture.Texture);
                _graphicsDevice.SamplerStates[0] = primitive.Material.BaseTexture.Sampler.SamplerState;
            }
            if (primitive.Material is { HasDiffuseTexture: true })
            {
                globalEffect.Parameters["Texture"]?.SetValue(primitive.Material.DiffuseTexture.Texture);
                _graphicsDevice.SamplerStates[0] = primitive.Material.DiffuseTexture.Sampler.SamplerState;
            }
            
            var usedEffect = primitive.Material.Effect ?? globalEffect;
            // if (usedEffect != globalEffect)
            // {
            //     usedEffect.Parameters["View"]?.SetValue(view);
            //     usedEffect.Parameters["Projection"]?.SetValue(projection);
            //     usedEffect.Parameters["World"]?.SetValue(world);
            //     usedEffect.Parameters["SkinningEnabled"]?.SetValue(skinningEnabled);
            //     if (skinningEnabled)
            //     {
            //         usedEffect.Parameters["Bones"]?.SetValue(joints);
            //         usedEffect.Parameters["NumberOfBones"]?.SetValue(joints!.Length);
            //     }
            //     primitive.Material.ApplyEffectParameters?.Invoke(usedEffect, primitive.Material);
            // }
            
            foreach (var pass in usedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                int primitiveCount = primitive.IsIndexed
                    ? primitive.IndexCount / 3
                    : primitive.VertexBuffer.VertexCount / 3;

                if (primitive.IsIndexed)
                {
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
                }
                else
                {
                    _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
                }
            }
        }
    }
}