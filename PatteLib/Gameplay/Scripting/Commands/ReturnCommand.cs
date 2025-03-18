namespace PatteLib.Gameplay.Scripting.Commands;

public class ReturnCommand : ICommand
{
    public static bool TryParse(string[] args, out ICommand? command)
    {
        command = new ReturnCommand();
        return true;
    }

    public void Execute(ScriptProcessor processor)
    {
        processor.ReturnFromFunction();
    }
}