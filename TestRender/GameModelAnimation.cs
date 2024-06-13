namespace TestRender
{
    public class GameModelAnimationChannelTarget
    {
        public GameNode Node;
        public string Path;
    } 
    
    public class GameModelAnimationSampler
    {
    } 
    
    public class GameModelAnimationChannel
    {
        public GameModelAnimationSampler Sampler;
        public GameModelAnimationChannelTarget Target;
    } 
    
    public class GameModelAnimation
    {
        public GameModelAnimationChannel[] Channels;
        public GameModelAnimationSampler[] Samplers;
    } 
}