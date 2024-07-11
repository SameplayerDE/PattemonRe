namespace PatteLib.Gameplay.Scripting.Commands;

public class EndCommand : ICommand
{
    public void Execute(ScriptProcessor processor)
    {
        processor.EndCurrentSection();
    }
}