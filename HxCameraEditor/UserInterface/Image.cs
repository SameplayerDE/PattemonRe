namespace HxCameraEditor.UserInterface;

public class Image : UserInterfaceNode
{
    public string Path;
    public float Scale;
    
    public Image(string path, float scale = 1f) : base(UserInterfaceNodeType.Image)
    {
        Path = path;
        Scale = scale;
    }
}