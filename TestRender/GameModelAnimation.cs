using System.Dynamic;
using HxGLTF;
using Microsoft.Xna.Framework.Graphics;

namespace TestRender
{
    public class GameModelAnimationChannelTarget
    {
        public int NodeIndex;
        public string Path;
        
        public static GameModelAnimationChannelTarget From(GraphicsDevice graphicsDevice, GLTFFile file, Animation animation, AnimationChannelTarget target)
        {
            var result = new GameModelAnimationChannelTarget();
            result.NodeIndex = target.Node.Index;
            result.Path = target.Path;
            return result;
        }
    } 
    
    public class GameModelAnimationSampler
    {

        public float[] Input;
        public InterpolationAlgorithm InterpolationAlgorithm;
        public float[] Output;
        
        public static GameModelAnimationSampler From(GraphicsDevice graphicsDevice, GLTFFile file, Animation animation, AnimationSampler sampler)
        {
            var result = new GameModelAnimationSampler();

            result.Input = AccessorReader.ReadData(sampler.Input);
            result.Output = AccessorReader.ReadData(sampler.Output);
            result.InterpolationAlgorithm = sampler.Interpolation;
            
            return result;
        }
    }
    
    public class GameModelAnimationChannel
    {
        public int SamplerIndex;
        public GameModelAnimationChannelTarget Target;
        
        public static GameModelAnimationChannel From(GraphicsDevice graphicsDevice, GLTFFile file, Animation animation, AnimationChannel channel)
        {
            var result = new GameModelAnimationChannel();
            result.SamplerIndex = channel.Sampler.Index;
            result.Target = GameModelAnimationChannelTarget.From(graphicsDevice, file, animation, channel.Target);
            return result;
        }
    } 
    
    public class GameModelAnimation
    {
        public GameModelAnimationChannel[] Channels;
        public GameModelAnimationSampler[] Samplers;

        public static GameModelAnimation From(GraphicsDevice graphicsDevice, GLTFFile file, Animation animation)
        {
            var result = new GameModelAnimation();
            
            result.Channels = new GameModelAnimationChannel[animation.Channels.Length];
            for (int i = 0; i < result.Channels.Length; i++)
            {
                var animationChannel = animation.Channels[i];
                result.Channels[i] = GameModelAnimationChannel.From(graphicsDevice, file, animation, animationChannel);
            }
            
            result.Samplers =  new GameModelAnimationSampler[animation.Samplers.Length];
            for (int i = 0; i < result.Samplers.Length; i++)
            {
                var animationSampler = animation.Samplers[i];
                result.Samplers[i] = GameModelAnimationSampler.From(graphicsDevice, file, animation, animationSampler);
            }
            
           
            return result;
        }
    } 
}