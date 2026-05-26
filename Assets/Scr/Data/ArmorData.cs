using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game", menuName = "Data/Armor")]
public class ArmorData : ScriptableObject, ITradeItem
{
    public int level;
    public int Level => level;

    public Sprite iconSprite;
    public Static armorStatic = new Static();
    public List<EffectItemData> effectItemClass;
    public int armor;
    public string description;
}
