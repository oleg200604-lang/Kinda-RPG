using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class Static
{
    public int power, speed;
    public int staminaMax;
    public int vitalityMax;
    public void Reset()
    {
        power  =  speed = 0;
        staminaMax = vitalityMax = 0;
    }
}
public class PlayerScr : MonoBehaviour
{
    public int hitPoint;
    public int hitPointBonus;
    public EffectTarget whoIm;
    public BattleManegerScr manegerScr;
    public PlayerInventoryScr inventory;
    public int damageBonus;
    public int def;
    public int energy;
    public Static playerStats;
    public Static playerBonusStats;
    public bool IsMove;
    public int attackRech;
    public int defendRech;
    public List<EffectSetting> effectDatas;
    public ItemData[] item;
    public int[] itemAmount = { 0, 0, 0 };


    public void MoveAttack(PlayerScr enemy)
    {
        if (attackRech <= 0 && inventory.weaponData.attackClass.attackCost <= energy)
        {
            EffectRun(EffectType.typeAttack, 1);

            int baseDamage = inventory.weaponData.attackClass.damage + damageBonus + playerStats.power + playerBonusStats.power + inventory.armorData.armorStatic.power;

            switch (inventory.weaponData.attackClass.attacType)
            {
                case AttacType.Slow:
                    EffectRun(EffectType.typeDamage, baseDamage);
                    enemy.TakeDamage(baseDamage, gameObject.GetComponent<PlayerScr>());
                    break;
                case AttacType.Fast:
                    for (int i = 0; i < (inventory.weaponData.attackClass.damage + damageBonus); i++)
                    {
                        EffectRun(EffectType.typeDamage, playerStats.power + playerBonusStats.power + inventory.armorData.armorStatic.power);
                        enemy.TakeDamage(1 + playerStats.power + playerBonusStats.power + inventory.armorData.armorStatic.power, gameObject.GetComponent<PlayerScr>());
                    }
                    break;
            }

            energy -= inventory.weaponData.attackClass.attackCost;
            attackRech = inventory.weaponData.attackClass.attackRecharge;
        }
    }

    public void MoveItem(int i)
    {

        PlayerScr target;

        foreach (var effectData in item[i].itemClass)
        {

            PlayerScr self = (whoIm == EffectTarget.Player) ? manegerScr.player : manegerScr.enemy;
            PlayerScr other = (whoIm == EffectTarget.Player) ? manegerScr.enemy : manegerScr.player;

            target = (effectData.effectTarget == EffectTarget.Player) ? self : other;

            EffectSetting effectCopy = effectData.effectSetting.Clone();

            if (effectCopy == null)
            {
                continue;
            }

            if (effectData.effectType == EffectType.typeSingle)
            {
                effectCopy.ActionEffect(target);
            }
            else
            {
                EffectSetting existing = target.effectDatas
                    .FirstOrDefault(e => e.effectType == effectCopy.effectType);

                if (existing != null)
                {
                    existing.effectPower += effectCopy.effectPower;
                }
                else
                {
                    target.effectDatas.Add(effectCopy);
                }
            }
        }

        energy -= item[i].itemCost;
        itemAmount[i]--;
        print($"[MoveItem] Завершено. Енергія після: {energy}, залишок предмету [{i}]: {itemAmount[i]}");
    }

    public void MoveDefens()
    {
        if (inventory.weaponData.defendClass.DefendCost <= energy)
        {
            def += inventory.weaponData.defendClass.defens;
            energy -= inventory.weaponData.defendClass.DefendCost;
            defendRech = inventory.weaponData.defendClass.defensRecharge;
            EffectRun(EffectType.typeDefend, def);
        }
    }

