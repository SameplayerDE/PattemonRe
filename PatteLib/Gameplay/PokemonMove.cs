namespace PatteLib.Gameplay;

public class PokemonMove
{
    public PokemonMoveType Type;
    public PokemonElementType ElementType;
    public PokemonMoveCategory CategoryType;
    public byte Points;
    public byte Power;
    public byte Accuracy; // 0 - 100
}