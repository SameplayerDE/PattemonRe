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

    public static bool TryParse(string[] args, out ICommand? command)
    {
        command = null;
        if (args.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(args[0], System.Globalization.NumberStyles.HexNumber, null, out int address))
        {
            return false;
        }

        if (!int.TryParse(args[1], out int value))
        {
            return false;
        }
        
        command = new SetVarCommand(address, value);
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.SetVariable(_address, _value);
        Console.WriteLine($"SetVar {_address:X} to {_value}");
    }
}