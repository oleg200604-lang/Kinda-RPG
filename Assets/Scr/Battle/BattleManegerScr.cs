using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class EffectUI
{
    public GameObject effectPanel;
    public TextMeshProUGUI effectText;
}
public class BattleManegerScr : MonoBehaviour
{
    public Animator anim;
    public GameObject enemyTexture;
    public GameObject playerTexture;
    public PlayerScr enemy;
    public static BotData botData;
    public BotScr botScr;
    public PlayerScr player;
    public InventoryData playerInventoryData;
    public InventoryData enemyInventoryData;
    public EffectUI[] playerEffect;
    public EffectUI[] enemyEffect;
    public Image[] imagesItem;
    public GameObject[] panelEndGame;
    public GameObject[] slotUI;
    public PlayrtAnimatiorScr[] playerAnimatorScr;
    public SlotUIScr slotVictoryUIScr;
    [SerializeField] private ItemData noneItem;
    [SerializeField] private int remuneration;
    public BattleDataCarrier dataCarrier;
    private void Awake()
    {
        var carrier = BattleDataCarrier.Instance;
        dataCarrier = carrier;

        playerInventoryData = dataCarrier.playerInventoryData;
        enemyInventoryData = carrier.enemyInventoryData;
        botData = carrier.allEnemy[carrier.bot];
        player.inventory.InitInventory(playerInventoryData, dataCarrier);
        enemy.inventory.InitInventory(enemyInventoryData, dataCarrier);

        player.playerStats = playerInventoryData.staticPlayer;
        enemy.playerStats = enemyInventoryData.staticPlayer;

        player.hitPoint = player.playerStats.vitalityMax + player.inventory.armorData.armorStatic.vitalityMax;
        enemy.hitPoint = enemy.playerStats.vitalityMax + enemy.inventory.armorData.armorStatic.vitalityMax;

        player.energy = player.playerStats.staminaMax + player.inventory.armorData.armorStatic.staminaMax;
        enemy.energy = enemy.playerStats.staminaMax + enemy.inventory.armorData.armorStatic.staminaMax;

        player.item = player.inventory.itemData;

        playerTexture.GetComponent<PlayrtAnimatiorScr>().playerSprite = dataCarrier.playerInventoryData.animatorSprite;
        enemyTexture.GetComponent<PlayrtAnimatiorScr>().playerSprite = dataCarrier.enemyInventoryData.animatorSprite;

        remuneration = carrier.allEnemy[carrier.bot].remuneration;

        for (int i = 0; i < player.item.Length; i++)
        {
            if (player.item[i] == null || player.item[i] == noneItem)
            {
                slotUI[i].SetActive(false);
                if (dataCarrier.allItems[playerInventoryData.items[i]] is ItemData item)
                {
                    imagesItem[i].sprite = item.iconSprite;
                }
                Debug.LogError($"ITEM NULL на індексі {i}");
            }
            else
            {
                //slotUI[i].SetActive(true);
            }
        }


        botScr.Init(botData);

        Debug.Log("BattleManager INIT OK");
    }

    public void NextTurn()
    {
        anim.SetTrigger("NextTurn");
        if (player.IsMove)
        {
            player.EndMove();

            enemy.NewMove();
            player.IsMove = false;
            enemy.IsMove = true;
            enemy.GetComponent<BotScr>().OnTurn(enemy.energy);
            
        }
        else
        {
            enemy.EndMove();
            player.NewMove();

            enemy.IsMove = false;
            player.IsMove = true;
        }
        UpdateEffect();
    }

    /*private void Update()
    {
        if (battleEnded) return;

        if (player != null && player.hitPoint <= 0)
        {
            EndBattle(false);
            return; // ← додати return щоб не перевіряти enemy після
        }

        if (enemy != null && enemy.hitPoint <= 0)
        {
            EndBattle(true);
            return;
        }
    }*/
    public void IsEndBattle(int hitPoint, EffectTarget whoIm)
    {

        if (hitPoint<=0)
        {
            if (whoIm ==EffectTarget.Player)
            {
                EndBattle(false);
            }
            else
            {
                EndBattle(true);
            }
        }
    }

