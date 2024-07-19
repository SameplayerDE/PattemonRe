namespace PatteLib.Gameplay;

public class OverWorld : EventEntity
{
    public int Id; // id for scripts
    public int Flag; // flag 
    public int Script; // script to trigger
    public int EntryId; // sprites and animations
    public bool Is3D;
    public OverWorldType Type;
    public Orientation Orientation;
    public MovementPattern MovementPattern;
    public int MovementRangeX;
    public int MovementRangeY;
}