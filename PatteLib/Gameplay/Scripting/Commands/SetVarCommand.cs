namespace PatteLib.Gameplay.Scripting.Commands;

public class SetVarCommand : ICommand
{
    private int _address;
    private int _value;

    public SetVarCommand(int address, int value)
    {
        _address = address;
        _value = value;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.SetVariable(_address, _value);
        Console.WriteLine($"SetVar {_address:X} to {_value}");
    }
}