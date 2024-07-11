using System.Text.RegularExpressions;
using PatteLib.Gameplay.Scripting.Commands;

namespace PatteLib.Gameplay.Scripting;

public class CommandFactory
{
    public static ICommand CreateCommand(string line)
    {
        string[] parts = Regex.Replace(line, @"\t|\n|\r", "").Split(' ');

        switch (parts[0])
        {
            case "SetVar":
                if (parts.Length == 3 && int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int address) && int.TryParse(parts[2], out int val))
                {
                    return new SetVarCommand(address, val);
                }
                throw new ArgumentException("Invalid SetVar command");
            case "CompareVarValue":
                if (parts.Length == 3)
                {
                    // Extrahiere die Adresse und den Wert aus parts[1] und parts[2]
                    string addressStr = parts[1];
                    string valueStr = parts[2];

                    // Konvertiere die hexadezimale Adresse in eine Dezimalzahl
                    int compAddress = Convert.ToInt32(addressStr.StartsWith("0x") ? addressStr.Substring(2) : addressStr, 16);

                    // Konvertiere den Wert in eine Dezimalzahl
                    int compVal;
                    if (!int.TryParse(valueStr, out compVal))
                    {
                        throw new ArgumentException("Invalid CompareVarValue command: value is not a valid integer.");
                    }

                    return new CompareVarValueCommand(compAddress, compVal);
                }
                throw new ArgumentException("Invalid CompareVarValue command: not enough arguments.");
            case "CallIf":
                if (parts.Length == 3)
                {
                    return new CallIfCommand(parts[1], parts[2]);
                }
                throw new ArgumentException("Invalid CallIf command");
            case "JumpIf":
                if (parts.Length == 3)
                {
                    return new JumpIfCommand(parts[1], parts[2]);
                }
                throw new ArgumentException("Invalid JumpIf command");
            case "End":
                return new EndCommand();
            case "Return":
                return new ReturnCommand();
            case "Jump":
                if (parts.Length == 2)
                {
                    return new JumpCommand(parts[1]);
                }
                throw new ArgumentException("Invalid Jump command");
            case "Print":
                if (parts.Length == 2)
                {
                    return new PrintCommand(parts[1]);
                }
                throw new ArgumentException("Invalid Print command");
            default:
                throw new ArgumentException($"Unknown command: {parts[0]}");
        }
    }
}