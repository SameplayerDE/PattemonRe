namespace PatteLib.Gameplay.Scripting.Commands;

public class ReturnCommand : ICommand
{
    public void Execute(ScriptProcessor processor)
    {
        processor.ReturnFromFunction();
    }
}