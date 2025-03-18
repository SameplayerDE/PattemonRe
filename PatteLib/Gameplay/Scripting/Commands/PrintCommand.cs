namespace PatteLib.Gameplay.Scripting.Commands;

public class PrintCommand : ICommand
{
    private string _value;

    public PrintCommand(string value)
    {
        _value = value;
    }

    public static bool TryParse(string[] args, out ICommand? command)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException("Invalid Print command");
        }
        command = new PrintCommand(args[0]);
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        // Überprüfen, ob _value ein hexadezimaler Wert ist
        bool isHex = _value.StartsWith("0x", StringComparison.OrdinalIgnoreCase);

        // Falls _value hexadezimal ist, konvertiere es zu einer Dezimalzahl
        int address;
        if (isHex)
        {
            address = Convert.ToInt32(_value.Substring(2), 16);
        }
        else
        {
            Console.WriteLine(_value);
            return;
        }

        // Den Wert der Adresse aus der ScriptProcessor Klasse abrufen
        int value = processor.GetVariable(address);

        // Ausgabe des Werts
        Console.WriteLine($"Value at address {_value}: {value}");
    }
}