namespace HxCameraEditor.UserInterface;

public class Slider : UserInterfaceNode
{
    public int Value;
    public int Max;
    public int Min;
    public UserInterfaceNode? ToolTip;

    public Slider(int min, int max, int value, UserInterfaceNodeType type) : base(type)
    {
        Min = min;
        Max = max;
        Value = value;
    }
}