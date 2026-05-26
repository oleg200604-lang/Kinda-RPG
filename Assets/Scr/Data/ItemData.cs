
using System.Collections.Generic;
using UnityEngine;
public enum AttacType
{
    Slow = 0,
    Fast = 1
}

public enum EffectType
{
    typeSingle = 0,
    typeConstant = 1,
    typeDefend = 2,
    typeAttack = 3,
    typeDamage = 4,
    typeTakeDamege = 5,
    typeHealing = 6,
    typeShok = 7,
    typeFrostbite = 8,
    targetBlock = 9
}

public enum EffectTarget
{
    Player = 0,
    Enemy = 1
}

public enum TargetEffect
{
    targetNone = 0,
    targetBleeding = 1,
    targetPoisoning = 2,
    targetShock = 3,
    targetFire = 4,
    targetFrostbite = 5,
    targetDamage = 6,
    targetHealing = 7,
    targetWeaponBonus = 8,
    targetHitPoint = 9,
    targetAttack = 10,
    targetNoneHitPoint = 11
}


[System.Serializable]
public class DefClass
{
    public int defens;
    public int defensRecharge;
    public int DefendCost;
}


[System.Serializable]
public class AttackClass
{
    public AttacType attacType;
    public int damage;
    public int attackRecharge;
    public int attackCost;
}
[System.Serializable]
public class ItemClass
{
    public EffectSetting effectSetting;
    public EffectTarget effectTarget;
    public EffectType effectType;
}
[System.Serializable]
public class EffectClass
{
    public EffectSetting effect;
    public EffectType effectType;
    public EffectTarget acionTarget;
    public EffectTarget target;
    public int powerMin;
    public int power;
    public int powerMax;
}

[CreateAssetMenu(fileName = "Game", menuName = "Data/Item")]
public class ItemData : ScriptableObject, ITradeItem
{
    public int level;
    public int Level => level;

    public Sprite iconSprite;
    public ItemClass[] itemClass;
    public string itemName;
    public int itemRecharge;
    public int itemCost;
    public int itemMaxAmount;
    public string description;
}