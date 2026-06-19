using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class InventoryScr : MonoBehaviour
{
    [Header("Player Stats UI")]
    public TextMeshProUGUI upgradePointsText;

    public TextMeshProUGUI powerText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI vitalityText;

    public Image imageStaticSpeedAttack;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackRechargeText;
    public TextMeshProUGUI attackCostText;

    public TextMeshProUGUI defendText;
    public TextMeshProUGUI defendCostText;
    public TextMeshProUGUI defendRechargeText;

    [Header("Info Panel Positioning")]
    public RectTransform canvasRect;      // RectTransform головного Canvas
    public Vector2 panelOffset = new Vector2(16f, 0f);
    public Vector2 panelPadding = new Vector2(8f, 8f);

    private Canvas _rootCanvas;
    private RectTransform _weaponPanelRect, _armorPanelRect, _itemPanelRect;
    [Header("Start Item")] public int startItemId = 0;

    [Header("Inventory Slots")]
    public SlotUIScr[] slotUI;
    public SlotUIScr slotWeaponUI, slotArmorUI, slotSelectUI;
    public SlotUIScr[] inventoryItemUI;

    [Header("Defaults")]
    public ArmorData noneArmor;

    [Header("Info Panels")]
    public GameObject weaponPanel, armorPanel, itemPanel;
    public TextMeshProUGUI[] inventoryNameText, inventoryDescriptionText;
    public TextMeshProUGUI[] weaponAttackText, weaponDefenText;
    public TextMeshProUGUI[] armorStaticText;
    public TextMeshProUGUI armorArmorText, itemStaticText, itemMaxText;


    public Image imageSpeedAttack;
    public Sprite[] spriteSpeedAttack;
    [Header("Shared")]
    public InventoryData sharedInventory;

    public int selectItem = -1;

    private BattleDataCarrier dataCarrier;
    private float saveTimer;

    private int[] _cachedInv;
    private int _cachedWeapon = int.MinValue, _cachedArmor = int.MinValue;
    private int[] _cachedItems;
    private int _cachedSelect = int.MinValue;

    private bool isHoveringSlot = false;
    private int hoveredId = -1;
    private RectTransform hoveredSlot = null;
    public KeyCode interactKey;
    private void Awake()
    {
        dataCarrier = BattleDataCarrier.Instance;
        foreach (var item in dataCarrier.allItems)
            if (item != null) _ = item.name;

        LoadOrInit();
        StartCoroutine(SavePositionRoutine());
        UpdateInventoryUI();
        transform.position = sharedInventory.playerPosition;

        _weaponPanelRect = weaponPanel.GetComponent<RectTransform>();
        _armorPanelRect = armorPanel.GetComponent<RectTransform>();
        _itemPanelRect = itemPanel.GetComponent<RectTransform>();
        _rootCanvas = canvasRect != null ? canvasRect.GetComponentInParent<Canvas>() : GetComponentInParent<Canvas>();
        weaponPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        armorPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        itemPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    private void PositionInfoPanelNearSlot(RectTransform slotRect, RectTransform panelRect)
    {
        UpdatePlayerStatsUI();
        if (slotRect == null || panelRect == null || canvasRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        Camera cam = (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _rootCanvas.worldCamera
            : null;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, slotRect.position);

        Vector2 panelSize = panelRect.rect.size;

        Vector2 targetScreen = screenPos + new Vector2(panelSize.x * 0.5f + panelOffset.x, panelOffset.y);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (targetScreen.x + panelSize.x > screenWidth)
        {
            targetScreen = screenPos - new Vector2(panelSize.x * 0.5f + panelOffset.x, -panelOffset.y);
        }

        targetScreen.x = Mathf.Clamp(targetScreen.x, 0, screenWidth);
        targetScreen.y = Mathf.Clamp(targetScreen.y, 0, screenHeight);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreen, cam, out Vector2 localPos);

        panelRect.anchoredPosition = localPos;
    }
    private void LoadOrInit()
    {
        var loaded = SaveSystem.Load();
        if (loaded != null)
        {
            sharedInventory.Name = loaded.Name;
            sharedInventory.Lv = loaded.Lv;
            sharedInventory.XP = loaded.XP;
            sharedInventory.hitPoint = loaded.hitPoint;
            sharedInventory.playerPosition = loaded.playerPosition;
            if (loaded.inventory != null) sharedInventory.inventory = loaded.inventory;
            sharedInventory.weapon = loaded.weaponData;
            sharedInventory.armor = loaded.armorData;
            if (loaded.itemData != null) sharedInventory.items = loaded.itemData;
        }
        else
        {
            for (int i = 0; i < sharedInventory.inventory.Length; i++)
                sharedInventory.inventory[i] = -1;
            sharedInventory.AddInventory(startItemId);
            sharedInventory.ResrtHitPoint(dataCarrier);
            SaveSystem.Save(sharedInventory);
        }

        if (sharedInventory.hitPoint <= 0)
            sharedInventory.ResrtHitPoint(dataCarrier);
    }

    #region Save / Load
    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            SaveSystem.DeleteSave();

            for (int i = 0;
                 i < sharedInventory.inventory.Length;
                 i++)
            {
                sharedInventory.inventory[i] = -1;
            }

            for (int i = 0;
                 i < sharedInventory.items.Length;
                 i++)
            {
                sharedInventory.items[i] = -1;
            }

            sharedInventory.weapon = 0;
            sharedInventory.armor = -1;

            selectItem = -1;

            sharedInventory.ResrtHitPoint(dataCarrier);

            _cachedInv = null;
            _cachedItems = null;

            _cachedWeapon = int.MinValue;
            _cachedArmor = int.MinValue;
            _cachedSelect = int.MinValue;

            UpdateInventoryUI();

            SaveSystem.Save(sharedInventory);

            Debug.Log("SAVE RESET");
        }
        if (selectItem != -1)
        {
            HideInfoPanels();
        }
        saveTimer += Time.deltaTime;
        if (saveTimer > 2f) { SaveSystem.Save(sharedInventory); saveTimer = 0f; }

        if (Input.GetKey(KeyCode.V))
        {
            sharedInventory.AddInventory(Random.Range(0, dataCarrier.allItems.Length));
            UpdateInventoryUI();
        }
    }

    private IEnumerator SavePositionRoutine()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true) { sharedInventory.playerPosition = transform.position; yield return wait; }
    }
    #endregion

    #region UI Update (слоти)
    public void UpdateInventoryUI()
    {
        UpdateSlots(slotUI, sharedInventory.inventory, ref _cachedInv);

        int wId = sharedInventory.weapon;
        if (_cachedWeapon != wId)
        {
            slotWeaponUI.scriptableObject = wId > -1 ? dataCarrier.allItems[wId] : null;
            slotWeaponUI.UpdateSlot();
            _cachedWeapon = wId;
        }

        int aId = sharedInventory.armor;
        if (_cachedArmor != aId)
        {
            slotArmorUI.scriptableObject = aId > -1 ? dataCarrier.allItems[aId] : null;
            slotArmorUI.UpdateSlot();
            _cachedArmor = aId;
        }

        UpdateSlots(inventoryItemUI, sharedInventory.items, ref _cachedItems);

        if (_cachedSelect != selectItem)
        {
            slotSelectUI.scriptableObject = selectItem > -1 ? dataCarrier.allItems[selectItem] : null;
            slotSelectUI.UpdateSlot();
            _cachedSelect = selectItem;
        }
    }

    private void UpdateSlots(SlotUIScr[] slots, int[] ids,  ref int[] cache)
    {
        if (cache == null)
        {
            cache = new int[slots.Length];

            for (int i = 0; i < cache.Length; i++)
            {
                cache[i] = int.MinValue;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            int id = -1;

            if (i < ids.Length)
            {
                id = ids[i];
            }

            if (cache[i] == id)
                continue;

            slots[i].scriptableObject =
                id >= 0
                ? dataCarrier.allItems[id]
                : null;

            slots[i].UpdateSlot();

            cache[i] = id;
        }
    }
    #endregion

    #region Info Panel

    public void UpdatePlayerStatsUI()
    {
        ArmorData armor =
            sharedInventory.armor >= 0
            ? dataCarrier.allItems[sharedInventory.armor] as ArmorData
            : noneArmor;

        WeaponData weapon =
            sharedInventory.weapon >= 0
            ? dataCarrier.allItems[sharedInventory.weapon] as WeaponData
            : null;

        // Бали покращення
        upgradePointsText.text = sharedInventory.playerPosition.ToString();

        // Стати + бонус броні
        powerText.text = (sharedInventory.staticPlayer.power + armor.armorStatic.power).ToString();

        speedText.text = (sharedInventory.staticPlayer.speed + armor.armorStatic.speed).ToString();

        staminaText.text =(sharedInventory.staticPlayer.staminaMax + armor.armorStatic.staminaMax).ToString();

        vitalityText.text = (sharedInventory.staticPlayer.vitalityMax + armor.armorStatic.vitalityMax).ToString();

        if (weapon != null)
        {
            // Атака
            damageText.text = weapon.attackClass.damage.ToString();
            attackRechargeText.text = weapon.attackClass.attackRecharge.ToString();
            attackCostText.text = weapon.attackClass.attackCost.ToString();

            // Захист
            defendText.text = (weapon.defendClass.defens + armor.armor).ToString();
            defendCostText.text = weapon.defendClass.DefendCost.ToString();
            defendRechargeText.text = weapon.defendClass.defensRecharge.ToString();

            if (weapon.attackClass.attacType == AttacType.Slow)
            {
                imageStaticSpeedAttack.sprite = spriteSpeedAttack[1];
            }
            else
            {
                imageStaticSpeedAttack.sprite = spriteSpeedAttack[0];
            }
        }
        else
        {
            damageText.text = "0";
            attackRechargeText.text = "0";
            attackCostText.text = "0";

            defendText.text = "0";
            defendRechargeText.text = "0";
            defendCostText.text = "0";
        }

    }
    private void HideInfoPanels()
    {
        weaponPanel.SetActive(false);
        armorPanel.SetActive(false);
        itemPanel.SetActive(false);
    }
    public void ShowItemInfo(int id, RectTransform anchorSlot = null)
    {
        if (id < 0)
        {
            HideInfoPanels();
            return;
        }

        var data = dataCarrier.allItems[id];
        RectTransform targetPanelRect = null;

        if (data is WeaponData w)
        {
            weaponPanel.SetActive(true);
            armorPanel.SetActive(false);
            itemPanel.SetActive(false);

            targetPanelRect = _weaponPanelRect;

            inventoryNameText[0].text = $"{w.name} {w.Level}Lv";
            inventoryDescriptionText[0].text = w.description;

            weaponAttackText[0].text = $"{w.attackClass.damage}";
            weaponAttackText[1].text = $"{w.attackClass.attackRecharge}";
            weaponAttackText[2].text = $"{w.attackClass.attackCost}";

            weaponDefenText[0].text = $"{w.defendClass.defens}";
            weaponDefenText[1].text = $"{w.defendClass.defensRecharge}";
            weaponDefenText[2].text = $"{w.defendClass.DefendCost}";
        }
        else if (data is ArmorData a)
        {
            weaponPanel.SetActive(false);
            armorPanel.SetActive(true);
            itemPanel.SetActive(false);

            targetPanelRect = _armorPanelRect;

            inventoryNameText[1].text = $"{a.name} {a.Level}Lv";
            inventoryDescriptionText[1].text = a.description;

            armorArmorText.text = a.armor.ToString();
            armorStaticText[0].text = a.armorStatic.power.ToString();
            armorStaticText[1].text = a.armorStatic.speed.ToString();
            armorStaticText[2].text = a.armorStatic.staminaMax.ToString();
            armorStaticText[3].text = a.armorStatic.vitalityMax.ToString();
        }
        else if (data is ItemData it)
        {
            weaponPanel.SetActive(false);
            armorPanel.SetActive(false);
            itemPanel.SetActive(true);

            targetPanelRect = _itemPanelRect;

            inventoryNameText[2].text = $"{it.name} {it.Level}Lv";
            inventoryDescriptionText[2].text = it.description;

            itemStaticText.text = $"{it.itemRecharge} {it.itemCost}";
            itemMaxText.text = it.itemMaxAmount.ToString();
        }

        if (anchorSlot != null && targetPanelRect != null)
            PositionInfoPanelNearSlot(anchorSlot, targetPanelRect);
    }
    public void SelectEnterItemUI(int slotIndex)
    {
        int id = sharedInventory.inventory[slotIndex];

        if (id < 0)
        {
            isHoveringSlot = false;
            hoveredId = -1;
            HideInfoPanels();
            return;
        }

        isHoveringSlot = true;
        hoveredId = id;
        hoveredSlot = slotUI[slotIndex].transform as RectTransform;

        ShowItemInfo(id, hoveredSlot);
    }

    public void SelectExitItemUI()
    {
        isHoveringSlot = false;
        hoveredId = -1;
        hoveredSlot = null;

        HideInfoPanels();
    }
    public void UpdateWeaponUI()
    {
        if (sharedInventory.weapon < 0)
        {
            HideInfoPanels();
            return;
        }

        ShowItemInfo(
            sharedInventory.weapon,
            slotWeaponUI.transform as RectTransform);
    }
    public void UpdateArmorUI()
    {
        if (sharedInventory.armor < 0)
        {
            HideInfoPanels();
            return;
        }

        ShowItemInfo(
            sharedInventory.armor,
            slotArmorUI.transform as RectTransform);
    }
    public void UpdateItemUI(int i)
    {
        int id = sharedInventory.items[i];

        if (id < 0)
        {
            HideInfoPanels();
            return;
        }

        ShowItemInfo(
            id,
            inventoryItemUI[i].transform as RectTransform);
    }
    #endregion

    #region Core Logic
    public void SwitchInventory(int index)
    {
        if ((uint)index >= (uint)sharedInventory.inventory.Length)
            return;

        (sharedInventory.inventory[index], selectItem) =
            (selectItem, sharedInventory.inventory[index]);

        HideInfoPanels();

        SaveSystem.Save(sharedInventory);
        UpdateInventoryUI();
    }

    public void EquipWeapon()
    {
        if (selectItem >= 0 && dataCarrier.allItems[selectItem] is WeaponData)
            (sharedInventory.weapon, selectItem) = (selectItem, sharedInventory.weapon);
        else
            (sharedInventory.weapon, selectItem) = (-1, sharedInventory.weapon);

        HideInfoPanels();
        UpdateInventoryUI();

    }

    public void EquipArmor()
    {
        ArmorData oldArmor =
            sharedInventory.armor >= 0
            ? dataCarrier.allItems[sharedInventory.armor] as ArmorData
            : noneArmor;

        // ЗНЯТТЯ БРОНІ
        if (selectItem == -1)
        {
            if (sharedInventory.armor == -1)
                return;

            sharedInventory.UpdateArmor(
                noneArmor,
                oldArmor);

            selectItem = sharedInventory.armor;
            sharedInventory.armor = -1;

            UpdateInventoryUI();
            return;
        }

        // НЕ БРОНЯ
        if (dataCarrier.allItems[selectItem]
            is not ArmorData newArmor)
        {
            return;
        }

        sharedInventory.UpdateArmor(
            newArmor,
            oldArmor);

        (sharedInventory.armor, selectItem) =
            (selectItem, sharedInventory.armor);

        HideInfoPanels();
        UpdateInventoryUI();
    }

    public void EquipItem(int index)
    {
        if (selectItem >= 0 && dataCarrier.allItems[selectItem] is ItemData)
            (sharedInventory.items[index], selectItem) = (selectItem, sharedInventory.items[index]);

        HideInfoPanels();
        UpdateInventoryUI();
    }

    public void DeleteSelected() { selectItem = -1; UpdateInventoryUI(); }
    public void RemovInventory(int i) { sharedInventory.inventory[i] = -1; UpdateInventoryUI(); }
    #endregion
}

