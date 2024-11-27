using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PatteLib.Graphics;

public static class TextureAnimationLinker
{
    public static Dictionary<(string[] Materials, AnimationCompareFunction CompareFunction), TextureAnimation> LoadAnimations(GraphicsDevice graphicsDevice, string path)
    {
        var animations = new Dictionary<(string[] Materials, AnimationCompareFunction CompareFunction), TextureAnimation>();

        JArray jAnimations = JArray.Parse(File.ReadAllText(path));

        foreach (var jAnimation in jAnimations)
        {
            string compareString = JsonUtils.GetValue<string>(jAnimation["compare"]);
            AnimationCompareFunction compareFunction = compareString switch
            {
                "equals" => AnimationCompareFunction.Equals,
                "contains" => AnimationCompareFunction.Contains,
                "startsWith" => AnimationCompareFunction.StartsWith,
                _ => throw new Exception()
            };
            
            IEnumerable<string> materialsEnumerable = JsonUtils.GetValues<string>(jAnimation["materials"]);
            string[] materials = new string[Enumerable.Count(materialsEnumerable)];
            int index = 0;
            foreach (var material in materialsEnumerable)
            {
                materials[index++] = material;
            }
            string pathString = JsonUtils.GetValue<string>(jAnimation["animation"]);
            string combinedPath = Path.IsPathRooted(pathString) ? pathString : Path.Combine(Path.GetDirectoryName(path), pathString);
            TextureAnimation animation = TextureAnimation.Load(graphicsDevice, combinedPath);

            animations.Add((materials, compareFunction), animation);
        }
        return animations;
    }
}