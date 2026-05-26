using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMouseControllerScr : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform target;

        [Tooltip("Наскільки сильно цей шар реагує. 1 = максимум, 0 = нерухомий.")]
        [Range(0f, 1f)]
        public float depth = 0.5f;

        [HideInInspector] public Vector3 startPosition;
    }

    [Header("Шари паралаксу")]
    [Tooltip("Залиш порожнім — скрипт автоматично знайде об'єкти з тегом 'ParallaxLayer'.")]
    public ParallaxLayer[] layers;

    [Header("Сила руху")]
    [Tooltip("Максимальне зміщення по X у юніт-просторі при краях екрана.")]
    public float strengthX = 1.5f;

    [Tooltip("Множник для Y-осі відносно X (зазвичай слабший ефект).")]
    [Range(0f, 1f)]
    public float yMultiplier = 0.3f;

    [Header("Ліміт зміщення")]
    [Tooltip("Базовий максимум зміщення по X. Масштабується на depth кожного шару.")]
    public float maxOffsetX = 2f;

    [Tooltip("Базовий максимум зміщення по Y. Масштабується на depth кожного шару.")]
    public float maxOffsetY = 0.6f;

    [Header("Плавність")]
    [Tooltip("Швидкість сглажування (Lerp). Менше = плавніше/повільніше.")]
    [Range(0.5f, 20f)]
    public float smoothing = 5f;

    // ─────────────────────────────────────────────────────────────────────────

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;

        // Автопошук шарів за тегом, якщо масив не заповнений вручну
        if (layers == null || layers.Length == 0)
            AutoCollectLayers();

        // Запам'ятовуємо початкові позиції
        foreach (var layer in layers)
        {
            if (layer.target != null)
                layer.startPosition = layer.target.position;
        }
    }

    private void Update()
    {
        // Нормалізована позиція миші: центр екрана = (0,0), краї = (±1, ±1)
        Vector2 mouseNorm = GetNormalizedMousePosition();

        float deltaTime = Time.deltaTime;

        foreach (var layer in layers)
        {
            if (layer.target == null) continue;

            // Максимум зміщення масштабується разом із depth шару:
            // далекий шар (depth=1) → повний maxOffset
            // близький шар (depth=0.1) → лише 10% від maxOffset
            float clampX = maxOffsetX * layer.depth;
            float clampY = maxOffsetY * layer.depth;

            float offsetX = mouseNorm.x * strengthX * layer.depth;
            float offsetY = mouseNorm.y * strengthX * yMultiplier * layer.depth;

            // Обмежуємо зміщення пропорційним максимумом
            offsetX = Mathf.Clamp(offsetX, -clampX, clampX);
            offsetY = Mathf.Clamp(offsetY, -clampY, clampY);

            Vector3 targetPos = layer.startPosition + new Vector3(offsetX, offsetY, 0f);

            // Плавний рух через Lerp
            layer.target.position = Vector3.Lerp(
                layer.target.position,
                targetPos,
                smoothing * deltaTime
            );
        }
    }

    // ─── Допоміжні методи ────────────────────────────────────────────────────

    /// <summary>
    /// Повертає позицію миші нормалізовану до [-1, 1] по обох осях,
    /// де (0, 0) — центр вікна.
    /// </summary>
    private Vector2 GetNormalizedMousePosition()
    {
        Vector3 mouse = Input.mousePosition;

        float x = (mouse.x / Screen.width - 0.5f) * 2f;  // [-1 .. 1]
        float y = (mouse.y / Screen.height - 0.5f) * 2f;  // [-1 .. 1]

        return new Vector2(x, y);
    }

    /// <summary>
    /// Шукає всі об'єкти з тегом "ParallaxLayer" і будує масив шарів.
    /// depth призначається автоматично за порядком (перший = найдальший).
    /// </summary>
    private void AutoCollectLayers()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag("ParallaxLayer");

        if (found.Length == 0)
        {
            Debug.LogWarning("[Parallax] Не знайдено жодного об'єкта з тегом 'ParallaxLayer'.");
            layers = new ParallaxLayer[0];
            return;
        }

        layers = new ParallaxLayer[found.Length];

        for (int i = 0; i < found.Length; i++)
        {
            layers[i] = new ParallaxLayer
            {
                target = found[i].transform,
                // Рівномірний розподіл depth від 1.0 (далеко) до (1/n) (близько)
                depth = 1f - (float)i / found.Length
            };
        }

        Debug.Log($"[Parallax] Знайдено {found.Length} шарів.");
    }

    // ─── Відладка ─────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (layers == null) return;

        foreach (var layer in layers)
        {
            if (layer.target == null) continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(layer.startPosition, 0.15f);
            Gizmos.DrawLine(layer.startPosition, layer.target.position);
        }
    }
}