    public void TakeDamage(int dam, PlayerScr Attacker)
    {
        if (def < dam)
        {
            EffectRun(EffectType.typeTakeDamege, dam);
            manegerScr.GetComponent<BattleUIScr>().feedback.SpawnDamage(this, dam - def, false, Color.red);

            if (hitPointBonus > 0)
            {
                hitPointBonus -= dam - def;
            }
            else
            {
                hitPoint -= dam - def;
            }

            if (whoIm == EffectTarget.Player)
            {
                manegerScr.PlayerTakeDamage();
            }
            else
            {
                manegerScr.EnemyTakeDamage();
            }

        }
        else if (def >= dam)
        {
            if (Attacker != null)
            {
                EffectRun(EffectType.targetBlock, dam);
                print(def - dam);
                manegerScr.GetComponent<BattleUIScr>().feedback.SpawnText(this, "BLOCK", Color.gray);
            }
        }
        manegerScr.IsEndBattle(hitPoint, whoIm);

    }

    public void TakeFullDamage(int dam)
    {
        hitPoint -= dam;
        manegerScr.GetComponent<BattleUIScr>().feedback.SpawnDamage(this, dam, false, Color.red);
        EffectRun(EffectType.typeTakeDamege, dam);
    }

    public void TakeHealing(int heal)
    {
        hitPoint += heal;
        if (hitPoint > playerStats.vitalityMax + inventory.armorData.armorStatic.vitalityMax) hitPoint = playerStats.vitalityMax + inventory.armorData.armorStatic.vitalityMax;
        manegerScr.GetComponent<BattleUIScr>().feedback.SpawnDamage(this, heal, false, Color.green);
        EffectRun(EffectType.typeHealing, heal);
    }

    public void TakeShock(int shock)
    {
        energy -= shock;
        EffectRun(EffectType.typeShok, shock);
        manegerScr.GetComponent<BattleUIScr>().feedback.SpawnText(this, "SHOCK", Color.yellow);
    }

    public void TakeFrostbite(int frost)
    {
        playerBonusStats.staminaMax -= frost;
        EffectRun(EffectType.typeFrostbite, frost);
        manegerScr.GetComponent<BattleUIScr>().feedback.SpawnText(this, "FROSTBITE",  new Color(135, 206, 235));
    }

    public void NewMove()
    {

        for (int i = 0; i < effectDatas.Count; i++)
        {
            effectDatas[i].ActionEffect(gameObject.GetComponent<PlayerScr>());
            if (effectDatas[i].effectPower <= 0)
            {
                effectDatas.RemoveAt(i);
            }
        }

        defendRech--;
        attackRech--;
        def = 0;
        EffectRun(EffectType.typeConstant, 1);
        EnergyAdd();
    }

    public void EndMove()
    {
        playerBonusStats.Reset();
        damageBonus = 0;
    }

    public void EnergyAdd()
    {
        energy += playerStats.speed + playerBonusStats.speed + inventory.armorData.armorStatic.speed;
        int maxEnergy = playerStats.staminaMax + playerBonusStats.staminaMax + inventory.armorData.armorStatic.staminaMax;
        if (energy > maxEnergy) energy = maxEnergy;
    }

