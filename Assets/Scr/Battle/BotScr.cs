using UnityEngine;
using System.Collections;

public enum TargetRC
{
    Attack, Defend, Skip,
    Item0, Item1, Item2
}
// Додай до enum StaticRC:
public enum StaticRC
{
    None, Energy, Damage, HitPoint, Defend, DamageBonus, PlayerDef,     
}

[System.Serializable]
public class PriorityEffectRC
{
    public EffectTarget target;
    public Effect targetEffect;
    public float targetPower;
    public float powerRC;
    public TargetRC targetRC;
}

[System.Serializable]
public class PriorityStaticRC
{
    public EffectTarget target;
    public StaticRC targetStatic;
    public float targetPower;
    public float powerRC;
    public TargetRC targetRC;
}

public class BotScr : MonoBehaviour
{
    public BattleManegerScr battleScr;

    PlayerScr bot;
    PlayerScr player;

    public PriorityEffectRC[] priorityEffects;
    public PriorityStaticRC[] priorityStatic;

    [Header("Personality")]
    public float attackRC = 1f;
    public float defendRC = 1f;
    public float skipRC = 0.3f;

    [Header("Items Base RC")]
    public float[] itemRC = new float[3] { 0f, 0f, 0f };

    [Header("Timing")]
    public float startDelay = 0.5f;
    public float delayBetweenActions = 1.5f;


    // =============================================
    //  SATURATION — diminishing returns
    // =============================================
    [Header("Saturation")]
    [Tooltip("Множник зменшення score за кожне повторення тієї самої дії підряд. " +
             "0.55 = кожна наступна -45% від попереднього score.")]
    public float saturationDecay = 0.55f;

    [Tooltip("Мінімальний дозволений множник насичення.")]
    public float saturationFloor = 0.15f;

    float[] actionSaturation;
    TargetRC lastPerformedAction = TargetRC.Skip;

    // =============================================
    //  DEFEND CAP
    // =============================================
    [Header("Defend Cap")]
    [Tooltip("Якщо hitPointBonus бота >= vitalityMax * cap — захист майже марний (penalty -> 1).")]
    public float defendBonusCap01 = 0.6f;

    //================ ENTRY =================
    void Start()
    {
        InitSaturation(); 
        Init(battleScr.dataCarrier.allEnemy[battleScr.dataCarrier.bot]);
    }

    void InitSaturation()
    {
        int count = System.Enum.GetValues(typeof(TargetRC)).Length;
        actionSaturation = new float[count];
        for (int i = 0; i < count; i++)
            actionSaturation[i] = 1f;
        lastPerformedAction = TargetRC.Skip;
    }

    // Викликається на початку ходу — частково відновлює всі дії
    void ResetSaturation()
    {
        if (actionSaturation == null) { InitSaturation(); return; }

        for (int i = 0; i < actionSaturation.Length; i++)
        {
            // Поступово відновлюємо між ходами, але не до 1.0
            actionSaturation[i] = Mathf.Min(1f, actionSaturation[i] + 0.4f);
        }
    }
    public void OnTurn(int energyBot)
    {
        if (!battleScr.enemy.IsMove)
            return;

        bot    = battleScr.enemy;
        player = battleScr.player;

        if (player.hitPoint + player.hitPointBonus <= 0)
        {
            battleScr.NextTurn();
            return;
        }

        ResetSaturation();
        StartCoroutine(DecisionRoutine(energyBot));
    }

    //================ SATURATION =================


    void ApplySaturation(TargetRC action)
    {
        // Частково відновлюємо насичення попередньої дії
        if (lastPerformedAction != action && lastPerformedAction != TargetRC.Skip)
        {
            int prevIdx = (int)lastPerformedAction;
            actionSaturation[prevIdx] = Mathf.Min(1f, actionSaturation[prevIdx] + 0.3f);
        }

        int idx = (int)action;
        actionSaturation[idx] = Mathf.Max(
            saturationFloor,
            actionSaturation[idx] * saturationDecay
        );

        lastPerformedAction = action;

        Debug.Log("[Saturation] " + action + " => x" + actionSaturation[idx].ToString("F2"));
    }

    float GetSatMult(TargetRC action)
    {
        if (actionSaturation == null) return 1f;
        int idx = (int)action;
        if (idx < 0 || idx >= actionSaturation.Length) return 1f;
        return actionSaturation[idx];
    }

    //================ LOOP =================

    IEnumerator DecisionRoutine(int energy)
    {
        yield return new WaitForSeconds(startDelay);

        while (energy > 0 && bot.IsMove)
        {
            TargetRC best;
            float bestScore = EvaluateBestAction(out best, energy);

            Debug.Log("[Bot] BEST=" + best + " score=" + bestScore.ToString("F3") + " energy=" + energy);

            if (best == TargetRC.Skip)
                break;

            int cost = GetActionCost(best);
            if (energy < cost)
                break;

            PerformAction(best);
            ApplySaturation(best);
            energy -= cost;

            yield return new WaitForSeconds(delayBetweenActions);
        }

        battleScr.NextTurn();
    }

