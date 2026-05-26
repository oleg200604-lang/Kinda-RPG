using UnityEngine;

public class PlayerInventoryScr : MonoBehaviour
{

    [Header("Shared Inventory (source)")]
    [SerializeField] private InventoryData sharedInventory;

    public WeaponData weaponData;
    public ArmorData armorData;
    public ItemData[] itemData;

    public WeaponData weaponNoneData;
    public ArmorData armorNoneData;
    public ItemData itemNoneData;
    public bool IsReady;

    public void InitInventory(InventoryData source, BattleDataCarrier battleData)
    {
        if (source == null)
        {
            Debug.LogError("InitInventory: source NULL");
            IsReady = false;
            return;
        }

        sharedInventory = source;
        if (source.weapon > -1)
        {
            if (battleData.allItems[source.weapon] is WeaponData weapon)
            {
                weaponData = weapon;
            }
            else
            {
                weaponData = weaponNoneData;
            }
        }
        else
        {
            weaponData = weaponNoneData;
        }

        if (source.armor>-1)
        {
            if (battleData.allItems[source.armor] is ArmorData armor)
            {
                armorData = armor;
            }
            else
            {
                armorData = armorNoneData;
            }
        }
        else
        {
            armorData = armorNoneData;
        }


        if (source.items == null)
        {
            IsReady = false;
            return;
        }

        itemData = new ItemData[source.items.Length];

        for (int i = 0; i < source.items.Length; i++)
        {
            var raw = source.items[i];

            if (raw > -1)
            {
                if (battleData.allItems[source.items[i]] is ItemData item)
                {
                    itemData[i] = item;
                }

                else
                {
                    itemData[i] = itemNoneData;
                    continue;
                }
            }
            else
            {
                itemData[i] = itemNoneData;
                continue;
            }
        }

        IsReady = true;
    }

}
