using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HxGLTF.Core;

namespace HxGLTF.Monogame;

public class GameModel
{
    public GameNode[] Nodes;
    public GameScene[] Scenes;
    public GameMesh[] Meshes;
    public GameSkin[] Skins;
    public GameModelAnimation[] Animations;
    
    private BoundingBox _cachedBoundingBox;
    private bool _boundingBoxDirty = true;
    public BoundingBox BoundingBox
    {
        get
        {
            if (_boundingBoxDirty)
            {
                _cachedBoundingBox = CalculateModelBounds(this);
                _boundingBoxDirty = false;
            }
            return _cachedBoundingBox;
        }
    }

    public Matrix GlobalTranformation;
    public Matrix LocalTranformation;
        
    public Vector3 Translation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;
    public Quaternion Rotation = Quaternion.Identity;
        
    private int _currentAnimationIndex;
    private int _nextAnimationIndex;
        
    private float _animationTimer;
    public bool IsPlaying;
    public float AnimationScale = 0.5f;
        
    public event Action? OnAnimationCompleted;
    // ToDo: AnimationPlayerClass

    static internal BoundingBox CalculateModelBounds(GameModel model)
    {
        BoundingBox? total = null;

        // Welt-Transform des gesamten Modells
        var modelTransform =
            Matrix.CreateScale(model.Scale) *
            Matrix.CreateFromQuaternion(model.Rotation) *
            Matrix.CreateTranslation(model.Translation);

        foreach (var mesh in model.Meshes)
        {
            foreach (var primitive in mesh.Primitives)
            {
                var transformedPositions = new Vector3[primitive.Positions.Length];

                for (int i = 0; i < primitive.Positions.Length; i++)
                {
                    transformedPositions[i] = Vector3.Transform(primitive.Positions[i], modelTransform);
                }

                var box = BoundingBox.CreateFromPoints(transformedPositions);
                total = total.HasValue ? BoundingBox.CreateMerged(total.Value, box) : box;
            }
        }

        return total ?? new BoundingBox(Vector3.Zero, Vector3.Zero);
    }
    
    public void Dispose()
    {
        // Freigabe von Ressourcen und Zurücksetzen der Felder
        if (Nodes != null)
        {
            foreach (var node in Nodes)
            {
                //node.Dispose();
            }
            Nodes = null;
        }

        if (Scenes != null)
        {
            foreach (var scene in Scenes)
            {
                //scene.Dispose();
            }
            Scenes = null;
        }

        if (Meshes != null)
        {
            foreach (var mesh in Meshes)
            {
                mesh.Dispose();
            }
            Meshes = null;
        }

        if (Skins != null)
        {
            foreach (var skin in Skins)
            {
                //skin.Dispose();
            }
            Skins = null;
        }

        if (Animations != null)
        {
            foreach (var animation in Animations)
            {
                //animation.Dispose();
            }
            Animations = null;
        }

        GlobalTranformation = Matrix.Identity;
        LocalTranformation = Matrix.Identity;
        Translation = Vector3.Zero;
        Scale = Vector3.One;
        Rotation = Quaternion.Identity;

        _currentAnimationIndex = 0;
        _nextAnimationIndex = 0;
        _animationTimer = 0f;
        IsPlaying = false;
    }
    
    public GameMaterial? GetMaterialByName(string name)
    {
        foreach (var mesh in Meshes)
        {
            foreach (var primitive in mesh.Primitives)
            {
                var material = primitive.Material;
                if (material.Name == name)
                {
                    return material;
                }
            }
        }
        return null;
    }
        
    public void Play(int index)
    {
        _currentAnimationIndex = index;
        IsPlaying = true;
        // set what animation should be played
        // check if currenlty animaton is running
        // if currently not then just play the animation 
        // if yes then try to interpolate currently running animation with next animation so that there is not visual cut
    }
        
