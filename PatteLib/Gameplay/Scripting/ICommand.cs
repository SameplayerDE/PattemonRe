namespace PatteLib.Gameplay.Scripting;

public interface ICommand
{
    //static abstract bool TryParse(string[] args, out ICommand? command);
    void Execute(ScriptProcessor processor);
}