using System.Collections.Generic;
using UnityEngine;
public interface ITradeItem
{
    int Level { get; }
}
[CreateAssetMenu(fileName = "Game", menuName = "Data/Weapon")]
public class WeaponData : ScriptableObject, ITradeItem
{
    public int level;
    public int Level => level;

    public Sprite iconSprite;
    public DefClass defendClass;
    public List<EffectItemData> effectItemClass;
    public AttackClass attackClass;
    public string description;
    public int itemID;
}