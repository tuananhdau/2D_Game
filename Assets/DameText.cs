using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 1f;
    public float floatSpeed = 2f;
    public float fadeSpeed = 1f;
    public float moveDistance = 1.5f;

    [Header("Random Movement")]
    public float randomXRange = 0.5f;

    private TextMeshProUGUI textMesh;
    private Color originalColor;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }

        startPos = transform.position;

        float randomX = Random.Range(-randomXRange, randomXRange);
        targetPos = startPos + new Vector3(randomX, moveDistance, 0f);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        transform.position = Vector3.Lerp(startPos, targetPos, progress);

        if (textMesh != null)
        {
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, progress * fadeSpeed);
            textMesh.color = newColor;
        }
    }

    public void SetDamage(int damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
}