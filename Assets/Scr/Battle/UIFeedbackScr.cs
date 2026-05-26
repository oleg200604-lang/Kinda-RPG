using UnityEngine;

public class UIFeedbackScr : MonoBehaviour
{
    public Transform playerPoint;
    public Transform enemyPoint;

    public GameObject damageNumberPrefab;

    public void SpawnDamage(PlayerScr target, int value, bool isFast, Color color)
    {
        Transform spawnPoint = GetPoint(target);

        if (isFast)
        {
            int amount = Mathf.Clamp(value, 2, 8);

            for (int i = 0; i < amount; i++)
            {
                SpawnSingle(spawnPoint,
                    Mathf.CeilToInt((float)value / amount).ToString(),
                    color);
            }
        }
        else
        {
            SpawnSingle(spawnPoint, value.ToString(), color);
        }
    }

    public void SpawnText(PlayerScr target, string text, Color color)
    {
        Transform spawnPoint = GetPoint(target);

        SpawnSingle(spawnPoint, text, color);
    }

    Transform GetPoint(PlayerScr target)
    {
        return target.whoIm == EffectTarget.Player
            ? playerPoint
            : enemyPoint;
    }

    void SpawnSingle(Transform point, string value, Color color)
    {
        GameObject obj = Instantiate(
            damageNumberPrefab,
            point.position,
            Quaternion.identity,
            point);

        // компенсуємо mirror
        Vector3 scale = obj.transform.localScale;

        scale.x *= Mathf.Sign(point.lossyScale.x);
        scale.y *= Mathf.Sign(point.lossyScale.y);

        obj.transform.localScale = scale;

        obj.GetComponent<DamageNumberScr>()
            .Init(value, color);
    }
}