namespace Pattemon.Scenes.WorldMap;

public record WorldMapMatrixEntry
{
    public static WorldMapMatrixEntry Empty => new()
    {
        Name = string.Empty,
        Description = string.Empty,
    };
    
    public string Name;
    public string Description;
}