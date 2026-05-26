using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;

public class PlayrtAnimatiorScr : MonoBehaviour
{
    public int spriteNum;
    public bool isPlayer;
    public int numItem;
    public Image playerImage;
    public Sprite[] playerSprite;
    public BattleManegerScr battleManegerScr;

    void Update()
    {
        playerImage.sprite = playerSprite[spriteNum];
    }
    public void AttackAnim()
    {
        print("Attack");
        if (isPlayer == true)
        {
            battleManegerScr.player.MoveAttack(battleManegerScr.enemy);
        }
        else
        {
            battleManegerScr.enemy.MoveAttack(battleManegerScr.player);
        }
        battleManegerScr.UpdateEffect();
    }

    public void DefendAnim()
    {
        print("Defend");
        if (isPlayer == true)
        {
            battleManegerScr.player.MoveDefens();
        }
        else
        {
            battleManegerScr.enemy.MoveDefens();
        }
        battleManegerScr.UpdateEffect();
    }

    public void useItemAnim()
    {
        print("Item");
        if (isPlayer == true)
        {

            battleManegerScr.player.MoveItem(numItem);
        }

        else
        {
            battleManegerScr.enemy.MoveItem(numItem);
        }
        battleManegerScr.UpdateEffect();
    }

    
}
