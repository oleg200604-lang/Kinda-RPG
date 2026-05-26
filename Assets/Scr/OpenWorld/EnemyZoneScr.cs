using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyZoneScr : MonoBehaviour
{
    public NPCZoneUIScr zoneUI;

    public InventoryData[] inventoryDatas;

    public int[] botDatas;

    [HideInInspector]
    public bool isZoneNPC;

    public PointScr point;

    public void StartBattle()
    {
        var carrier = BattleDataCarrier.Instance;

        if (carrier == null)
        {
            Debug.LogError("Carrier is null");
            return;
        }

        int randomIndex = Random.Range(0, inventoryDatas.Length);

        PlayerMoveScr playerMove =
            FindObjectOfType<PlayerMoveScr>();

        if (playerMove == null)
        {
            Debug.LogError("PlayerMoveScr null");
            return;
        }

        if (playerMove.inventoryScr == null)
        {
            Debug.LogError("InventoryScr null");
            return;
        }

        carrier.playerInventoryData = playerMove.inventoryScr.sharedInventory;

        carrier.enemyInventoryData = inventoryDatas[randomIndex];

        carrier.bot = botDatas[randomIndex];

        playerMove.SaveCurrentPoint();

        if (WorldMapGeneratorScr.Instance != null)
        {
            WorldMapGeneratorScr.Instance.SetMapVisible(false);
        }

        SceneManager.LoadScene(2);
    }
}