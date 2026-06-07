using UnityEngine;
using System.Collections;

public class PlayerMoveScr : MonoBehaviour
{
    public InventoryScr inventoryScr;

    [Header("UI")]
    public GameObject[] panelUI;

    [Header("Player")]
    public Transform player;

    [Header("World")]
    public WorldMapGeneratorScr worldMapGenerator;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;

    [Header("Runtime")]
    public PointScr currentPoint;

    private static int savedPointId = -1;

    private bool isMoving;

    // ─────────────────────────────────────────
    // EVENTS
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        PointScr.OnPointClicked += HandlePointClicked;
    }

    private void OnDisable()
    {
        PointScr.OnPointClicked -= HandlePointClicked;
    }

    // ─────────────────────────────────────────
    // START
    // ─────────────────────────────────────────

    private IEnumerator Start()
    {
        // Чекаємо поки сцена та карта стабілізуються
        yield return null;
        yield return null;

        // Автоматично підключаємось
        if (worldMapGenerator == null)
        {
            worldMapGenerator =
                WorldMapGeneratorScr.Instance;
        }

        // Якщо singleton ще не готовий
        while (worldMapGenerator == null)
        {
            yield return null;

            worldMapGenerator =
                WorldMapGeneratorScr.Instance;
        }

        worldMapGenerator.SetMapVisible(true);

        RestoreOrFindStartPoint();
    }

    // ─────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            InteractWithCurrentPoint();
        }

        Inventory();
    }

    // ─────────────────────────────────────────
    // INTERACTION
    // ─────────────────────────────────────────

    public void InteractWithCurrentPoint()
    {
        if (currentPoint == null)
            return;

        if (isMoving)
            return;

        currentPoint.Interact();
    }

    // ─────────────────────────────────────────
    // SAVE / LOAD
    // ─────────────────────────────────────────

    public void SaveCurrentPoint()
    {
        if (currentPoint == null)
            return;

        savedPointId = currentPoint.id;

        Debug.Log("Saved Point ID: " + savedPointId);
    }

    public void OnMapRegenerated()
    {
        savedPointId = -1;

        currentPoint = null;

        RestoreOrFindStartPoint();
    }

    private void RestoreOrFindStartPoint()
    {
        PointScr restoredPoint = null;

        if (savedPointId >= 0)
        {
            restoredPoint = FindPointById(savedPointId);

            Debug.Log("Trying restore point: " + savedPointId);
        }

        if (restoredPoint == null)
        {
            restoredPoint = FindClosestPoint();

            Debug.Log("Fallback to closest point");
        }

        currentPoint = restoredPoint;

        if (currentPoint == null)
        {
            Debug.LogError("CurrentPoint STILL NULL");
            return;
        }

        player.position =
            currentPoint.transform.position;

        worldMapGenerator.NoSelectPoints();

        currentPoint.SelectPoint(true);

        SaveCurrentPoint();

        RefreshAvailablePoints();

        Debug.Log("Restored point: " + currentPoint.id);
    }

    // ─────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────

    private void HandlePointClicked(PointScr targetPoint)
    {
        if (isMoving)
            return;

        if (currentPoint == null)
            return;

        // БЛОКУВАННЯ РУХУ ПІД ЧАС БОЮ
        if (currentPoint.pointType == pointType.enemy)
        {
            Debug.Log("Не можна рухатись під час бою!");
            return;
        }

        if (targetPoint == currentPoint)
            return;

        bool hasDirectConnection =
            currentPoint.connections.Contains(targetPoint);

        if (!hasDirectConnection)
        {
            Debug.Log("Немає прямого шляху!");
            return;
        }

        CloseAllZoneUI();

        worldMapGenerator.NoSelectPoints();

        StartCoroutine(MoveToPoint(targetPoint));
    }

    private IEnumerator MoveToPoint(PointScr targetPoint)
    {
        isMoving = true;

        Vector3 start = player.position;
        Vector3 end = targetPoint.transform.position;

        float distance =
            Vector3.Distance(start, end);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime *
                 moveSpeed / distance;

            player.position =
                Vector3.Lerp(start, end, t);

            yield return null;
        }

        player.position = end;

        currentPoint = targetPoint;

        worldMapGenerator.NoSelectPoints();

        currentPoint.SelectPoint(true);

        SaveCurrentPoint();

        RefreshAvailablePoints();

        isMoving = false;
    }

    // ─────────────────────────────────────────
    // VISUAL
    // ─────────────────────────────────────────

    private void RefreshAvailablePoints()
    {
        PointScr[] allPoints =
            FindObjectsOfType<PointScr>();

        foreach (PointScr p in allPoints)
        {
            p.SetDefault();
        }

        if (currentPoint == null)
            return;

        currentPoint.SetCurrent();

        foreach (PointScr connected
                 in currentPoint.connections)
        {
            connected.SetAvailable();
        }
    }

    private PointScr FindPointById(int id)
    {
        PointScr[] points =
            FindObjectsOfType<PointScr>(true);

        foreach (PointScr p in points)
        {
            if (p.id == id)
            {
                return p;
            }
        }

        return null;
    }

    private PointScr FindClosestPoint()
    {
        PointScr[] points =
            FindObjectsOfType<PointScr>(true);

        PointScr closest = null;

        float bestDist = float.MaxValue;

        foreach (PointScr p in points)
        {
            float dist =
                Vector3.Distance(
                    player.position,
                    p.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                closest = p;
            }
        }

        return closest;
    }
    private void CloseAllZoneUI()
    {
        NPCZoneUIScr zoneUI =
            FindObjectOfType<NPCZoneUIScr>(true);

        if (zoneUI == null)
            return;

        if (zoneUI.SelectNPCPanel != null)
            zoneUI.SelectNPCPanel.SetActive(false);

        if (zoneUI.UIPanel != null)
            zoneUI.UIPanel.SetActive(false);

        if (zoneUI.UIPanelShop != null)
            zoneUI.UIPanelShop.SetActive(false);
    }

    // ─────────────────────────────────────────
    // INVENTORY
    // ─────────────────────────────────────────

    private void Inventory()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (panelUI[0].activeSelf)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
    }
    public void OpenCurrentShop()
    {
        if (currentPoint == null)
            return;

        if (currentPoint.pointType != pointType.city)
            return;

        if (currentPoint.city == null)
            return;

        currentPoint.city.OpenShop();
    }
    public void OpenInventory()
    {
        panelUI[0].SetActive(true);
        inventoryScr.UpdateInventoryUI();
    }

    public void CloseInventory()
    {
        panelUI[0].SetActive(false);
    }
}