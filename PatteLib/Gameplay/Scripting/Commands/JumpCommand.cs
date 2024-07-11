namespace PatteLib.Gameplay.Scripting.Commands;

public class JumpCommand : ICommand
{
    private string _label;

    public JumpCommand(string label)
    {
        _label = label;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.JumpToSection(_label);
    }
}