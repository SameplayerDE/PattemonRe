using PatteLib.Gameplay;

namespace PatteLib.Data;

public class Pokemon
{
    public int Species;
    public int Status;
    public int Mail;
    public int HealthPoints;
    public int MaxHealthPoints;
    public int Attack;
    public int Defense;
    public int Speed;
    public int SpecialAttack;
    public int SpecialDefense;
    public int TotalExperience;
    public int Weight;
    
    public PokemonElementType Type0;
    public PokemonElementType Type1;

    public bool IsShiny;
    public bool HasNickname;

    public int GetLevel()
    {
        return GetSpeciesLevelAt(Species, TotalExperience);
    }
    
    public static int GetSpeciesLevelAt(int species, int experience)
    {
        return 1;
    }
}