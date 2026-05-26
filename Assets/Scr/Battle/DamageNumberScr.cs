using UnityEngine;
using TMPro;

public class DamageNumberScr : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float speed = 160f;
    public float lifeTime = 1.2f;

    public float randomAngle = 45f;
    public float shake = 20f;

    public float startScale = 1f;
    public float endScale = 1.4f;

    private Vector2 direction;
    private CanvasGroup canvasGroup;

    float timer;

    public void Init(string value, Color color)
    {
        text.text = value;
        text.color = color;

        canvasGroup = GetComponent<CanvasGroup>();

        // випадковий напрямок
        float angle = Random.Range(-randomAngle, randomAngle);

        direction =
            Quaternion.Euler(0, 0, angle)
            * Vector2.up;

        // невеликий стартовий shake
        Vector2 offset = Random.insideUnitCircle * shake;
        transform.localPosition += (Vector3)offset;

        transform.localScale = Vector3.one * startScale;

        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifeTime;

        // рух
        float currentSpeed = speed * (1f - t * 0.7f);

        transform.Translate(
            direction
            * currentSpeed
            * Time.deltaTime);

        // scale
        float scale =
            Mathf.Lerp(startScale, endScale, t);

        transform.localScale =
            Vector3.one * scale;

        // fade
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - t;
        }

        // destroy
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}