    public void Prepare(int index)
    {
        if (Animations == null || index >= Animations.Length)
            return;

        _currentAnimationIndex = index;
        _animationTimer = 0f;
        IsPlaying = false;
            
        GameModelAnimation currentAnimation = Animations[_currentAnimationIndex];
        UpdateAnimation(currentAnimation, _animationTimer);
    }
        
    public void Stop()
    {
        _currentAnimationIndex = -1;
        IsPlaying = false;
        // set what animation should be played
        // check if currenlty animaton is running
        // if currently not then just play the animation 
        // if yes then try to interpolate currently running animation with next animation so that there is not visual cut
    }
        
    public void Reset()
    {
        _animationTimer = 0f;
    }
        
    public void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        delta *= AnimationScale;

        if (Animations != null)
        {
            if (IsPlaying)
            {
                GameModelAnimation currentAnimation;
                if (Animations.Length <= _currentAnimationIndex)
                {
                    throw new Exception();
                }

                currentAnimation = Animations[_currentAnimationIndex];
                UpdateAnimation(currentAnimation, _animationTimer);
                _animationTimer += delta;
                if (_animationTimer > currentAnimation.Duration)
                {
                    OnAnimationCompleted?.Invoke();
                    _animationTimer = 0;
                }
            }
        }

        if (Skins != null)
        {
            if (Skins.Length > 0)
            {
                foreach (var skin in Skins)
                {
                    skin.Update(gameTime);
                }
            }
        }
    }

    private void UpdateAnimation(GameModelAnimation animation, float animationTime)
    {
            
        //GameModelAnimation nextAnimation;
        //if (Animations.Length <= _nextAnimationIndex)
        //{
        //    throw new Exception();
        //}
        //nextAnimation = Animations[_nextAnimationIndex];
            
        foreach (var channel in animation.Channels)
        {
            var targetPath = channel.Target.Path;

            var valuesPerElement = targetPath switch
            {
                AnimationChannelTargetPath.Rotation => 4,
                AnimationChannelTargetPath.Translation or AnimationChannelTargetPath.Scale => 3,
                AnimationChannelTargetPath.Texture => 2,
                _ => 0
            };

            if (valuesPerElement <= 0)
            {
                continue;
            }
            var sampler = animation.Samplers[channel.SamplerIndex];
            var timeStamps = sampler.Input;

            var prevIndex = -1;
            var nextIndex = -1;

            // Find the indices for the surrounding keyframes
            for (var i = 0; i < timeStamps.Length; i++)
            {
                if (timeStamps[i] <= animationTime)
                {
                    prevIndex = i;
                }

                if (!(timeStamps[i] > animationTime))
                {
                    continue;
                }
                nextIndex = i;
                break;
            }

            if (nextIndex == -1)
            {
                nextIndex = 0;
            }

            switch (sampler.InterpolationAlgorithm)
            {
                case InterpolationAlgorithm.Step:
                {
                    // For step interpolation, just use the previous index
                    if (prevIndex >= 0)
                    {
                        var data = new float[valuesPerElement];
                        for (int i = 0; i < valuesPerElement; i++)
                        {
                            int offset = prevIndex * valuesPerElement;
                            data[i] = sampler.Output[offset + i];
                        }

                        if (targetPath == AnimationChannelTargetPath.Rotation)
                        {
                            Quaternion rotation = new Quaternion(data[0], data[1], data[2], data[3]);
                            Nodes[channel.Target.NodeIndex].Rotate(rotation);
                        }
                        else if (targetPath == AnimationChannelTargetPath.Translation)
                        {
                            Vector3 translation = new Vector3(data[0], data[1], data[2]);
                            Nodes[channel.Target.NodeIndex].Translate(translation);
                        }
                        else if (targetPath == AnimationChannelTargetPath.Scale)
                        {
                            Vector3 scale = new Vector3(data[0], data[1], data[2]);
                            Nodes[channel.Target.NodeIndex].Resize(scale);
                        }
                        else if (targetPath == AnimationChannelTargetPath.Texture)
                        {
                            Vector2 uv = new Vector2(data[0], data[1]);
                            var node = Nodes[channel.Target.NodeIndex];
                            if (node.HasMesh)
                            {
                                var mesh = node.Mesh;
                                //mesh.Primitives.
                            }

                        }
                    }

                    break;
                }
                case InterpolationAlgorithm.Linear:
                {
                    // For linear interpolation, use both prev and next indices
                    if (prevIndex >= 0 && nextIndex >= 0)
                    {
                        var prevTimeStamp = timeStamps[prevIndex];
                        var nextTimeStamp = timeStamps[nextIndex];
                        float t;

                        // Ensure t is within the range of 0 to 1
                        if (nextTimeStamp > prevTimeStamp)
                        {
                            t = (animationTime - prevTimeStamp) / (nextTimeStamp - prevTimeStamp);
                            t = MathHelper.Clamp(t, 0f, 1f); // Clamp to range from 0 to 1
                        }
                        else
                        {
                            t = 0f; // Case when prevTimeStamp == nextTimeStamp (rare but possible)
                        }

                        var prevData = new float[valuesPerElement];
                        var nextData = new float[valuesPerElement];
                        for (int i = 0; i < valuesPerElement; i++)
                        {
                            int offsetPrev = prevIndex * valuesPerElement;
                            int offsetNext = nextIndex * valuesPerElement;
                            prevData[i] = sampler.Output[offsetPrev + i];
                            nextData[i] = sampler.Output[offsetNext + i];
                        }

                        if (targetPath == AnimationChannelTargetPath.Rotation)
                        {
                            Quaternion prevRotation = new Quaternion(prevData[0], prevData[1], prevData[2], prevData[3]);
                            Quaternion nextRotation = new Quaternion(nextData[0], nextData[1], nextData[2], nextData[3]);
                            Quaternion rotation = Quaternion.Lerp(prevRotation, nextRotation, t);
                            Nodes[channel.Target.NodeIndex].Rotate(rotation);
                        }
                        else if (targetPath == AnimationChannelTargetPath.Translation)
                        {
                            Vector3 prevTranslation = new Vector3(prevData[0], prevData[1], prevData[2]);
                            Vector3 nextTranslation = new Vector3(nextData[0], nextData[1], nextData[2]);
                            Vector3 translation = Vector3.Lerp(prevTranslation, nextTranslation, t);
                            Nodes[channel.Target.NodeIndex].Translate(translation);
                        }
                        else if (targetPath == AnimationChannelTargetPath.Scale)
                        {
                            Vector3 prevScale = new Vector3(prevData[0], prevData[1], prevData[2]);
                            Vector3 nextScale = new Vector3(nextData[0], nextData[1], nextData[2]);
                            Vector3 scale = Vector3.Lerp(prevScale, nextScale, t);
                            Nodes[channel.Target.NodeIndex].Resize(scale);
                        }
                    }

                    break;
                }
                case InterpolationAlgorithm.Cubicspline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
            
    }

    public void Translate(Vector3 translation)
    {
        Translation += translation;
        UpdateTransform();
    }
        
    public void TranslateTo(Vector3 translation)
    {
        Translation = translation;
        UpdateTransform();
    }

    public void RotateBy(Quaternion rotation)
    {
        Rotation *= rotation;
        UpdateTransform();
    }
        
    public void RotateTo(Quaternion rotation)
    {
        Rotation = rotation;
        UpdateTransform();
    }
    
    public void ScaleBy(Vector3 scale)
    {
        Scale *= scale;
        UpdateTransform();
    }
    
    public void ScaleBy(float scale)
    {
        Scale *= scale;
        UpdateTransform();
    }
        
    public void ScaleTo(Vector3 scale)
    {
        Scale = scale;
        UpdateTransform();
    }
    
    public void ScaleTo(float scale)
    {
        ScaleTo(new Vector3(scale));
    }

    private void UpdateTransform()
    {
        LocalTranformation = Matrix.CreateScale(Scale) *
                             Matrix.CreateFromQuaternion(Rotation) *
                             Matrix.CreateTranslation(Translation);

        _boundingBoxDirty = true;
        
        foreach (var node in Nodes)
        {
            //node.UpdateGlobalTransform();
        }
    }

    //public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix world, Matrix view, Matrix projection)
    //{
    //    
    //}
        
    public static GameModel From(GraphicsDevice graphicsDevice, GLTFFile file)
    {
            
        var result = new GameModel();
            
        if (file.HasNodes)
        {
            result.Nodes = new GameNode[file.Nodes.Length];
            for (var i = 0; i < file.Nodes.Length; i++)
            {
                var node = GameNode.From(graphicsDevice, file, file.Nodes[i]);
                node.Model = result;
                result.Nodes[i] = node;
            }

            for (var i = 0; i < result.Nodes.Length; i++)
            {
                var node = result.Nodes[i];
                if (node.HasChildren)
                {
                    for (var j = 0; j < node.Children.Length; j++)
                    {
                        result.Nodes[node.Children[j]].Parent = node;
                    }
                }
            }
        }

        if (file.HasMeshes)
        {
            result.Meshes = new GameMesh[file.Meshes.Length];
            for (var i = 0; i < file.Meshes.Length; i++)
            {
                var mesh = GameMesh.From(graphicsDevice, file, file.Meshes[i]);
                result.Meshes[i] = mesh;
            }
        }
            
        if (file.HasAnimations)
        {
            result.Animations = new GameModelAnimation[file.Animations.Length];
            for (int i = 0; i < file.Animations.Length; i++)
            {
                var animation = file.Animations[i];
                result.Animations[i] = GameModelAnimation.From(graphicsDevice, file, animation);
            }
        }

        if (file.HasScenes)
        {
            result.Scenes = new GameScene[file.Scenes.Length];
            for (var i = 0; i < file.Scenes.Length; i++)
            {
                var scene = GameScene.From(graphicsDevice, file, file.Scenes[i]);
                for (var j = 0; j < scene.Nodes.Length; j++)
                {
                    result.Nodes[j].UpdateGlobalTransform();
                }
                result.Scenes[i] = scene;
            }
        }
            
        if (file.HasSkins)
        {
            result.Skins = new GameSkin[file.Skins.Length];
            for (var i = 0; i < file.Skins.Length; i++)
            {
                var skin = GameSkin.From(graphicsDevice, file, file.Skins[i]);
                skin.Model = result;
                result.Skins[i] = skin;
            }
        }
            
        //if (scene.HasNodes)
        //{
        //    result = new GameModel();
        //    result.Nodes = new GameNode[scene.Nodes.Length];
        //    for (int i = 0; i < scene.Nodes.Length; i++)
        //    {
        //        var node = scene.Nodes[i];
        //        var gameNode = GameNode.From(graphicsDevice, file, node);
        //        result.Nodes[i] = gameNode;
        //    }
//
        //    if (file.Animations != null)
        //    {
        //        result.Animations = new Dictionary<string, GameModelAnimation>();
        //        for (int i = 0; i < file.Animations.Length; i++)
        //        {
        //            var animation = file.Animations[i];
        //            result.Animations[Guid.NewGuid().ToString()] =
        //                GameModelAnimation.From(graphicsDevice, file, animation);
        //        }
        //    }
//
        //}
        return result;
    }

    public void Save(BinaryWriter writer)
    {
        // Nodes
        writer.Write(Nodes.Length);
        foreach (var node in Nodes)
        {
            node.Save(writer);
        }

        // Meshes
        writer.Write(Meshes.Length);
        foreach (var mesh in Meshes)
        {
            //mesh.Save(writer);
        }

        // Scenes
        writer.Write(Scenes.Length);
        foreach (var scene in Scenes)
        {
            scene.Save(writer);
        }

        // Skins
        writer.Write(Skins.Length);
        foreach (var skin in Skins)
        {
            //skin.Save(writer);
        }

        // Animations
        writer.Write(Animations.Length);
        foreach (var animation in Animations)
        {
            //animation.Save(writer);
        }
        
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
    }
}