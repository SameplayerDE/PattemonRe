namespace PatteLib.Gameplay;

public class EventContainer
{
    public int Id;
    public List<Spawnable> Spawnables = [];
    public List<Overworld> Overworlds = [];
    public List<Trigger> Triggers = [];
    public List<Warp> Warps = [];
}