using System.Text.RegularExpressions;
using PatteLib.Gameplay.Scripting.Commands;

namespace PatteLib.Gameplay.Scripting;

public class CommandFactory
{
    private static readonly Dictionary<string, Func<string[], ICommand>> _commandRegistry = new();

    static CommandFactory()
    {
        RegisterCommand("Call", args =>
        {
            if (CallCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("CallIf", args =>
        {
            if (CallIfCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("CompareVarValue", args =>
        {
            if (CompareVarValueCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("End", args =>
        {
            if (EndCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("IncrementVar", args =>
        {
            if (IncrementVarCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("Print", args =>
        {
            if (PrintCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("Return", args =>
        {
            if (ReturnCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("SetVar", args =>
        {
            if (SetVarCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("Jump", args =>
        {
            if (JumpCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
        RegisterCommand("JumpIf", args =>
        {
            if (JumpIfCommand.TryParse(args, out ICommand? command))
            {
                return command!;
            }
            throw new Exception();
        });
    }
    
    public static void RegisterCommand(string name, Func<string[], ICommand> parser)
    {
        _commandRegistry[name] = parser;
    }
    
    public static bool TryCreateCommand(string line, out ICommand? command)
    {
        command = null;
        string[] parts = Regex.Replace(line, @"\t|\n|\r", "").Split(' ');
        string commandName = parts[0];
        string[] args = parts.Skip(1).ToArray();
        
        if (_commandRegistry.TryGetValue(commandName, out var parser))
        {
            return parser(args) is { } cmd && (command = cmd) != null;
        }
        throw new ArgumentException("Unkown Command");
    }
}