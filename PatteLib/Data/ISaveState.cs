namespace PatteLib.Data;

public interface ISaveState
{
    public void Save(string path);
    public void Load(string path);
}