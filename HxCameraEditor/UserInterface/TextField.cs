namespace HxCameraEditor.UserInterface;

public enum TextFieldFormat
{
    Text,
    Password,
    Number
}

public class TextField
{
    public TextFieldFormat Format;
    public string Value;
}