[System.Serializable]
public class InventoryData
{
    public string Name;
    public int Lv;
    public int XP;
    public int[] XPMax;
    public Static staticPlayer;
    public int[] inventory;
    public int weapon;
    public int armor;
    public int[] items;

    public int hitPoint;
    public Sprite[] animatorSprite;

    //[Header("Quest")]
    // public List<QuestKill> questKills = new();
    // public List<QuestNPC> questNPCs = new();
    // public List<QuestGet> questGets = new();

    [Header("Player Position")]
    public Vector3 playerPosition;  // ← додати це

    public void UpdateArmor(ArmorData newArmor, ArmorData oldArmor)
    {
        int oldArmorBonus = oldArmor != null ? oldArmor.armorStatic.vitalityMax : 0;
        int newArmorBonus = newArmor != null ? newArmor.armorStatic.vitalityMax : 0;

        int oldMax = staticPlayer.vitalityMax + oldArmorBonus;
        int newMax = staticPlayer.vitalityMax + newArmorBonus;

        if (hitPoint >= oldMax)
        {
            // Були на максимумі — залишаємось на максимумі нової броні
            hitPoint = newMax;
        }
        else
        {
            // Були не на максимумі — зберігаємо різницю від старого максимуму
            int missingHP = oldMax - hitPoint;
            hitPoint = Mathf.Max(1, newMax - missingHP);
        }
    }


