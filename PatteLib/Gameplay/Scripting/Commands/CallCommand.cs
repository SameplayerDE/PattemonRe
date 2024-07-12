namespace PatteLib.Gameplay.Scripting.Commands;

public class CallCommand : ICommand
{
    private string _label;

    public CallCommand(string label)
    {
        _label = label;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.CallSection(_label);
    }
}