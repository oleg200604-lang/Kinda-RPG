using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMapGeneratorScr : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject pointPrefab;
    public GameObject linePrefab;

    [Header("Generation Settings")]
    public int maxPoints = 25;
    public int maxDepth = 4;
    [Range(1, 3)] public int minChildren = 1;
    [Range(1, 3)] public int maxChildren = 3;

    [Header("Layout — Radial")]
    public float ringSpacing = 2.2f;
    [Range(10f, 90f)] public float arcSpreadDeg = 60f;
    public float positionJitter = 0.15f;
    public float minSpacing = 1.6f;
    public int maxPlacementRetries = 14;
    [Range(5f, 45f)] public float minSiblingAngleDeg = 25f;

    [Header("Phase 1 — Guarantees")]
    [Tooltip("Мінімум дітей для точок на depth 0 і 1 (щоб уникнути ланцюжків)")]
    public int minChildrenForEarlyDepths = 2;
    [Tooltip("До якого depth діє посилений мінімум")]
    public int earlyDepthThreshold = 1;

    [Header("Phase 2 — Extra Routes")]
    public int neighborCandidates = 4;
    [Range(0f, 1f)] public float extraRouteChance = 0.3f;
    public float maxNeighborDistance = 4.5f;
    public int maxDepthDiff = 2;
    public int maxConnectionsPerPoint = 4;
    [Tooltip("Мінімальна відстань від нового ребра до будь-якої точки що не є його кінцями")]
    public float minEdgePointClearance = 0.7f;

    [Header("Location Generation")]
    [Tooltip("Один Boss або більше в точках максимальної глибини")]
    public bool multipleBossesAtMaxDepth = false;
    [Tooltip("Скільки ворогів між містами (включно: місто з'являється кожні N ворогів)")]
    public int enemiesPerCity = 3;
    [Tooltip("Шанс що точка стане none замість enemy (0 = тільки вороги, 1 = тільки none)")]
    [Range(0f, 0.5f)] public float noneChance = 0.2f;

    [Header("Visuals")]
    public Color lineColor = new Color(0.6f, 0.8f, 1f, 0.8f);
    public Color extraRouteColor = new Color(1f, 0.75f, 0.3f, 0.6f);

    // ─── Runtime ───────────────────────────────────────────────────

    private List<PointScr> allPoints = new List<PointScr>();
    private List<(Vector2 a, Vector2 b)> allEdges = new List<(Vector2, Vector2)>();
    private int nextId = 0;
    private Transform pointsRoot;
    private Transform linesRoot;

    public PlayerMoveScr mapNavigator;

    public BattleDataCarrier dataCarrier;
    public NPCZoneUIScr zoneUI;
    public int[] enemySpawnForests;

    // ─── Singleton / DontDestroyOnLoad ─────────────────────────────

    public static WorldMapGeneratorScr Instance { get; private set; }

    private void Awake()
    {
        // Якщо інший екземпляр вже існує — знищуємо себе
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GenerateMap();
    }

    // ─── Public API ────────────────────────────────────────────────
    
    public void NoSelectPoints()
    {
        for (int i = 0; i < allPoints.Count; i++)
            allPoints[i].SelectPoint(false);
    }

    /// <summary>
    /// Знищує поточний світ і генерує новий з нуля.
    /// Викликай цей метод коли потрібен свіжий рун.
    /// </summary>
    public void ResetAndRegenerateMap()
    {
        GenerateMap();

        // Повідомляємо навігатор про те, що карта змінилась —
        // він сам знайде стартову точку і перемістить гравця.
        if (mapNavigator != null)
            mapNavigator.OnMapRegenerated();
    }

    public void GenerateMap()
    {
        foreach (var p in allPoints)
            if (p) Destroy(p.gameObject);
        allPoints.Clear();
        allEdges.Clear();
        nextId = 0;

        pointsRoot = GetOrCreateRoot("Points");
        linesRoot = GetOrCreateRoot("Lines");

        PhaseOne_BuildSpanningTree();
        PhaseTwo_AddExtraRoutes();
        PhaseThree_AssignLocationTypes();

        Debug.Log($"[WorldMap] {allPoints.Count} точок, {allEdges.Count} ребер");
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE 1
    // ═══════════════════════════════════════════════════════════════

    private void PhaseOne_BuildSpanningTree()
    {
        PointScr start = SpawnPoint(Vector3.zero, 0);

        var queue = new Queue<(PointScr point, Vector2 inDir)>();
        queue.Enqueue((start, Vector2.zero));

        while (queue.Count > 0 && allPoints.Count < maxPoints)
        {
            var (current, inDir) = queue.Dequeue();
            if (current.depth >= maxDepth) continue;

            int effectiveMin = current.depth <= earlyDepthThreshold
                ? Mathf.Max(minChildren, minChildrenForEarlyDepths)
                : minChildren;

            int childCount = Mathf.Min(
                Random.Range(effectiveMin, maxChildren + 1),
                maxPoints - allPoints.Count);

            List<float> baseAngles = ComputeBaseAngles(inDir, childCount);
            var usedAngles = new List<float>();

            for (int i = 0; i < childCount; i++)
            {
                Vector3? pos = TryPlaceChild(
                    current.transform.position,
                    baseAngles[i],
                    usedAngles,
                    out float finalAngle);

                if (!pos.HasValue) continue;

                usedAngles.Add(finalAngle);

                PointScr child = SpawnPoint(pos.Value, current.depth + 1);
                AddEdge(current, child, lineColor);

                queue.Enqueue((child, AngleToDir(finalAngle)));
            }
        }
    }

    private List<float> ComputeBaseAngles(Vector2 inDir, int count)
    {
        var angles = new List<float>();

        if (inDir == Vector2.zero)
        {
            float startOffset = Random.Range(0f, 360f);
            for (int i = 0; i < count; i++)
                angles.Add(startOffset + i * (360f / count));
        }
        else
        {
            float baseAngle = Mathf.Atan2(inDir.y, inDir.x) * Mathf.Rad2Deg;
            if (count == 1)
            {
                angles.Add(baseAngle);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    angles.Add(baseAngle + Mathf.Lerp(-arcSpreadDeg, arcSpreadDeg,
                                                       (float)i / (count - 1)));
            }
        }

        return angles;
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE 2
    // ═══════════════════════════════════════════════════════════════

    private void PhaseTwo_AddExtraRoutes()
    {
        var shuffled = new List<PointScr>(allPoints);
        Shuffle(shuffled);

        foreach (PointScr pt in shuffled)
        {
            var candidates = GetNeighborCandidates(pt);

            foreach (PointScr neighbor in candidates)
            {
                if (!CanConnect(pt, neighbor)) continue;
                if (Random.value > extraRouteChance) continue;

                Vector2 a = V2(pt.transform.position);
                Vector2 b = V2(neighbor.transform.position);

                if (EdgeIntersectsAny(a, b, pt.transform.position)) continue;
                if (EdgeTooCloseToPoints(a, b, pt, neighbor)) continue;

                AddEdge(pt, neighbor, extraRouteColor);
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PHASE 3 — Assign Location Types
    // ═══════════════════════════════════════════════════════════════

    private void PhaseThree_AssignLocationTypes()
    {
        int actualMaxDepth = 0;
        foreach (var p in allPoints)
            if (p.depth > actualMaxDepth) actualMaxDepth = p.depth;

        var startPoints = new List<PointScr>();
        var bossPoints = new List<PointScr>();
        var middlePoints = new List<PointScr>();

        foreach (var p in allPoints)
        {
            if (p.depth == 0) startPoints.Add(p);
            else if (p.depth == actualMaxDepth) bossPoints.Add(p);
            else middlePoints.Add(p);
        }

        foreach (var p in startPoints)
            p.SetLocationType(pointType.city);

        if (multipleBossesAtMaxDepth)
        {
            foreach (var p in bossPoints)
                p.SetLocationType(pointType.boss);
        }
        else
        {
            Shuffle(bossPoints);
            bossPoints[0].SetLocationType(pointType.boss);
            for (int i = 1; i < bossPoints.Count; i++)
                bossPoints[i].SetLocationType(pointType.enemy);
        }

        middlePoints.Sort((a, b) =>
        {
            int dc = a.depth.CompareTo(b.depth);
            return dc != 0 ? dc : a.id.CompareTo(b.id);
        });

        int enemyCount = 0;

        foreach (var p in middlePoints)
        {
            if (enemyCount > 0 && enemyCount % enemiesPerCity == 0)
            {
                p.SetLocationType(pointType.city);
                enemyCount = 0;
                continue;
            }

            if (Random.value < noneChance)
            {
                p.SetLocationType(pointType.none);
            }
            else
            {
                p.SetLocationType(pointType.enemy);
                enemyCount++;
            }
        }

        bool hasCityInMiddle = false;
        foreach (var p in middlePoints)
            if (p.pointType == pointType.city) { hasCityInMiddle = true; break; }

        if (!hasCityInMiddle && middlePoints.Count > 0)
        {
            foreach (var p in middlePoints)
            {
                if (p.pointType == pointType.enemy)
                {
                    p.SetLocationType(pointType.city);
                    break;
                }
            }
        }

        Debug.Log($"[WorldMap] Типи: start={startPoints.Count} shop, boss={bossPoints.Count}, middle={middlePoints.Count}");
    }

    // ═══════════════════════════════════════════════════════════════
    // Shared helpers
    // ═══════════════════════════════════════════════════════════════

    private List<PointScr> GetNeighborCandidates(PointScr origin)
    {
        var result = new List<(PointScr pt, float dist)>();

        foreach (PointScr other in allPoints)
        {
            if (other == origin) continue;
            float dist = Vector3.Distance(
                origin.transform.position, other.transform.position);
            if (dist <= maxNeighborDistance)
                result.Add((other, dist));
        }

        result.Sort((x, y) => x.dist.CompareTo(y.dist));

        var neighbors = new List<PointScr>();
        for (int i = 0; i < Mathf.Min(neighborCandidates, result.Count); i++)
            neighbors.Add(result[i].pt);

        return neighbors;
    }

    private bool CanConnect(PointScr a, PointScr b)
    {
        if (a.connections.Contains(b)) return false;
        if (Mathf.Abs(a.depth - b.depth) > maxDepthDiff) return false;
        if (a.connections.Count >= maxConnectionsPerPoint) return false;
        if (b.connections.Count >= maxConnectionsPerPoint) return false;
        return true;
    }

    private bool EdgeTooCloseToPoints(Vector2 a, Vector2 b, PointScr endA, PointScr endB)
    {
        foreach (PointScr p in allPoints)
        {
            if (p == endA || p == endB) continue;
            float dist = PointToSegmentDistance(V2(p.transform.position), a, b);
            if (dist < minEdgePointClearance) return true;
        }
        return false;
    }

    private static float PointToSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }

    private void AddEdge(PointScr from, PointScr to, Color color)
    {
        from.connections.Add(to);
        to.connections.Add(from);
        allEdges.Add((V2(from.transform.position), V2(to.transform.position)));
        SpawnLine(from.transform, to.transform, color);
    }

    private Vector3? TryPlaceChild(Vector3 parentPos, float idealAngleDeg,
                                    List<float> usedAngles, out float finalAngle)
    {
        finalAngle = idealAngleDeg;

        for (int attempt = 0; attempt < maxPlacementRetries; attempt++)
        {
            float jitter = attempt == 0
                ? 0f
                : Random.Range(-arcSpreadDeg * 0.4f, arcSpreadDeg * 0.4f);

            float angleDeg = idealAngleDeg + jitter;
            if (!IsSiblingAngleOk(angleDeg, usedAngles)) continue;

            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector3 candidate = parentPos
                + new Vector3(dir.x, dir.y, 0f) * ringSpacing
                + SmallJitter();

            if (!IsFarEnoughFromAllPoints(candidate)) continue;
            if (EdgeIntersectsAny(V2(parentPos), V2(candidate), parentPos)) continue;
            if (EdgeTooCloseToPoints(V2(parentPos), V2(candidate),
                    FindPointAt(parentPos), null)) continue;

            finalAngle = angleDeg;
            return candidate;
        }

        return null;
    }

    private PointScr FindPointAt(Vector3 pos)
    {
        foreach (var p in allPoints)
            if (Vector3.Distance(p.transform.position, pos) < 0.01f)
                return p;
        return null;
    }

    private bool IsSiblingAngleOk(float angleDeg, List<float> usedAngles)
    {
        foreach (float used in usedAngles)
            if (Mathf.Abs(Mathf.DeltaAngle(angleDeg, used)) < minSiblingAngleDeg)
                return false;
        return true;
    }

    private bool IsFarEnoughFromAllPoints(Vector3 pos)
    {
        foreach (var p in allPoints)
            if (Vector3.Distance(p.transform.position, pos) < minSpacing)
                return false;
        return true;
    }

    private bool EdgeIntersectsAny(Vector2 a, Vector2 b, Vector3 skipSharedWith)
    {
        Vector2 skip = V2(skipSharedWith);
        foreach (var (ea, eb) in allEdges)
        {
            if (ea == skip || eb == skip) continue;
            if (ea == a || eb == a) continue;
            if (ea == b || eb == b) continue;
            if (SegmentsIntersect(a, b, ea, eb)) return true;
        }
        return false;
    }

    private static bool SegmentsIntersect(Vector2 a1, Vector2 a2,
                                           Vector2 b1, Vector2 b2)
    {
        float d1 = Cross(b2 - b1, a1 - b1);
        float d2 = Cross(b2 - b1, a2 - b1);
        float d3 = Cross(a2 - a1, b1 - a1);
        float d4 = Cross(a2 - a1, b2 - a1);

        return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
               ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
    }

    // ─── Spawn helpers ─────────────────────────────────────────────

    private PointScr SpawnPoint(Vector3 position, int depth)
    {
        GameObject go = Instantiate(pointPrefab, position, Quaternion.identity, pointsRoot);
        go.name = $"Point_{nextId}_d{depth}";

        PointScr p = go.GetComponent<PointScr>() ?? go.AddComponent<PointScr>();
        p.id = nextId++;
        p.depth = depth;
        p.dataCarrier = dataCarrier;
        allPoints.Add(p);
        p.Init();

        return p;
    }

    private void SpawnLine(Transform from, Transform to, Color color)
    {
        GameObject go = Instantiate(linePrefab, linesRoot);
        go.name = $"Line_{from.name}_{to.name}";
        ConnectionLineScr cl = go.GetComponent<ConnectionLineScr>() ?? go.AddComponent<ConnectionLineScr>();
        cl.Init(from, to, color);
    }

    private Transform GetOrCreateRoot(string rootName)
    {
        var existing = transform.Find(rootName);
        if (existing != null)
        {
            foreach (Transform child in existing)
                Destroy(child.gameObject);
            return existing;
        }
        var root = new GameObject(rootName);
        root.transform.SetParent(transform);
        return root.transform;
    }

    // ─── Math utils ────────────────────────────────────────────────

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static float Cross(Vector2 v, Vector2 w) => v.x * w.y - v.y * w.x;
    private static Vector2 V2(Vector3 v) => new Vector2(v.x, v.y);
    private static Vector2 AngleToDir(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private Vector3 SmallJitter() => new Vector3(
        Random.Range(-positionJitter, positionJitter),
        Random.Range(-positionJitter, positionJitter), 0f);

    [ContextMenu("Regenerate Map")]
    private void RegenerateFromMenu() => GenerateMap();

    // ─── Scene visibility ───────────────────────────────────────────

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMapScene = scene.buildIndex == 0;

        SetMapVisible(isMapScene);

        if (!isMapScene)
            return;

        zoneUI = FindObjectOfType<NPCZoneUIScr>();

        foreach (var p in allPoints)
        {
            if (p == null)
                continue;


            p.Init();
        }

        PlayerMoveScr player =
            FindObjectOfType<PlayerMoveScr>();

        if (player != null)
        {
            player.currentPoint?.SelectPoint(true);
        }
    }

    public void SetMapVisible(bool visible)
    {
        if (pointsRoot != null)
            pointsRoot.gameObject.SetActive(visible);
        if (linesRoot != null)
            linesRoot.gameObject.SetActive(visible);
    }
}