    public void AddInventory(int newItem)
    {
        if (newItem == -1)
        {
            Debug.LogError("Спроба додати NULL предмет");
            return;
        }

        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == -1)
            {
                inventory[i] = newItem;
                return;
            }
        }
    }

    public void ResrtHitPoint(BattleDataCarrier dataCarrier)
    {
        int baseHP = staticPlayer.vitalityMax;

        if (armor >= 0 && armor < dataCarrier.allItems.Length &&
            dataCarrier.allItems[armor] is ArmorData armors)
        {
            hitPoint = baseHP + armors.armorStatic.vitalityMax;
        }
        else
        {
            hitPoint = baseHP; // броні немає — базове HP
        }
    }
}

[CreateAssetMenu(menuName = "Game/Bot Data")]
public class BotData : ScriptableObject
{
    public int remuneration;
    public QuestTypeKill typeKill;
    public PriorityEffectRC[] priorityEffects;
    public PriorityStaticRC[] priorityStatic;

    [Header("Personality")]
    public float attackRC = 1f;
    public float defendRC = 1f;
    public float skipRC = 0.3f;

    [Header("Items Base RC")]
    public float[] itemRC = new float[3] { 0f, 0f, 0f };

}

[System.Serializable]
public class SaveData
{
    public string Name;
    public int Lv;
    public int XP;
    public int hitPoint;
    public Vector3 playerPosition;
    public int[] inventory;
    public int weaponData;
    public int armorData;
    public int[] itemData;
}

public static class SaveSystem
{
    private static string path => Application.persistentDataPath + "/save.json";

    public static void Save(InventoryData data)
    {
        SaveData save = new SaveData
        {
            Name = data.Name,
            Lv = data.Lv,
            XP = data.XP,
            hitPoint = data.hitPoint,
            playerPosition = data.playerPosition,
            inventory = data.inventory,
            weaponData = data.weapon,
            armorData = data.armor,
            itemData = data.items
        };
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(path, json);
    }
    public static void DeleteSave()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    public static SaveData Load()
    {
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
