# HxGLTF - GLTF Loader for MonoGame

**HxGLTF** is a simple library designed to load **GLTF** files into your **MonoGame** projects. It provides a straightforward method for loading 3D models and animations from GLTF files.

## Features

- Load GLTF / GLB models into MonoGame.
- Support for animations and textures.

![Animations](https://i.imgur.com/K2twCyw.gif)
![Animations_1](https://i.imgur.com/aoXY3gZ.gif)
![Models](https://i.imgur.com/0XMY4Sj.gif)

## Installation

You can install **HxGLTF** via [NuGet](https://www.nuget.org/):

```shell
dotnet add package H073.HxGLTF
```

Or add the following line to your `.csproj` file:

```xml
<PackageReference Include="H073.HxGLTF" Version="1.0.0" />
```

## Usage

This package only provides the functionality to **load** GLTF models. Rendering these models, including handling skinning, materials, and effects, must be implemented manually, as it can vary greatly depending on the specific use case. Below is an example of how you might set up rendering logic for a loaded model.
Models and textures have to be in a folder the code can access.

### Example Rendering Code

```csharp
GameModel.From(GraphicsDevice, GLTFLoader.Load(modelPath))

private void DrawModel(GameTime gameTime, Effect effect, GameModel model, bool alpha = false, Vector3 offset = default)
{
    foreach (var scene in model.Scenes)
    {
        foreach (var nodeIndex in scene.Nodes)
        {
            var node = model.Nodes[nodeIndex];
            DrawNode(gameTime, effect, model, node, alpha, offset);
        }
    }
}

private void DrawNode(GameTime gameTime, Effect effect, GameModel model, GameNode node, bool alpha = false, Vector3 offset = default)
{
    if (node.HasMesh)
    {
        DrawMesh(gameTime, effect, model, node, node.Mesh, alpha, offset);
    }

    if (node.HasChildren)
    {
        foreach (var child in node.Children)
        {
            DrawNode(gameTime, effect, model, node.Model.Nodes[child], alpha, offset);
        }
    }
}

private void DrawMesh(GameTime gameTime, Effect effect, GameModel model, GameNode node, GameMesh mesh, bool alpha = false, Vector3 offset = default)
{
    var worldMatrix = Matrix.CreateScale(model.Scale) *
                      Matrix.CreateFromQuaternion(model.Rotation) *
                      Matrix.CreateTranslation(model.Translation) *
                      Matrix.CreateTranslation(offset);

    effect.Parameters["World"].SetValue(node.GlobalTransform * worldMatrix);
    effect.Parameters["View"].SetValue(Camera.ActiveCamera.ViewMatrix);
    effect.Parameters["Projection"].SetValue(Camera.ActiveCamera.ProjectionMatrix);

    // Skinning logic
    if (model.IsPlaying && model.Skins is { Length: > 0 })
    {
        var skin = model.Skins[0];
        if (skin.JointMatrices.Length > 180)
        {
            effect.Parameters["SkinningEnabled"]?.SetValue(false);
        }
        else
        {
            effect.Parameters["Bones"]?.SetValue(skin.JointMatrices);
            effect.Parameters["NumberOfBones"]?.SetValue(skin.JointMatrices.Length);
            effect.Parameters["SkinningEnabled"]?.SetValue(true);
        }
    }
    else
    {
        effect.Parameters["SkinningEnabled"]?.SetValue(false);
    }
    
    foreach (var primitive in mesh.Primitives)
    {
        if (ShouldSkipPrimitive(primitive, effect, alpha, ref alphaMode))
        {
            continue;
        }

        SetPrimitiveMaterialParameters(gameTime, primitive, effect);

        foreach (var pass in effect.Techniques[Math.Max(alphaMode - 1, 0)].Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                primitive.VertexBuffer.VertexCount / 3);
        }
    }
}

private bool ShouldSkipPrimitive(GameMeshPrimitives primitive, Effect effect, bool alpha, ref int alphaMode)
{
    if (primitive.Material != null)
    {
        var material = primitive.Material;

        switch (material.AlphaMode)
        {
            case "OPAQUE":
                if (alpha)
                {
                    return true;
                }
                alphaMode = 0;
                break;
            case "MASK":
                if (alpha)
                {
                    return true;
                }
                alphaMode = 1;
                break;
            case "BLEND":
                if (!alpha)
                {
                    return true;
                }
                alphaMode = 2;
                break;
        }

        effect.Parameters["AlphaMode"]?.SetValue(alphaMode);
    }
    return false;
}

private void SetPrimitiveMaterialParameters(GameTime gameTime, GameMeshPrimitives primitive, Effect effect)
{
    GraphicsDevice.SetVertexBuffer(primitive.VertexBuffer);

    effect.Parameters["TextureEnabled"]?.SetValue(false);
    effect.Parameters["NormalMapEnabled"]?.SetValue(false);
    effect.Parameters["OcclusionMapEnabled"]?.SetValue(false);
    effect.Parameters["EmissiveTextureEnabled"]?.SetValue(false);

    if (primitive.Material != null)
    {
        var material = primitive.Material;
            
        if (_debugTexture)
        {
            Console.WriteLine(material.Name);
        }
            
        effect.Parameters["EmissiveColorFactor"]?.SetValue(material.EmissiveFactor.ToVector4());
        effect.Parameters["BaseColorFactor"]?.SetValue(material.BaseColorFactor.ToVector4());
        effect.Parameters["AdditionalColorFactor"]?.SetValue(Color.White.ToVector4());
        effect.Parameters["AlphaCutoff"]?.SetValue(material.AlphaCutoff);

        if (material.HasTexture)
        {
            effect.Parameters["TextureEnabled"]?.SetValue(true);
            effect.Parameters["TextureDimensions"]?.SetValue(material.BaseTexture.Texture.Bounds.Size.ToVector2());
            effect.Parameters["Texture"]?.SetValue(material.BaseTexture.Texture);
            effect.Parameters["ShouldAnimate"]?.SetValue(false);
            SetTextureAnimation(gameTime, material, effect);
            SetTextureEffects(gameTime, material, effect);

            GraphicsDevice.SamplerStates[0] = material.BaseTexture.Sampler.SamplerState;
        }
    }
}
```

### Important Notes

This example is provided as a **specific implementation** of how you might render a GLTF model loaded via this library. However, rendering GLTF models can vary greatly depending on the complexity of the model, animation handling, and other factors specific to your project.

Feel free to adapt and expand this code based on your project's needs. For any questions or issues, you can reach me on Discord at **sameplayer**.
