
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [Header("UI")]
    public Image flashImage;

    [Header("Flash Settings")]
    [Range(0f, 1f)]
    public float flashAlpha = 0.35f;

    public float fadeDuration = 0.3f;

    private float remainingTime;

    void Awake()
    {
        SetAlpha(0f);
    }

    void Update()
    {
        if (remainingTime <= 0f)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(
            remainingTime / Mathf.Max(0.01f, fadeDuration)
        );

        SetAlpha(flashAlpha * progress);
    }

    public void Flash()
    {
        remainingTime = fadeDuration;
        SetAlpha(flashAlpha);
    }

    void SetAlpha(float alpha)
    {
        if (flashImage == null)
            return;

        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }
}
