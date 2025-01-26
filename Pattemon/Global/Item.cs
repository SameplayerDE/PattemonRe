namespace Pattemon.Global;

public static class Item
{
    public enum BattleItemCategory : int
    {
        RecoverHP = 0,
        RecoverStatus,
        PokeBalls,
        BattleItems,
        
        Max
    }
    
    public enum ItemType
    {
        FullRestore = 0,
        RecoverHP,
        RecoverStatus,
        StatBooster,
        GuardSpec,

        Max
    }
    
    public const int PocketItems = 0;
    public const int PocketMedicine = 1;
    public const int PocketBalls = 2;
    public const int PocketTMHMs = 3;
    public const int PocketBerries = 4;
    public const int PocketMail = 5;
    public const int PocketBattleItems = 6;
    public const int PocketKeyItems = 7;
    public const int PocketMax = 8;
    
    public const int ItemRecoverConfusion = 1 << 0;
    public const int ItemRecoverParalysis = 1 << 1;
    public const int ItemRecoverFreeze = 1 << 2;
    public const int ItemRecoverBurn = 1 << 3;
    public const int ItemRecoverPoison = 1 << 4;
    public const int ItemRecoverSleep = 1 << 5;
    
    public const int ItemRecoverFull = ItemRecoverSleep
                                       | ItemRecoverPoison
                                       | ItemRecoverBurn
                                       | ItemRecoverFreeze
                                       | ItemRecoverParalysis;
    
}