    public void EffectsRunArmor(PlayerScr targetEffect, PlayerScr target, int power)
    {
        for (int i = 0; i < inventory.armorData.effectItemClass.Count; i++)
        {
            var targetEffects = inventory.armorData.effectItemClass[i].targetEffect;
            var effectClass = inventory.armorData.effectItemClass[i].effectClass;


            if (!CheckTargetCondition(targetEffects, targetEffect))
                continue; 

            switch (targetEffects)
            {
                case TargetEffect.targetNone:
                case TargetEffect.targetBleeding:
                case TargetEffect.targetPoisoning:
                case TargetEffect.targetShock:
                case TargetEffect.targetFire:
                case TargetEffect.targetFrostbite:
                    RunEffectEnumArmor(target, power + inventory.armorData.effectItemClass[i].effectClass.power, i, targetEffect.effectDatas, inventory.armorData.effectItemClass[i].effectClass.effect.effectType);
                    break;

                case TargetEffect.targetDamage:
                    switch (inventory.weaponData.attackClass.attacType)
                    {
                        case AttacType.Slow:
                            RunEffectIntArmor(target, power + inventory.armorData.effectItemClass[i].effectClass.power, i, targetEffect.damageBonus + targetEffect.inventory.weaponData.attackClass.damage + targetEffect.playerStats.power + targetEffect.playerBonusStats.power + targetEffect.inventory.armorData.armorStatic.power);
                            break;

                        case AttacType.Fast:
                            RunEffectIntArmor(target, power + inventory.armorData.effectItemClass[i].effectClass.power, i, targetEffect.playerStats.power + targetEffect.playerBonusStats.power + targetEffect.inventory.armorData.armorStatic.power);
                            break;
                    }
                    break;

                case TargetEffect.targetHealing:
                    RunEffectIntArmor(target, power + hitPoint, i, targetEffect.damageBonus);
                    break;

                case TargetEffect.targetWeaponBonus:
                    RunEffectIntArmor(target, power + damageBonus, i, targetEffect.damageBonus);
                    break;

                case TargetEffect.targetHitPoint:
                    RunEffectIntArmor(target, power + hitPoint, i, targetEffect.hitPoint);
                    break;

                case TargetEffect.targetNoneHitPoint:
                    RunEffectIntArmor(target, power + inventory.armorData.effectItemClass[i].effectClass.power, i, (targetEffect.playerStats.vitalityMax + targetEffect.playerBonusStats.vitalityMax + targetEffect.inventory.armorData.armorStatic.vitalityMax) - targetEffect.hitPoint);

                    break;
            }
            print("Ефект працює2");

        }
    }

    private void RunEffectIntArmor(PlayerScr target, int power, int numer, int intEffect)
    {
        var effectClass = inventory.armorData.effectItemClass[numer].effectClass;
        int newPower = power + intEffect;

        // Якщо сила нижча за мінімальну — ефект не спрацьовує
        if (newPower < effectClass.powerMin)
            return;

        // Якщо сила більша за максимальну — обрізаємо
        if (newPower > effectClass.powerMax)
            newPower = effectClass.powerMax;

        // Накладаємо ефект на target
        EffectSetting existingEffect = target.effectDatas
            .FirstOrDefault(e => e.effectType == effectClass.effect.effectType);

        if (existingEffect != null)
        {
            // Якщо ефект вже є — додаємо до поточного
            existingEffect.effectPower += newPower;

            // Обмежуємо верхньою межею
            if (existingEffect.effectPower > effectClass.powerMax)
                existingEffect.effectPower = effectClass.powerMax;
        }
        else
        {
            // Якщо ефекту немає — створюємо новий
            EffectSetting newEffect = new EffectSetting()
            {
                effectType = effectClass.effect.effectType,
                effectPower = newPower
            };

            target.effectDatas.Add(newEffect);
        }
        print(effectClass.effect.effectType + " " + newPower);
        print("Ефект працює3");
    }

    private void RunEffectEnumArmor(PlayerScr target, int power, int numer, List<EffectSetting> effectsToApply, Effect tarEffect)
    {
        var effectClass = inventory.armorData.effectItemClass[numer].effectClass;
        int newPower = power;

        // Якщо сила нижча за мінімальну — ефект не спрацьовує
        if (newPower < effectClass.powerMin)
            return;

        // Якщо сила більша за максимальну — обрізаємо
        if (newPower > effectClass.powerMax)
            newPower = effectClass.powerMax;

        // Дивимось чи вже є ефект на target
        EffectSetting existing = target.effectDatas
            .FirstOrDefault(e => e.effectType == tarEffect);

        if (existing != null)
        {
            existing.effectPower += newPower;
            if (existing.effectPower > effectClass.powerMax)
                existing.effectPower = effectClass.powerMax;
        }
        else
        {
            target.effectDatas.Add(new EffectSetting()
            {
                effectType = tarEffect,
                effectPower = newPower
            });
        }
    }

