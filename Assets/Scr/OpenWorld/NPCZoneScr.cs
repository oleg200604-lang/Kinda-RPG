using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum QuestTypeKill
{
    None,
    Wolf,
    Slime
}

[DefaultExecutionOrder(-100)]
public class NPCZoneScr : MonoBehaviour
{
    [Header("Trade")]
    public int trade;

    public NPCZoneUIScr zoneUI;

    public int line;

    public GameObject[] objDialogue;

    public ObjDialogueLine[] dialogue;

    public NPCData[] npcDatas;

    public Shop shop;

    [HideInInspector]
    public bool isZoneNPC;

    // ─────────────────────────────────────────
    // AWAKE
    // ─────────────────────────────────────────

    private void Awake()
    {
        ShopStart();
    }

    // ─────────────────────────────────────────
    // SHOP
    // ─────────────────────────────────────────

    public void ShopStart()
    {
        if (shop == null)
            return;

        // INIT LIST
        if (shop.itemsShop == null)
        {
            shop.itemsShop = new List<int>();
        }

        shop.itemsShop.Clear();

        // ВСІ СЛОТИ = -1
        for (int i = 0; i < shop.Max; i++)
        {
            shop.itemsShop.Add(-1);
        }

        // Захист
        if (shop.itemShopData == null ||
            shop.itemShopData.Length == 0)
        {
            Debug.LogWarning("Shop itemShopData empty");
            return;
        }

        int itemCount =
            Random.Range(shop.Min, shop.Max + 1);

        itemCount =
            Mathf.Clamp(itemCount, 0, shop.Max);

        for (int i = 0; i < itemCount; i++)
        {
            int randomItem =
                shop.itemShopData[
                    Random.Range(0, shop.itemShopData.Length)];

            shop.itemsShop[i] = randomItem;
        }
    }

    // ─────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────

    public void OpenInteraction()
    {
        if (zoneUI == null)
            return;

        zoneUI.SelectNPCPanel.SetActive(true);
    }

    public void OpenShop()
    {
        if (zoneUI == null)
            return;

        zoneUI.SelectNPCPanel.SetActive(false);

        zoneUI.UIPanelShop.SetActive(true);

        ShopUIScr shopUI =
            zoneUI.UIPanelShop.GetComponent<ShopUIScr>();

        if (shopUI != null)
        {
            // ВАЖЛИВО
            shopUI.npcZoneShop = this;

            // ПЕРЕПІДКЛЮЧАЄМО SHOP
            shopUI.shop = shop;

            // ОНОВЛЕННЯ UI
            shopUI.RefreshShop();
        }
    }

    public void ExitShop()
    {
        if (zoneUI == null)
            return;

        zoneUI.UIPanelShop.SetActive(false);

        zoneUI.SelectNPCPanel.SetActive(true);
    }
}

[System.Serializable]
public class QuestKill
{
    public QuestKillData questKillData;
    public int questCurrentAmount;
}

[System.Serializable]
public class QuestNPC
{
    public QuestNPCData questKillData;
    public bool questActive;
}


[System.Serializable]
public class QuestGet
{
    public QuestGetData questItemData;
}


[System.Serializable]
public class Shop
{
    public int[] itemShopData;
    public List<int> itemsShop;
    public int Max, Min;
}

[CreateAssetMenu(menuName = "Game/Quest/Kill Data")]
public class QuestKillData : ScriptableObject
{
    public QuestTypeKill questTypeKill;
    public int questAmount;

    public ScriptableObject[] itemRemuneration;
    public int XPRemuneration;
}


[CreateAssetMenu(menuName = "Game/Quest/NPC Data")]
public class QuestNPCData : ScriptableObject
{
    public NPCData NPCData;

    public ScriptableObject[] itemRemuneration;
    public int XPRemuneration;
}


[CreateAssetMenu(menuName = "Game/Quest/Get Data")]
public class QuestGetData : ScriptableObject
{
    public ScriptableObject itemGet;
    public int questAmount;

    public ScriptableObject[] itemRemuneration;
    public int XPRemuneration;
}

[CreateAssetMenu(menuName = "Game/NPC Data")]
public class NPCData : ScriptableObject
{
    public Dialogue[] dialogs;
    public QuestDialogue[] questDialogs;
    public DialogueQuest[] dialogsQuest;
}


[System.Serializable]
public class QuestDialogue
{
    public DialogueLine[] dialogueLines;
    public QuestNPC questNPC;
    public QuestKill questKill;
    public QuestGet questGet;
}

[System.Serializable]
public class DialogueQuest
{
    public QuestNPC questNPC;
    public DialogueLine[] dialogueLines;
}

[System.Serializable]
public class Dialogue
{
    public DialogueLine[] dialogueLines;
}

[System.Serializable]
public class DialogueLine
{
    public string nameSpeaker;
    public bool isNPC;
    public Sprite spriteNPC;
    [TextArea(3, 5)]
    public string textSpeaker;
}

[System.Serializable]
public class ObjDialogueLine
{
    public TextMeshProUGUI textName;
    public UnityEngine.UI.Image imageNPC;
    public TextMeshProUGUI textText;
}

