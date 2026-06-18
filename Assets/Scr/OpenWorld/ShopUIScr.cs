using TMPro;
using UnityEngine;

public class ShopUIScr : MonoBehaviour
{    private int Trade
    {
        get
        {
            if (npcZoneShop == null)
                return 0;

            return npcZoneShop.trade;
        }

        set
        {
            if (npcZoneShop == null)
                return;

            npcZoneShop.trade = value;
        }
    }
    public TextMeshProUGUI traidText;

    [Header("Refs")]
    public NPCZoneScr npcZoneShop;
    public NPCZoneUIScr npcZoneScr;
    public InventoryScr inventoryScr;
    public BattleDataCarrier dataCarrier;

    [Header("Shop")]
    public Shop shop;

    [Header("Slots")]
    public SlotShopUIScr[] slotShop;
    public SlotShopUIScr[] slotPlayer;

    [Header("Selected")]
    public int selectItem = -1;
    public SlotUIScr selectSlotSlot;



    private void InitShopSlots()
    {
        if (shop.itemsShop == null)
        {
            shop.itemsShop = new System.Collections.Generic.List<int>();
        }

        while (shop.itemsShop.Count < slotShop.Length)
        {
            shop.itemsShop.Add(-1);
        }
    }

    // ─────────────────────────────────────────
    // PLAYER
    // ─────────────────────────────────────────

    public void PlayerItem(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= inventoryScr.sharedInventory.inventory.Length)
            return;

        int playerItem =
            inventoryScr.sharedInventory.inventory[playerIndex];

        // НЕ МОЖНА БРАТИ ПУСТИЙ СЛОТ
        if (playerItem == -1 &&
            selectItem == -1)
        {
            return;
        }

        int temp = selectItem;

        selectItem = playerItem;

        inventoryScr.sharedInventory.inventory[playerIndex] = temp;

