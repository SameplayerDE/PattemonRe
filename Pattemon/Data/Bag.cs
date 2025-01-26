using System.Collections.Generic;

namespace Pattemon.Data;

public struct BagItem
{
    public ushort Id;
    public ushort Quantity;
}

public class BagPocket
{
    private List<BagItem> _content;
 
    public readonly int Capacity;
    public bool IsEmpty => _content.Count == 0;
    
    public BagPocket(int maxCapacity)
    {
        Capacity = maxCapacity;
        _content = new List<BagItem>(maxCapacity);
    }
    
    /*static BagItem *Pocket_FindSlotWithItemQuantity(BagItem *pocket, u32 pocketSize, u16 item, u16 count)
    {
        for (u32 i = 0; i < pocketSize; i++) {
            if (pocket[i].item == item) {
                return pocket[i].quantity >= count ? &pocket[i] : NULL;
            }
        }

        return NULL;
    }*/
    
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
    public const uint SlotInvalid = uint.MaxValue;
    public const int MaxQuantityItem = 999;
    public const int MaxQuantityTMHM = 99;

    public Dictionary<int, BagPocket> Pockets = new()
    {
        { Global.Items.PocketItems, new BagPocket(ItemPocketSize) },
        { Global.Items.PocketMedicine, new BagPocket(MedicinePocketSize) },
        { Global.Items.PocketBalls, new BagPocket(PokeballPocketSize) },
        { Global.Items.PocketTMHMs, new BagPocket(TMHMPocketSize) },
        { Global.Items.PocketBerries, new BagPocket(BerryPocketSize) },
        { Global.Items.PocketMail, new BagPocket(MailPocketSize) },
        { Global.Items.PocketBattleItems, new BagPocket(BattleItemPocketSize) },
        { Global.Items.PocketKeyItems, new BagPocket(KeyItemPocketSize) },
    };
    
    // public BagPocket ItemPocket = new BagPocket(ItemPocketSize);
    // public BagPocket MedicinePocket = new BagPocket(MedicinePocketSize);
    // public BagPocket BallPocket = new BagPocket(PokeballPocketSize);
    // public BagPocket ActionPocket = new BagPocket(TMHMPocketSize);
    // public BagPocket BerryPocket = new BagPocket(BerryPocketSize);
    // public BagPocket LetterPocket = new BagPocket(MailPocketSize);
    // public BagPocket BattleItemPocket = new BagPocket(BattleItemPocketSize);
    // public BagPocket KeyItemPocket = new BagPocket(KeyItemPocketSize);

    /*public uint GetPocketForItem(ushort itemId, BagItem **outPocket, u32 *outMax, enum HeapId heapID)
    {
        u32 pocket = Item_LoadParam(item, ITEM_PARAM_FIELD_POCKET, heapID);

        switch (pocket) {
            case POCKET_KEY_ITEMS:
                *outPocket = bag->keyItems;
                *outMax = KEY_ITEM_POCKET_SIZE;
                break;
            case POCKET_ITEMS:
                *outPocket = bag->items;
                *outMax = ITEM_POCKET_SIZE;
                break;
            case POCKET_BERRIES:
                *outPocket = bag->berries;
                *outMax = BERRY_POCKET_SIZE;
                break;
            case POCKET_MEDICINE:
                *outPocket = bag->medicine;
                *outMax = MEDICINE_POCKET_SIZE;
                break;
            case POCKET_BALLS:
                *outPocket = bag->pokeballs;
                *outMax = POKEBALL_POCKET_SIZE;
                break;
            case POCKET_BATTLE_ITEMS:
                *outPocket = bag->battleItems;
                *outMax = BATTLE_ITEM_POCKET_SIZE;
                break;
            case POCKET_MAIL:
                *outPocket = bag->mail;
                *outMax = MAIL_POCKET_SIZE;
                break;
            case POCKET_TMHMS:
                *outPocket = bag->tmHms;
                *outMax = TMHM_POCKET_SIZE;
                break;
        }

        return pocket;
    }*/
    
    public bool HasItemsInPocket(int pocketID)
    {
        if (Pockets.TryGetValue(pocketID, out var pocket))
        {
            return !pocket.IsEmpty;
        }
        return false;
    }
} 