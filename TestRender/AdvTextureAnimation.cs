using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using PatteLib;

namespace TestRender;

public enum AnimationType: byte
{
    Texture,
    TextureCoords,
}

public enum AnimationStyle: byte
{
    LinearLoop,
    BounceLoop,
    StepLoop,
    Linear
}

public enum AnimationDirection
{
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public struct AnimationStep
{
    public Vector2 Offset;
    public Vector2 Scale;
    public int Duration;
}

public class AdvTextureAnimation
{
    private static readonly Dictionary<string, AnimationDirection> DirectionMap = new()
    {
        { "up", AnimationDirection.Up },
        { "down", AnimationDirection.Down },
        { "left", AnimationDirection.Left },
        { "right", AnimationDirection.Right },
        { "up-left", AnimationDirection.UpLeft },
        { "up-right", AnimationDirection.UpRight },
        { "down-left", AnimationDirection.DownLeft },
        { "down-right", AnimationDirection.DownRight },
    };
    
    public AnimationType Type;
    public AnimationStyle Style;
    public AnimationDirection Direction;
    public Texture2D[] Frames;
    public int Duration;
    public int Speed;
    public AnimationStep[] Steps;

    public Vector2 Offset;
    public TimeSpan AnimationTimer;
    public int CurrentIndex;

    public void Update(GameTime gameTime)
    {
        if (Type == AnimationType.Texture)
        {
            if (Style == AnimationStyle.LinearLoop)
            {
                AnimationTimer += gameTime.ElapsedGameTime;
                if (AnimationTimer <= TimeSpan.FromMilliseconds(Duration))
                {
                    return;
                }

                AnimationTimer -= TimeSpan.FromMilliseconds(Duration);
                CurrentIndex = (CurrentIndex + 1) % Frames.Length;
            }
        }
        else if (Type == AnimationType.TextureCoords)
        {
            if (Style == AnimationStyle.StepLoop)
            {
                var currentStep = Steps[CurrentIndex];
                AnimationTimer += gameTime.ElapsedGameTime;
                if (AnimationTimer <= TimeSpan.FromMilliseconds(currentStep.Duration))
                {
                    return;
                }
                AnimationTimer -= TimeSpan.FromMilliseconds(currentStep.Duration);
                CurrentIndex = (CurrentIndex + 1) % Steps.Length;
                Offset = Steps[CurrentIndex].Offset;
            }
            else if (Style == AnimationStyle.Linear)
            {
                //AnimationTimer += gameTime.ElapsedGameTime;

                float speed = 1000f / Speed;
                
                float deltaX = 0, deltaY = 0;

                switch (Direction)
                {
                    case AnimationDirection.Up: deltaY = speed; break;
                    case AnimationDirection.Down: deltaY = -speed; break;
                    case AnimationDirection.Left: deltaX = speed; break;
                    case AnimationDirection.Right: deltaX = -speed; break;
                    case AnimationDirection.UpLeft: deltaY = speed; deltaX = speed; break;
                    case AnimationDirection.UpRight: deltaY = speed; deltaX = -speed; break;
                    case AnimationDirection.DownLeft: deltaY = -speed; deltaX = speed; break;
                    case AnimationDirection.DownRight: deltaY = -speed; deltaX = -speed; break;
                }
                
                Offset.X += deltaX * (float)gameTime.ElapsedGameTime.TotalSeconds;;
                Offset.Y += deltaY * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                Offset.X = Utils.Wrap(Offset.X, 0f, 1f);
                Offset.Y = Utils.Wrap(Offset.Y, 0f, 1f);

            }
        }
    }
    
    public static AdvTextureAnimation Load(GraphicsDevice graphicsDevice, string path)
    {
        var result = new AdvTextureAnimation();
        JObject jAnimation = JObject.Parse(File.ReadAllText(path));
        
        string typeString = JsonUtils.GetValue<string>(jAnimation["type"]);
        result.Type = typeString switch
        {
            "texture" or "frame-by-frame" => AnimationType.Texture,
            "texture-coords" => AnimationType.TextureCoords,
            _ => throw new Exception()
        };

        string styleString = JsonUtils.GetValue<string>(jAnimation["style"]);
        result.Style = styleString switch
        {
            "linear-loop" => AnimationStyle.LinearLoop,
            "bounce-loop" => AnimationStyle.BounceLoop,
            "step-loop" => AnimationStyle.StepLoop,
            "linear" => AnimationStyle.Linear,
            _ => throw new Exception()
        };

        if (result.Type == AnimationType.Texture)
        {
            var frames = JsonUtils.GetValues<string>(jAnimation["frames"]);
            foreach (var framePath in frames)
            {
                Console.WriteLine(framePath);
                //ToDo: load textures
                //var texture = Texture2D.FromFile(graphicsDevice, framePath.ToString());
            }
        }
        else if (result.Type == AnimationType.TextureCoords)
        {
            if (result.Style == AnimationStyle.Linear)
            {
                int speedValue = JsonUtils.GetValue<int>(jAnimation["speed"]);
                result.Speed = speedValue;

                string directionString = JsonUtils.GetValue<string>(jAnimation["direction"]);
                
                if (DirectionMap.TryGetValue(directionString, out var animationDirection))
                {
                    result.Direction = animationDirection;
                }
                else
                {
                    throw new Exception();
                }
            }
            else if (result.Style == AnimationStyle.StepLoop)
            {
                JToken jSteps = jAnimation["steps"];
                var steps = JsonUtils.GetValues<JObject>(jSteps).Select(step => new AnimationStep
                {
                    Offset = new Vector2(
                        JsonUtils.GetValue<float>(step["offset"][0]),
                        JsonUtils.GetValue<float>(step["offset"][1])
                    ),
                    Scale = new Vector2(
                        JsonUtils.GetValue<float>(step["scale"][0]),
                        JsonUtils.GetValue<float>(step["scale"][1])
                    ),
                    Duration = JsonUtils.GetValue<int>(step["duration"])
                }).ToArray();
                result.Steps = steps;
            }
        } 
        return result;
    }
}