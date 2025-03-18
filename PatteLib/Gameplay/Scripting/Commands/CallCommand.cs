namespace PatteLib.Gameplay.Scripting.Commands;

public class CallCommand : ICommand
{
    private string _label;

    public CallCommand(string label)
    {
        _label = label;
    }
    
    public static bool TryParse(string[] args, out ICommand? command)
    {
        if (args.Length == 1)
        {
            command = new CallCommand(args[0]);
            return true;
        }
        throw new ArgumentException("Invalid Call command");
    }
    
    public void Execute(ScriptProcessor processor)
    {
        processor.CallSection(_label);
    }
}