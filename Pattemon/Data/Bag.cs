using Pattemon.Global;

namespace Pattemon.Data;

public class BagItem
{
    public ushort Item;
    public ushort Quantity;
}

public class Bag
{
    public const int ItemPocketSize = 165;
    public const int KeyItemPocketSize = 50;
    public const int TMHMPocketSize = 100;
    public const int MailPocketSize = 12;
    public const int MedicinePocketSize = 40;
    public const int BerryPocketSize = 64;
    public const int PokeballPocketSize = 15;
    public const int BattleItemPocketSize = 30;
    
    public BagItem[] Items { get; set; } = new BagItem[ItemPocketSize];
    public BagItem[] KeyItems { get; set; } = new BagItem[KeyItemPocketSize];
    public BagItem[] TMHMs { get; set; } = new BagItem[TMHMPocketSize];
    public BagItem[] Mail { get; set; } = new BagItem[MailPocketSize];
    public BagItem[] Medicine { get; set; } = new BagItem[MedicinePocketSize];
    public BagItem[] Berries { get; set; } = new BagItem[BerryPocketSize];
    public BagItem[] Pokeballs { get; set; } = new BagItem[PokeballPocketSize];
    public BagItem[] BattleItems { get; set; } = new BagItem[BattleItemPocketSize];

    public uint RegisteredItem { get; set; } // Entspricht `u32`
}

public class FieldBagCursor
{
    public byte[] Scroll { get; set; } = new byte[Item.PocketMax];
    public byte[] Index { get; set; } = new byte[Item.PocketMax];
    public ushort Pocket { get; set; }
    public ushort Dummy12 { get; set; }
}

public class BattleBagCursor
{
    public byte[] Scroll { get; set; } = new byte[(int)(Item.BattleItemCategory.Max + 1)];
    public byte[] Index { get; set; } = new byte[(int)(Item.BattleItemCategory.Max + 1)];
    public ushort LastUsedItemID { get; set; }
    public ushort LastUsedCategory { get; set; }
    public ushort CurrentCategory { get; set; }
}

public class BagCursor
{
    public FieldBagCursor Field { get; set; }
    public BattleBagCursor Battle { get; set; }
}