using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class BattleDataCarrier : MonoBehaviour
{
    public static BattleDataCarrier Instance;
    public ScriptableObject[] allItems;
    public BotData[] allEnemy;
    public InventoryData playerInventoryData;
    public InventoryData enemyInventoryData;
    public int remuneration;
    public int bot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Переносимо allItems якщо новий об'єкт має дані
            if (allItems != null && allItems.Length > 0)
                Instance.allItems = allItems;

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
