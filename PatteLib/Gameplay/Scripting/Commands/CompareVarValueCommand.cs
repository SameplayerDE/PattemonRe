namespace PatteLib.Gameplay.Scripting.Commands;

public class CompareVarValueCommand : ICommand
{
    private int _address;
    private int _value;

    public CompareVarValueCommand(int address, int value)
    {
        _address = address;
        _value = value;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.SetComparisonResult(processor.GetVariable(_address) == _value);
    }
}