    //================ DECISION =================

    float EvaluateBestAction(out TargetRC best, int energy)
    {
        float skipScore   = SkipUtility();
        float attackScore = AttackUtility(energy);
        float defendScore = DefendUtility(energy);
        float item0Score  = ItemUtility(0, energy);
        float item1Score  = ItemUtility(1, energy);
        float item2Score  = ItemUtility(2, energy);

        best = TargetRC.Skip;
        float bestVal = skipScore;

        Check(ref best, ref bestVal, attackScore, TargetRC.Attack);
        Check(ref best, ref bestVal, defendScore, TargetRC.Defend);
        Check(ref best, ref bestVal, item0Score,  TargetRC.Item0);
        Check(ref best, ref bestVal, item1Score,  TargetRC.Item1);
        Check(ref best, ref bestVal, item2Score,  TargetRC.Item2);

        return bestVal;
    }

    //================ COST =================

    int GetActionCost(TargetRC action)
    {
        switch (action)
        {
            case TargetRC.Attack: return bot.inventory.weaponData.attackClass.attackCost;
            case TargetRC.Defend: return bot.inventory.weaponData.defendClass.DefendCost;
            case TargetRC.Item0:
            case TargetRC.Item1:
            case TargetRC.Item2: return 1;
        }
        return 0;
    }

    //================ EXECUTE =================

    void PerformAction(TargetRC action)
    {
        switch (action)
        {
            case TargetRC.Attack: battleScr.EnemyAttack(); break;
            case TargetRC.Defend: battleScr.EnemyDefend(); break;
            case TargetRC.Item0:
            case TargetRC.Item1:
            case TargetRC.Item2:
                battleScr.EnemyItem(action - TargetRC.Item0);
                break;
        }
    }

    //================ UTILITY =================

    // AttackUtility — ЧИСТИЙ, без вбудованої логіки:
    float AttackUtility(int energy)
    {
        if (bot.attackRech > 0) return -999f;
        if (energy < GetActionCost(TargetRC.Attack)) return -999f;

        // Лише базова personality — решта через Inspector
        float personality = attackRC;
        float staticBias = StaticBias(TargetRC.Attack);
        float effectBias = EffectBias(TargetRC.Attack);
        float sat = GetSatMult(TargetRC.Attack);

        float final = (personality + staticBias + effectBias) * sat;
        Debug.Log("[Attack] pers=" + personality.ToString("F3") +
                  " s=" + staticBias.ToString("F3") +
                  " e=" + effectBias.ToString("F3") +
                  " => " + final.ToString("F3"));
        return final;
    }

    // DefendUtility — теж ЧИСТИЙ:
    float DefendUtility(int energy)
    {
        if (bot.defendRech > 0) return -999f;
        if (energy < GetActionCost(TargetRC.Defend)) return -999f;

        float personality = defendRC;
        float staticBias = StaticBias(TargetRC.Defend);
        float effectBias = EffectBias(TargetRC.Defend);
        float sat = GetSatMult(TargetRC.Defend);
        float capPenalty = DefendCapPenalty();

        float final = (personality + staticBias + effectBias) * sat * (1f - capPenalty);
        Debug.Log("[Defend] capPen=" + capPenalty.ToString("F2") + " => " + final.ToString("F3"));
        return final;
    }

    // 0..1: наскільки hitPointBonus заповнено відносно cap
    float DefendCapPenalty()
    {
        if (bot.playerStats.vitalityMax <= 0) return 0f;
        float capValue = bot.playerStats.vitalityMax * defendBonusCap01;
        return Mathf.Clamp01(bot.hitPointBonus / Mathf.Max(1f, capValue));
    }

    float ItemUtility(int index, int energy)
    {
        if (bot.itemAmount == null)                    return -999f;
        if (index >= bot.itemAmount.Length)            return -999f;
        if (bot.itemAmount[index] <= 0)                return -999f;
        if (energy < 1)                                return -999f;

        float botWeakness = 1f - Hp01(bot);
        float personality = itemRC[index] + botWeakness * 0.6f;

        TargetRC rc = TargetRC.Item0 + index;
        float staticBias = StaticBias(rc);
        float effectBias = EffectBias(rc);
        float sat        = GetSatMult(rc);

        float final = (personality + staticBias + effectBias) * sat;

        Debug.Log("[" + rc + "] base=" + personality.ToString("F3") +
                  " sat=" + sat.ToString("F2") +
                  " => " + final.ToString("F3"));

        return final;
    }

    float SkipUtility()
    {
        // Skip — стабільний поріг, не насичується
        float final = skipRC + StaticBias(TargetRC.Skip) + EffectBias(TargetRC.Skip);
        Debug.Log("[Skip] => " + final.ToString("F3"));
        return final;
    }

    //================ BIAS =================

