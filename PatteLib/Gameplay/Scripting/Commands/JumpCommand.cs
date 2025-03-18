namespace PatteLib.Gameplay.Scripting.Commands;

public class JumpCommand : ICommand
{
    private string _label;

    public JumpCommand(string label)
    {
        _label = label;
    }

    public static bool TryParse(string[] args, out ICommand? command)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException("Invalid Jump command");
        }
        command = new JumpCommand(args[0]);
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.JumpToSection(_label);
    }
}