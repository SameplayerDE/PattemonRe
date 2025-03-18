using InputLib;
using PatteLib.Gameplay.Scripting;

namespace Pattemon.Gameplay.Scripting.Commands;

public class LockAllCommand : ICommand
{
    public void Execute(ScriptProcessor processor)
    {
        InputHandler.Lock();
    }
}