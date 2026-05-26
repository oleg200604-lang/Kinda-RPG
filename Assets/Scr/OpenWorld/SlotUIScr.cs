using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIScr : MonoBehaviour
{
    public ScriptableObject scriptableObject;
    public Image iconObj;

    public void UpdateSlot()
    {
        Sprite icon = null;

        
        if (scriptableObject is ItemData id)
            icon = id.iconSprite;
        else if (scriptableObject is WeaponData wd)
            icon = wd.iconSprite;
        else if (scriptableObject is ArmorData ad)
            icon = ad.iconSprite;

        if (icon != null)
        {
            print("Не порожній");
            iconObj.sprite = icon;
            iconObj.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            print("Порожній");
            iconObj.sprite = null;
            iconObj.color = new Color32(255, 255, 255, 0);
        }
    }
}