    float StaticBias(TargetRC rcType)
    {
        float bias = 0f;

        foreach (var pr in priorityStatic)
        {
            if (pr.targetRC != rcType) continue;

            PlayerScr target = pr.target == EffectTarget.Player ? player : bot;
            float dir = Mathf.Sign(pr.targetPower);
            float value01 = 0f;

            switch (pr.targetStatic)
            {
                case StaticRC.HitPoint:
                    value01 = dir > 0 ? Hp01(target) : 1f - Hp01(target);
                    break;

                case StaticRC.Energy:
                    value01 = dir > 0 ? Energy01(target) : 1f - Energy01(target);
                    break;

                case StaticRC.Defend:
                    float def01 = Mathf.Clamp01(target.def / 100f);
                    value01 = dir > 0 ? def01 : 1f - def01;
                    break;

                case StaticRC.Damage:
                    float dmg01 = Damage01(target, Mathf.Abs(pr.targetPower));
                    value01 = dir > 0 ? dmg01 : 1f - dmg01;
                    break;
                // У StaticBias додай нові case:
                case StaticRC.DamageBonus:
                    float maxBonus = Mathf.Max(1f, target.inventory.weaponData.attackClass.damage * 2f);
                    float bonus01 = Mathf.Clamp01(target.damageBonus / maxBonus);
                    value01 = dir > 0 ? bonus01 : 1f - bonus01;
                    break;

                case StaticRC.PlayerDef:
                    // Порівнюємо def цілі з очікуваним ударом бота
                    float expectedDmg = ExpectedDamage(bot);
                    float defRatio = Mathf.Clamp01(target.def / Mathf.Max(1f, expectedDmg));
                    value01 = dir > 0 ? defRatio : 1f - defRatio;
                    break;
            }

            float part = value01 * pr.powerRC;
            bias += part;

            Debug.Log("[StaticBias " + rcType + "] " + pr.targetStatic +
                      " part=" + part.ToString("F3"));
        }

        return bias;
    }
    public void Init(BotData data)
    {
        print("Значення завантажені");
        attackRC = data.attackRC;
        defendRC = data.defendRC;
        skipRC = data.skipRC;

        itemRC = (float[])data.itemRC.Clone();

        priorityEffects = (PriorityEffectRC[])data.priorityEffects.Clone();

        priorityStatic = (PriorityStaticRC[])data.priorityStatic.Clone();
        print("Значення завантажені 2");
    }
    // Новий хелпер:
    float ExpectedDamage(PlayerScr attacker)
    {
        var attack = attacker.inventory.weaponData.attackClass;
        float power = attacker.playerStats.power
                    + attacker.playerBonusStats.power
                    + attacker.damageBonus;
        return attack.attacType == AttacType.Slow
            ? attack.damage + power
            : attack.damage * power;
    }
    float EffectBias(TargetRC rcType)
    {
        float bias = 0f;

        foreach (var pr in priorityEffects)
        {
            if (pr.targetRC != rcType) continue;

            PlayerScr target = pr.target == EffectTarget.Player ? player : bot;

            foreach (var eff in target.effectDatas)
            {
                if (eff.effectType != pr.targetEffect) continue;

                float value01 = Mathf.Clamp01(
                    eff.effectPower / Mathf.Abs(pr.targetPower)
                );

                float part = value01 * pr.powerRC;
                bias += part;

                Debug.Log("[EffectBias " + rcType + "] " + eff.effectType +
                          " part=" + part.ToString("F3"));
            }
        }

        return bias;
    }

    //================ NORMALIZATION =================

    // ВИПРАВЛЕНО: Slow = damage + power (один удар із бонусом)
    //             Multi = damage * power (damage = кількість ударів)
    float Damage01(PlayerScr target, float scale)
    {
        if (scale <= 0f) return 0f;

        var attack = target.inventory.weaponData.attackClass;

        float powerBase =
            target.playerStats.power +
            target.playerBonusStats.power +
            target.damageBonus;

        float total = attack.attacType == AttacType.Slow
            ? attack.damage + powerBase          // один сильний удар
            : attack.damage * powerBase;         // N ударів по powerBase кожен

        return Mathf.Clamp01(total / scale);
    }

    float Hp01(PlayerScr p)
    {
        if (p.playerStats.vitalityMax <= 0) return 0f;
        return Mathf.Clamp01(p.hitPoint / (float)p.playerStats.vitalityMax);
    }
    float Energy01(PlayerScr p)
    {
        int maxEnergy = p.playerStats.staminaMax + p.inventory.armorData.armorStatic.staminaMax; // або як у тебе зберігається макс
        if (maxEnergy <= 0) return 0f;
        return Mathf.Clamp01(p.energy / (float)maxEnergy);
    }

    // Відносна кількість залишкового AP у поточному ході
    float EnergyRatio01(int energyLeft)
    {
        int maxEnergy = bot.energy + bot.playerBonusStats.speed;
        if (maxEnergy <= 0) return 0f;
        return Mathf.Clamp01(energyLeft / (float)maxEnergy);
    }

    //================ HELPER =================

    void Check(ref TargetRC best, ref float bestVal, float val, TargetRC rc)
    {
        if (val > bestVal)
        {
            bestVal = val;
            best    = rc;
        }
    }
}