    public void EffectsRunWapon(PlayerScr targetEffect, PlayerScr target, int power)
    {
        for (int i = 0; i < inventory.weaponData.effectItemClass.Count; i++)
        {
            var targetEffects = inventory.weaponData.effectItemClass[i].targetEffect;
            var effectClass = inventory.weaponData.effectItemClass[i].effectClass;

            if (!CheckTargetCondition(targetEffects, targetEffect))
                continue; 

            switch (targetEffects)
            {
                case TargetEffect.targetNone:
                case TargetEffect.targetBleeding:
                case TargetEffect.targetPoisoning:
                case TargetEffect.targetShock:
                case TargetEffect.targetFire:
                case TargetEffect.targetFrostbite:
                    RunEffectEnumWapon(target, power + inventory.weaponData.effectItemClass[i].effectClass.power, i, targetEffect.effectDatas, inventory.weaponData.effectItemClass[i].effectClass.effect.effectType);
                    break;

                case TargetEffect.targetDamage:
                    switch (inventory.weaponData.attackClass.attacType)
                    {
                        case AttacType.Slow:
                            RunEffectIntWapon(target, power + inventory.weaponData.effectItemClass[i].effectClass.power, i, targetEffect.damageBonus + targetEffect.inventory.weaponData.attackClass.damage + targetEffect.playerStats.power + targetEffect.playerBonusStats.power + targetEffect.inventory.armorData.armorStatic.power);
                            break;

                        case AttacType.Fast:
                            RunEffectIntWapon(target, power + inventory.weaponData.effectItemClass[i].effectClass.power, i, targetEffect.playerStats.power + targetEffect.playerBonusStats.power + targetEffect.inventory.armorData.armorStatic.power);
                            break;
                    }
                    break;

                case TargetEffect.targetHealing:
                    RunEffectIntWapon(target, power + hitPoint, i, targetEffect.damageBonus);
                    break;

                case TargetEffect.targetWeaponBonus:
                    RunEffectIntWapon(target, power + damageBonus, i, targetEffect.damageBonus);
                    break;

                case TargetEffect.targetHitPoint:
                    RunEffectIntWapon(target, power + hitPoint, i, targetEffect.hitPoint);
                    break;

                case TargetEffect.targetNoneHitPoint:
                    RunEffectIntWapon(target, power + inventory.weaponData.effectItemClass[i].effectClass.power, i, (targetEffect.playerStats.vitalityMax + targetEffect.playerBonusStats.vitalityMax + targetEffect.inventory.armorData.armorStatic.vitalityMax) - targetEffect.hitPoint);
                    break;
            }
            print("Ефект працює2");

        }
    }

    private void RunEffectIntWapon(PlayerScr target, int power, int numer, int intEffect)
    {
        var effectClass = inventory.weaponData.effectItemClass[numer].effectClass;
        int newPower = power + intEffect;

        // Якщо сила нижча за мінімальну — ефект не спрацьовує
        if (newPower < effectClass.powerMin)
            return;

        // Якщо сила більша за максимальну — обрізаємо
        if (newPower > effectClass.powerMax)
            newPower = effectClass.powerMax;

        // Накладаємо ефект на target
        EffectSetting existingEffect = target.effectDatas
            .FirstOrDefault(e => e.effectType == effectClass.effect.effectType);

        if (existingEffect != null)
        {
            // Якщо ефект вже є — додаємо до поточного
            existingEffect.effectPower += newPower;

            // Обмежуємо верхньою межею
            if (existingEffect.effectPower > effectClass.powerMax)
                existingEffect.effectPower = effectClass.powerMax;
        }
        else
        {
            // Якщо ефекту немає — створюємо новий
            EffectSetting newEffect = new EffectSetting()
            {
                effectType = effectClass.effect.effectType,
                effectPower = newPower
            };

            target.effectDatas.Add(newEffect.Clone());
        }
        print(effectClass.effect.effectType + " " + newPower);
    }

