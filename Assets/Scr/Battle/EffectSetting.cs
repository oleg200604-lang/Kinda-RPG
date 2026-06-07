using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Effect
{
    None, Bleeding, Poisoning, Shock, Fire, Frostbite, Damage, Healing, DamageBonus, WeaponBonus
}

[System.Serializable]
public class EffectSetting
{

    public Effect effectType;
    public int effectPower;
    public void ActionEffect(PlayerScr target)
    {
        switch (effectType)
        {
            case Effect.None:

                break;

            case Effect.Bleeding:
                for (int i=0; target.effectDatas.Count > i; i++)
                {
                    if (target.effectDatas[i].effectType == Effect.Healing)
                    {
                        effectPower = 0;
                    }
                }
                target.TakeFullDamage(effectPower);
                break;

            case Effect.Poisoning:
                for (int i = 0; target.effectDatas.Count > i; i++)
                {
                    if (target.effectDatas[i].effectType == Effect.Healing)
                    {
                        if (effectPower > target.effectDatas[i].effectPower)
                        {
                            effectPower -= target.effectDatas[i].effectPower;
                        }
                        else
                        {
                            effectPower -= 0;
                        }
                    }
                }
                target.TakeFullDamage(effectPower);
                effectPower--;
                break;

            case Effect.Shock:
                target.TakeShock(effectPower);
                break;

            case Effect.Fire:
                target.TakeDamage(effectPower, null);
                effectPower -= target.def;
                if (effectPower < target.def)
                {
                    effectPower = 0;
                }
                if (effectPower>0)
                {
                    effectPower += 1;
                }
                break;

            case Effect.Frostbite:
                for (int i = 0; target.effectDatas.Count > i; i++)
                {
                    if (target.effectDatas[i].effectType == Effect.Frostbite)
                    {
                        if (target.effectDatas[i].effectPower > effectPower) 
                        {
                            target.effectDatas[i].effectPower -= effectPower;
                            effectPower = 0;
                        }
                        else if(target.effectDatas[i].effectPower == effectPower)
                        {
                            target.effectDatas[i].effectPower = 0;
                            effectPower = 0;
                        }
                        else
                        {
                            effectPower -= target.effectDatas[i].effectPower;
                            target.effectDatas[i].effectPower = 0;
                        }
                    }
                }
                target.TakeFrostbite(effectPower);
                break;
            case Effect.Damage: 
                target.TakeDamage(effectPower, null); 
                effectPower = 0; 
                break;

            case Effect.Healing: 
                target.TakeHealing(effectPower); 
                effectPower = 0; 
                break;

            case Effect.DamageBonus:
                target.damageBonus+=effectPower;
                effectPower = 0; 
                break;
        }
    }

    public EffectSetting Clone()
    {
        return new EffectSetting()
        {
            effectType = this.effectType,
            effectPower = this.effectPower
        };
    }
}