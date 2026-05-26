using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIScr : MonoBehaviour
{
    public BattleManegerScr battleManeger;
    public UIFeedbackScr feedback;

    private PlayerScr player;
    public Image playerHPBar;
    public Image playerEnergyBar;
    public Text playerTextHP;
    public Text playerTextEnergy;
    public Text playerTextDefend;


    private PlayerScr enemy;
    public Image enemyHPBar;
    public Image enemyEnergyBar;
    public Text enemyTextHP;
    public Text enemyTextEnergy;
    public Text enemyTextDefend;

    private bool isReady = false;

    private void Start()
    {
        player = battleManeger.player;
        enemy = battleManeger.enemy;

        if (player == null || enemy == null)
        {
            Debug.LogError("UI: player або enemy NULL");
            return;
        }

        if (player.inventory == null || enemy.inventory == null)
        {
            Debug.LogError("UI: inventory NULL");
            return;
        }

        if (player.inventory.armorData == null)
            Debug.LogError("UI: player armor NULL");

        if (enemy.inventory.armorData == null)
            Debug.LogError("UI: enemy armor NULL");

        isReady = true;
        Debug.Log("UI READY");
    }

    private void Update()
    {
        if (!isReady) return;

        UpdateHP();
        UpdateEnergy();
        UpdateDefend();
    }
    private void UpdateHP()
    {
        float currentHP = player.hitPoint + player.hitPointBonus;
        float maxHP = player.playerStats.vitalityMax + player.playerBonusStats.vitalityMax + player.inventory.armorData.armorStatic.vitalityMax;

        playerHPBar.fillAmount = Mathf.Clamp01(currentHP / maxHP);
        playerTextHP.text = $"{player.hitPoint} (+{player.hitPointBonus}) / {maxHP}";



        float currentEnemyHP = enemy.hitPoint + enemy.hitPointBonus;
        float maxEnemyHP = enemy.playerStats.vitalityMax + enemy.playerBonusStats.vitalityMax + enemy.inventory.armorData.armorStatic.vitalityMax;

        enemyHPBar.fillAmount = Mathf.Clamp01(currentEnemyHP / maxEnemyHP);
        enemyTextHP.text = $"{currentEnemyHP} (+{enemy.hitPointBonus}) / {maxEnemyHP}";
    }

    private void UpdateEnergy()
    {
        float maxEnergy = player.playerStats.staminaMax + player.playerBonusStats.staminaMax + player.inventory.armorData.armorStatic.staminaMax;

        playerEnergyBar.fillAmount = Mathf.Clamp01(player.energy / maxEnergy);
        playerTextEnergy.text = $"{player.energy} (+{player.playerStats.speed + player.playerBonusStats.speed + player.inventory.armorData.armorStatic.speed}) / {maxEnergy}";



        float maxEnemyEnergy = enemy.playerStats.staminaMax + enemy.playerBonusStats.staminaMax + enemy.inventory.armorData.armorStatic.staminaMax;

        enemyEnergyBar.fillAmount = Mathf.Clamp01(enemy.energy / maxEnemyEnergy);
        enemyTextEnergy.text = $"{enemy.energy} (+{enemy.playerStats.speed + enemy.playerBonusStats.speed + enemy.inventory.armorData.armorStatic.speed}) / {maxEnemyEnergy}";
    }

    private void UpdateDefend()
    {
        playerTextDefend.text = player.def.ToString();

        enemyTextDefend.text = enemy.def.ToString();
    }

}
