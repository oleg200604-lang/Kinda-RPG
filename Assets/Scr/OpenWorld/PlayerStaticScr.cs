using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class PlayerStaticScr : MonoBehaviour
{
    public static Static playerStatic;
    public InventoryData inventoryData;
    public BattleDataCarrier itemDatabase;
    public TextMeshProUGUI[] textMeshPro;

    public int powerLv, recoveryLv;
    public int staminaMaxLv;
    public int hitPointMaxLv;
    public int pointUpdate;

    private void Awake()
    {
        playerStatic = inventoryData.staticPlayer;
        LvUp();
    }

    public void UpdatePower()
    {
        if (pointUpdate < 1) return;
        powerLv++;
        playerStatic.power = powerLv;
        pointUpdate--;
        inventoryData.staticPlayer = playerStatic;
    }

    public void UpdateSpeed()
    {
        if (pointUpdate < 2) return;
        recoveryLv++;
        playerStatic.speed = 1 + recoveryLv;
        pointUpdate -= 2;
        inventoryData.staticPlayer = playerStatic;
    }

    public void UpdateStamina()
    {
        if (pointUpdate < 2) return;
        staminaMaxLv++;
        playerStatic.staminaMax = 3 + staminaMaxLv;
        pointUpdate -= 2;
        inventoryData.staticPlayer = playerStatic;
    }

    public void UpdateVitality()
    {
        if (pointUpdate < 1) return;
        hitPointMaxLv++;
        playerStatic.vitalityMax = 10 + hitPointMaxLv;
        pointUpdate--;
        inventoryData.staticPlayer = playerStatic;
    }

    public void LvUp()
    {
        //if (inventoryData.XP >= inventoryData.XPMax[inventoryData.Lv])
        {
            //inventoryData.XP -= inventoryData.XPMax[inventoryData.Lv];
            //inventoryData.Lv++;
            //pointUpdate++;
        }
    }

    public void ResetStatic()
    {
        pointUpdate += powerLv + recoveryLv * 2 + staminaMaxLv * 2 + hitPointMaxLv;
        powerLv = recoveryLv = staminaMaxLv = hitPointMaxLv = 0;

        playerStatic.power = 0;
        playerStatic.speed = 1;
        playerStatic.staminaMax = 3;
        playerStatic.vitalityMax = 10;
        inventoryData.staticPlayer = playerStatic;
    }

    private void Update()
    {

        if (inventoryData.armor != -1)
        {
            if (itemDatabase.allItems[inventoryData.armor] is ArmorData armors)
            {
                textMeshPro[0].text = FormatStat(playerStatic.power, armors.armorStatic.power);
                textMeshPro[1].text = FormatStat(playerStatic.speed, armors.armorStatic.speed);
                textMeshPro[2].text = FormatStat(playerStatic.staminaMax, armors.armorStatic.staminaMax);
                textMeshPro[3].text = FormatStat(playerStatic.vitalityMax, armors.armorStatic.vitalityMax);
            }
        }
    }

    private string FormatStat(int base_, int bonus) =>
        bonus > 0
            ? $"{base_} +{bonus}"
            : $"{base_} {bonus}";
}