namespace PatteLib.Gameplay;

public class PokemonMove
{
    public PokemonMoveType Type;
    public PokemonElementType ElementType;
    public PokemonMoveCategory CategoryType;
    public byte Points;
    public byte Power;
    public byte Accuracy; // 0 - 100
    
    public PokemonMove(PokemonMoveType type, PokemonElementType elementType, PokemonMoveCategory categoryType, byte points, byte power, byte accuracy)
    {
        Type = type;
        ElementType = elementType;
        CategoryType = categoryType;
        Points = points;
        Power = power;
        Accuracy = accuracy;
    }
    
    private static readonly Dictionary<int, PokemonMove> PokemonMoves = new Dictionary<int, PokemonMove>
    {
        { 0x01, new PokemonMove(PokemonMoveType.Pound, PokemonElementType.Normal, PokemonMoveCategory.Physical, 35, 40, 100) }
    };
    
    public static PokemonMove GetById(int id)
    {
        if (PokemonMoves.TryGetValue(id, out var pokemonMove))
        {
            return pokemonMove;
        }
        throw new KeyNotFoundException($"PokemonMove with ID {id} not found.");
    }
}