    private void RunEffectEnumWapon(PlayerScr target, int power, int numer, List<EffectSetting> effectsToApply, Effect tarEffect)
    {
        var effectClass = inventory.weaponData.effectItemClass[numer].effectClass;
        int newPower = power;

        // Якщо сила нижча за мінімальну — ефект не спрацьовує
        if (newPower < effectClass.powerMin)
            return;

        // Якщо сила більша за максимальну — обрізаємо
        if (newPower > effectClass.powerMax)
            newPower = effectClass.powerMax;

        // Дивимось чи вже є ефект на target
        EffectSetting existing = target.effectDatas
            .FirstOrDefault(e => e.effectType == tarEffect);

        if (existing != null)
        {
            existing.effectPower += newPower;
            if (existing.effectPower > effectClass.powerMax)
                existing.effectPower = effectClass.powerMax;
        }
        else
        {
            target.effectDatas.Add(new EffectSetting()
            {
                effectType = tarEffect,
                effectPower = newPower
            });
        }
    }

    private void EffectRun(EffectType effectType, int power)
    {
        PlayerScr player = null;
        PlayerScr enemy = null;
        if (whoIm == EffectTarget.Enemy)
        {
            player = manegerScr.enemy;
            enemy = manegerScr.player;
        }
        else if (whoIm == EffectTarget.Player)
        {
            player = manegerScr.player;
            enemy = manegerScr.enemy;
        }
        if (inventory.armorData.effectItemClass.Count > 0)
        {
            for (int i = 0; i < inventory.armorData.effectItemClass.Count; i++)
            {
                var targetEffects = inventory.armorData.effectItemClass[i].effectClass;
                if (effectType == targetEffects.effectType)
                {
                    if (targetEffects.acionTarget == EffectTarget.Enemy)
                    {
                        if (targetEffects.target == EffectTarget.Enemy)
                        {
                            EffectsRunArmor(enemy, enemy, power);

                        }
                        else
                        {
                            EffectsRunArmor(enemy, player, power);
                        }
                    }
                    else
                    {

                        if (targetEffects.target == EffectTarget.Enemy)
                        {
                            EffectsRunArmor(player, enemy, power);
                        }
                        else
                        {
                            EffectsRunArmor(player, player, power);
                        }
                    }
                }
            }
        }
        if (inventory.weaponData.effectItemClass.Count > 0)
        {
            for (int i = 0; i < inventory.weaponData.effectItemClass.Count; i++)
            {
                var targetEffects = inventory.weaponData.effectItemClass[i].effectClass;
                if (effectType == targetEffects.effectType)
                {
                    if (targetEffects.acionTarget == EffectTarget.Enemy)
                    {
                        if (targetEffects.target == EffectTarget.Enemy)
                        {
                            EffectsRunWapon(enemy, enemy, power);

                        }
                        else
                        {
                            EffectsRunWapon(enemy, player, power);
                        }
                    }
                    else
                    {

                        if (targetEffects.target == EffectTarget.Enemy)
                        {
                            EffectsRunWapon(player, enemy, power);
                        }
                        else
                        {
                            EffectsRunWapon(player, player, power);
                        }
                    }
                }
            }
        }
    }

    private bool CheckTargetCondition(TargetEffect condition, PlayerScr targetEffect)
    {
        switch (condition)
        {
            case TargetEffect.targetNone:
                return true; // завжди спрацьовує

            case TargetEffect.targetBleeding:
                return targetEffect.effectDatas
                    .Any(e => e.effectType == Effect.Bleeding); // ← або твій enum

            case TargetEffect.targetPoisoning:
                return targetEffect.effectDatas
                    .Any(e => e.effectType == Effect.Poisoning);

            case TargetEffect.targetShock:
                return targetEffect.effectDatas
                    .Any(e => e.effectType == Effect.Shock);

            case TargetEffect.targetFire:
                return targetEffect.effectDatas
                    .Any(e => e.effectType == Effect.Fire);

            case TargetEffect.targetFrostbite:
                return targetEffect.effectDatas
                    .Any(e => e.effectType == Effect.Frostbite);

            // Для числових умов — завжди true,
            // бо вони перевіряються через powerMin/powerMax
            case TargetEffect.targetDamage:
            case TargetEffect.targetHealing:
            case TargetEffect.targetHitPoint:
            case TargetEffect.targetNoneHitPoint:
            case TargetEffect.targetWeaponBonus:
                return true;

            default:
                return false;
        }
    }
}