        SlotUpdate();
    }

    public void SelectItem(int playerIndex)
    {
        bool fast =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (fast)
        {
            FastSellButton(playerIndex);
            return;
        }

        PlayerItem(playerIndex);
    }

    // ─────────────────────────────────────────
    // BUY / SELL
    // ─────────────────────────────────────────

    public void ShopButton(int shopIndex)
    {
        if (shopIndex < 0 || shopIndex >= shop.itemsShop.Count)
            return;

        bool fast =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (fast)
        {
            FastBuyButton(shopIndex);
            return;
        }

        int shopItem = shop.itemsShop[shopIndex];

        // Пустий слот магазину
        if (shopItem == -1)
        {
            if (selectItem == -1)
                return;

            ITradeItem tradeSell =
                dataCarrier.allItems[selectItem] as ITradeItem;

            if (tradeSell != null)
            {
                Trade += tradeSell.Level;

                shop.itemsShop[shopIndex] = selectItem;

                selectItem = -1;
            }

            SlotUpdate();
            return;
        }

        ITradeItem tradeBuy =
            dataCarrier.allItems[shopItem] as ITradeItem;

        // Купівля
        if (selectItem == -1)
        {
            if (tradeBuy != null &&
                Trade >= tradeBuy.Level)
            {
                selectItem = shopItem;

                Trade -= tradeBuy.Level;

                shop.itemsShop[shopIndex] = -1;
            }

            SlotUpdate();
            return;
        }

        // Обмін
        ITradeItem tradeSelected =
            dataCarrier.allItems[selectItem] as ITradeItem;

        if (tradeSelected == null || tradeBuy == null)
        {
            SlotUpdate();
            return;
        }

        int diff = tradeSelected.Level - tradeBuy.Level;

        if (diff == 0)
        {
            Swap(shopIndex);
        }
        else if (diff > 0)
        {
            Swap(shopIndex);
            Trade += diff;
        }
        else if (Trade >= -diff)
        {
            Swap(shopIndex);
            Trade += diff;
        }

        SlotUpdate();
    }

    public void FastSellButton(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= inventoryScr.sharedInventory.inventory.Length)
            return;

        int item =
            inventoryScr.sharedInventory.inventory[playerIndex];

        if (item == -1)
            return;

        ITradeItem tradeItem =
            dataCarrier.allItems[item] as ITradeItem;

        if (tradeItem == null)
            return;

        for (int i = 0; i < shop.itemsShop.Count; i++)
        {
            if (shop.itemsShop[i] == -1)
            {
                shop.itemsShop[i] = item;

                inventoryScr.sharedInventory.inventory[playerIndex] = -1;

                Trade += tradeItem.Level;

                break;
            }
        }

        SlotUpdate();
    }

    public void FastBuyButton(int shopIndex)
    {
        if (shopIndex < 0 || shopIndex >= shop.itemsShop.Count)
            return;

        int item = shop.itemsShop[shopIndex];

        if (item == -1)
            return;

        ITradeItem tradeItem =
            dataCarrier.allItems[item] as ITradeItem;

        if (tradeItem == null)
            return;

        if (Trade < tradeItem.Level)
            return;

        for (int i = 0;
             i < inventoryScr.sharedInventory.inventory.Length;
             i++)
        {
            if (inventoryScr.sharedInventory.inventory[i] == -1)
            {
                inventoryScr.sharedInventory.inventory[i] = item;

                shop.itemsShop[shopIndex] = -1;

                Trade -= tradeItem.Level;

                break;
            }
        }

        SlotUpdate();
    }

    // ─────────────────────────────────────────
    // UPDATE UI
    // ─────────────────────────────────────────

    private void SlotUpdate()
    {
        // SHOP
        for (int i = 0; i < slotShop.Length; i++)
        {
            if (i >= shop.itemsShop.Count)
            {
                slotShop[i].scrObject = null;
                slotShop[i].UpdateSlot();
                continue;
            }

            int item = shop.itemsShop[i];

            if (item >= 0 &&
                item < dataCarrier.allItems.Length)
            {
                slotShop[i].scrObject =
                    dataCarrier.allItems[item];
            }
            else
            {
                slotShop[i].scrObject = null;
            }

            slotShop[i].UpdateSlot();
        }

        // PLAYER
        for (int i = 0; i < slotPlayer.Length; i++)
        {
            if (i >= inventoryScr.sharedInventory.inventory.Length)
            {
                slotPlayer[i].scrObject = null;
                slotPlayer[i].UpdateSlot();
                continue;
            }

            int item =
                inventoryScr.sharedInventory.inventory[i];

            if (item >= 0 &&
                item < dataCarrier.allItems.Length)
            {
                slotPlayer[i].scrObject =
                    dataCarrier.allItems[item];
            }
            else
            {
                slotPlayer[i].scrObject = null;
            }

            slotPlayer[i].UpdateSlot();
        }

        // SELECT
        if (selectItem >= 0 &&
            selectItem < dataCarrier.allItems.Length)
        {
            selectSlotSlot.scriptableObject =
                dataCarrier.allItems[selectItem];
        }
        else
        {
            selectSlotSlot.scriptableObject = null;
        }

        selectSlotSlot.UpdateSlot();

        traidText.text = Trade.ToString();

        npcZoneShop.shop = shop;
    }

    // ─────────────────────────────────────────
    // INFO UI
    // ─────────────────────────────────────────

    public void SelectEnterItemUI(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shop.itemsShop.Count)
            return;

    }

    public void SelectEnterInventoryUI(int itemIndex)
    {
        if (itemIndex < 0 ||
            itemIndex >= inventoryScr.sharedInventory.inventory.Length)
            return;

    }

    public void SelectExitItemUI()
    {
    }


    // ─────────────────────────────────────────
    // SWAP
    // ─────────────────────────────────────────

    private void Swap(int shopIndex)
    {
        int temp = selectItem;

        selectItem = shop.itemsShop[shopIndex];

        shop.itemsShop[shopIndex] = temp;
    }

    public void Initialize()
    {
        if (npcZoneShop == null)
        {
            Debug.LogError("NPCZoneShop NULL");
            return;
        }

        if (inventoryScr == null)
        {
            Debug.LogError("InventoryScr NULL");
            return;
        }

        if (dataCarrier == null)
        {
            Debug.LogError("BattleDataCarrier NULL");
            return;
        }

        shop = npcZoneShop.shop;

        if (shop == null)
        {
            Debug.LogError("Shop NULL");
            return;
        }

        InitShopSlots();

        RefreshShop();
    }

    public void RefreshShop()
    {
        if (npcZoneShop == null)
        {
            Debug.LogError("npcZoneShop NULL");
            return;
        }

        shop = npcZoneShop.shop;

        if (shop == null)
        {
            Debug.LogError("shop NULL");
            return;
        }

        InitShopSlots();

        SlotUpdate();
    }
}