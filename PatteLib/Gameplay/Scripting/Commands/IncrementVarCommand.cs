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

    public static bool TryParse(string[] args, out ICommand? command)
    {
        // Extrahiere die Adresse und den Wert aus parts[1] und parts[2]
        string addressStr = args[0];
        string valueStr = args[1];

        // Konvertiere die hexadezimale Adresse in eine Dezimalzahl
        int compAddress = Convert.ToInt32(addressStr.StartsWith("0x") ? addressStr.Substring(2) : addressStr, 16);

        // Konvertiere den Wert in eine Dezimalzahl
        int incVal;
        if (!int.TryParse(valueStr, out incVal))
        {
            throw new ArgumentException("Invalid CompareVarValue command: value is not a valid integer.");
        }

        command = new IncrementVarCommand(compAddress, incVal);
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.SetVariable(_address, processor.GetVariable(_address) + _value);
        Console.WriteLine($"Increment {_address:X} by {_value}");
    }
}