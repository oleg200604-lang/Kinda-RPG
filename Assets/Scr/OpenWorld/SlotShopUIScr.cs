using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotShopUIScr : MonoBehaviour
{
    public ScriptableObject scrObject;
    public Image iconObj;
    public TextMeshProUGUI levelText;
    public void UpdateSlot()
    {
        Sprite icon = null;

        //if (scrObject is EffectItemData eid)
        //{
            //icon = eid.icon;
            //levelText.text = eid.description;
        //}

        

        if (scrObject is ItemData id)
        {
            icon = id.iconSprite;
            levelText.text = id.level.ToString();
            levelText.color = new Color32(255, 255, 255, 255);
        }

        else if (scrObject is WeaponData wd)
        {
            icon = wd.iconSprite;
            levelText.text = wd.level.ToString();
            levelText.color = new Color32(255, 255, 255, 255);
        }

        else if (scrObject is ArmorData ad)
        {
            icon = ad.iconSprite;
            levelText.text = ad.level.ToString();
            levelText.color = new Color32(255, 255, 255, 255);
        }

        if (icon != null)
        {
            iconObj.sprite = icon;
            iconObj.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            iconObj.sprite = null;
            iconObj.color = new Color32(255, 255, 255, 0);
            levelText.color = new Color32(255, 255, 255, 0);
        }
    }
}
