namespace PatteLib.Gameplay;

public class Overworld : EventEntity
{
    public int Id; // id for scripts
    public int Flag; // flag 
    public int Script; // script to trigger
    public int EntryId; // sprites and animations
    public OverworldType Type;
    public Orientation Orientation;
    public MovementPattern MovementPattern;
    public int MovementRangeX;
    public int MovementRangeY;
}