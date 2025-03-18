namespace PatteLib.Gameplay.Scripting.Commands;

public class JumpIfCommand : ICommand
{
    private string _condition;
    private string _label;

    public JumpIfCommand(string condition, string label)
    {
        _condition = condition;
        _label = label;
    }

    public static bool TryParse(string[] args, out ICommand? command)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException("Invalid JumpIf command");
        }
        command = new JumpIfCommand(args[0], args[1]);
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        if ((_condition == "EQUAL" && processor.GetComparisonResult()) ||
            (_condition == "NOT_EQUAL" && !processor.GetComparisonResult()))
        {
            processor.JumpToSection(_label);
        }
    }
}