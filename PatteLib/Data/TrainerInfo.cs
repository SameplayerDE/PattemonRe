namespace PatteLib.Data;

/**
typedef struct TrainerInfo {
    charcode_t name[TRAINER_NAME_LEN + 1];
    u32 id;
    u32 money;
    u8 gender;
    u8 regionCode;
    u8 badgeMask;
    u8 appearance;
    u8 gameCode;
    u8 isMainStoryCleared : 1;
    u8 hasNationalDex : 1;
    u8 : 6;
} TrainerInfo;
**/

public class TrainerInfo
{
    private string _name;
    private uint _id;
    private uint _money;
    private bool _gender; // _gender ? male : female
    private int _badgeMask;
    
    public bool HasBadge(int badge)
    {
        return (_badgeMask & (1 << badge)) != 0;
    }

    public void SetBadge(int badge)
    {
        _badgeMask |= (1 << badge);
    }

    public int GetBadgeCount()
    {
        //  return System.Numerics.BitOperations.PopCount((uint)_badgeMask);
        int count = 0;
        for (int mask = _badgeMask; mask != 0; mask >>= 1)
        {
            if ((mask & 1) != 0)
            {
                count++;
            }
        }
        return count;
    }
}