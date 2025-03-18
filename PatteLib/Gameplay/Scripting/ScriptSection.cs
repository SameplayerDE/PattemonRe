namespace PatteLib.Gameplay.Scripting;

public record ScriptSection(List<ICommand> Commands)
{
    public readonly List<ICommand> Commands = Commands;
}