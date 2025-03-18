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

    public static bool TryParse(string[] args, out ICommand? command)
    {
        // Extrahiere die Adresse und den Wert aus parts[1] und parts[2]
        string addressStr = args[0];
        string valueStr = args[1];

        // Konvertiere die hexadezimale Adresse in eine Dezimalzahl
        int compAddress = Convert.ToInt32(addressStr.StartsWith("0x") ? addressStr.Substring(2) : addressStr, 16);

        // Konvertiere den Wert in eine Dezimalzahl
        int compVal;
        if (!int.TryParse(valueStr, out compVal))
        {
            throw new ArgumentException("Invalid CompareVarValue command: value is not a valid integer.");
        }

        command = new CompareVarValueCommand(compAddress, compVal);
        return true;
    }
    
    public void Execute(ScriptProcessor processor)
    {
        processor.SetComparisonResult(processor.GetVariable(_address) == _value);
    }


}