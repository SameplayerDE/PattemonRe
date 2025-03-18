namespace PatteLib.Gameplay.Scripting.Commands;

public class EndCommand : ICommand
{
    public static bool TryParse(string[] args, out ICommand? command)
    {
        command = new EndCommand();
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.EndCurrentSection();
    }
}