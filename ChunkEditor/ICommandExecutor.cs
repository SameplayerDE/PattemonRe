namespace ChunkEditor;

public interface ICommandExecutor
{
    bool OnCommand(Command command);
}