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
    public float Duration;
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
                    Duration = JsonUtils.GetValue<float>(step["duration"])
                }).ToArray();
                result.Steps = steps;
            }
        } 
        return result;
    }
}