using InputLib;
using PatteLib.Gameplay.Scripting;

namespace Pattemon.Gameplay.Scripting.Commands;

public class ReleaseAllCommand : ICommand
{
    public void Execute(ScriptProcessor processor)
    {
        InputHandler.Release();
    }
}