using Microsoft.Xna.Framework;

namespace HxCameraEditor.UserInterface.Models;

public class Message
{
    public string Text;
    public Color BackgroundColor;
    public Color MessageColor;
    public float LifeTimeInSeconds = 3;
    public float AnimationProgress = 0f;
}