    private void EndBattle(bool won)
    {

        if (won)
        {
            enemyTexture.GetComponent<Animator>().SetTrigger("IsDefeat");
            playerTexture.GetComponent<Animator>().SetTrigger("IsVictory");
            slotVictoryUIScr.scriptableObject = dataCarrier.allItems[remuneration];
            slotVictoryUIScr.UpdateSlot();
            print("Отриманий предмет " + dataCarrier.allItems[remuneration]);
            panelEndGame[1].SetActive(true);
        }
        else
        {
            playerTexture.GetComponent<Animator>().SetTrigger("IsDefeat");
            enemyTexture.GetComponent<Animator>().SetTrigger("IsVictory");
            panelEndGame[0].SetActive(true);
        }
    }
    public void ExitBattle(bool playerWon)
    {
        if (playerWon == true)
        {
            playerInventoryData.AddInventory(remuneration);

            
            //KillQuestCheck();
            playerInventoryData.hitPoint = player.hitPoint;


            if (enemyInventoryData != null)
                playerInventoryData.XP += enemyInventoryData.XP;

            SaveSystem.Save(playerInventoryData);
            SceneManager.LoadScene(1);
        }
        else
        {
            //playerInventoryData.ResrtHitPoint();
            SaveSystem.DeleteSave();
            SceneManager.LoadScene(1);
        }


    }

    private void KillQuestCheck()
    {
        //for (int i=0; i < playerInventoryData.questKills.Count; i++)
        {
            //if (playerInventoryData.questKills[i].questKillData.questTypeKill == botData.typeKill)
            {
                //playerInventoryData.questKills[i].questCurrentAmount++;
            }
            
        }
    }

    public void PlayerTakeDamage()
    {
        playerTexture.GetComponent<Animator>().SetTrigger("TakeDamage");

        UpdateEffect();
    }

    public void EnemyTakeDamage()
    {
        enemyTexture.GetComponent<Animator>().SetTrigger("TakeDamage");

        UpdateEffect();
    }

    public void PlayerAttack()
    {
        playerTexture.GetComponent<Animator>().SetTrigger("Attack");

        UpdateEffect();
    }

    public void EnemyAttack()
    {
        enemyTexture.GetComponent<Animator>().SetTrigger("Attack");
        
        UpdateEffect();
    }

    public void PlayerDefend()
    {
        playerTexture.GetComponent<Animator>().SetTrigger("Defend");

        UpdateEffect();
    }

    public void EnemyDefend()
    {
        enemyTexture.GetComponent<Animator>().SetTrigger("Defend");


        UpdateEffect();
    }

    public void PlayerItem(int num) 
    {
        playerTexture.GetComponent<Animator>().SetTrigger("UseItem");
        playerTexture.GetComponent<PlayrtAnimatiorScr>().numItem = num;

        UpdateEffect();
    }

    public void EnemyItem(int num)
    {
        enemyTexture.GetComponent<Animator>().SetTrigger("UseItem");
        enemyTexture.GetComponent<PlayrtAnimatiorScr>().numItem = num;

        UpdateEffect();
    }

    public void UpdateEffect()
    {
        // вимикаємо всі ефекти гравця
        for (int i = 0; i < playerEffect.Length; i++)
        {
            playerEffect[i].effectPanel.SetActive(false);
        }

        for (int i = 0; player.effectDatas.Count > i; i++) 
        {
            switch (player.effectDatas[i].effectType)
            {
                case Effect.Fire:
                    playerEffect[0].effectPanel.SetActive(true);
                    playerEffect[0].effectText.text  = player.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Frostbite:
                    playerEffect[1].effectPanel.SetActive(true);
                    playerEffect[1].effectText.text = player.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Shock:
                    playerEffect[2].effectPanel.SetActive(true);
                    playerEffect[2].effectText.text = player.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Poisoning:
                    playerEffect[3].effectPanel.SetActive(true);
                    playerEffect[3].effectText.text = player.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Bleeding:
                    playerEffect[4].effectPanel.SetActive(true);
                    playerEffect[4].effectText.text = player.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Healing:
                    playerEffect[5].effectPanel.SetActive(true);
                    playerEffect[5].effectText.text = player.effectDatas[i].effectPower.ToString();
                    break;
            }
        }

        // вимикаємо всі ефекти гравця
        for (int i = 0; i < enemyEffect.Length; i++)
        {
            enemyEffect[i].effectPanel.SetActive(false);
        }

        for (int i = 0; enemy.effectDatas.Count > i; i++)
        {
            switch (enemy.effectDatas[i].effectType)
            {
                case Effect.Fire:
                    enemyEffect[0].effectPanel.SetActive(true);
                    enemyEffect[0].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Frostbite:
                    enemyEffect[1].effectPanel.SetActive(true);
                    enemyEffect[1].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Shock:
                    enemyEffect[2].effectPanel.SetActive(true);
                    enemyEffect[2].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Poisoning:
                    enemyEffect[3].effectPanel.SetActive(true);
                    enemyEffect[3].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Bleeding:
                    enemyEffect[4].effectPanel.SetActive(true);
                    enemyEffect[4].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
                case Effect.Healing:
                    enemyEffect[5].effectPanel.SetActive(true);
                    enemyEffect[5].effectText.text = enemy.effectDatas[i].effectPower.ToString();
                    break;
            }
        }
    }
}
