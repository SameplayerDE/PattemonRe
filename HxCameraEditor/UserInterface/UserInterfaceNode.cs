namespace HxCameraEditor.UserInterface;

public enum UserInterfaceNodeType
{
    VStack,
    HStack,
    ZStack,
    ScrollView,
    Spacer,
    Label,
    Button,
    ToggleButton,
    RadioButton,
    Image,
    TextField,
    Slider
}

public class UserInterfaceNode
{
    public UserInterfaceNodeType Type;
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public bool IsClickable = false;
    public bool IsVisible = true;
    
    protected UserInterfaceNode(UserInterfaceNodeType type)
    {
        X = 0;
        Y = 0;
        Width = 0;
        Height = 0;
        Type = type;
    }
}