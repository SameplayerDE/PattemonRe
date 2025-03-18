namespace PatteLib.Gameplay.Scripting.Commands;

public class CallIfCommand : ICommand
{
    private string _condition;
    private string _label;

    public CallIfCommand(string condition, string label)
    {
        _condition = condition;
        _label = label;
    }
    
    public static bool TryParse(string[] args, out ICommand? command)
    {
        if (args.Length == 2)
        {
            command = new CallIfCommand(args[0], args[1]);
            return true;
        }
        throw new ArgumentException("Invalid CallIf command");
    }
    
    public void Execute(ScriptProcessor processor)
    {
        if ((_condition == "EQUAL" && processor.GetComparisonResult()) ||
            (_condition == "NOT_EQUAL" && !processor.GetComparisonResult()))
        {
            processor.CallSection(_label);
        }
    }
}