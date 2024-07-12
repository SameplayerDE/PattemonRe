namespace PatteLib.Gameplay.Scripting.Commands;

public class IncrementVarCommand : ICommand
{
    private int _address;
    private int _value;

    public IncrementVarCommand(int address, int value)
    {
        _address = address;
        _value = value;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.SetVariable(_address, processor.GetVariable(_address) + _value);
        Console.WriteLine($"Increment {_address:X} by {_value}");
    }
}