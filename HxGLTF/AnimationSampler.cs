namespace HxGLTF
{
    public enum InterpolationAlgorithm
    {
        Linear,
        Step,
        Cubicspline
    }

    public class AnimationSampler
    {
        public Accessor Input; // time 
        public Accessor Output; // data
        public InterpolationAlgorithm Interpolation = InterpolationAlgorithm.Linear; // algorithm
    }
}