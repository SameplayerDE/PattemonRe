using PatteLib.Data;
using PatteLib.World;

namespace PatteLib.Gameplay.BattlePlay;

public static class BattleFactory
{
    public static Battle Create(ChunkHeader header, BattleData data)
    {
        Battle result = new Battle();

        result.Data = data;
        
        return result;
    }
    
    public static Battle CreateFromData(BattleData data)
    {
        Battle result = new Battle();

        result.Data = data;
        
        return result;
    }
}