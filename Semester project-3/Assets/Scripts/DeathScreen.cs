using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    [Header("Death Screen UI")]
    public Image blackScreen;
    public TMP_Text youDiedText;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;

    private bool isFading = false;
    private float elapsedTime = 0f;

    private void Start()
    {
        SetAlpha(0f);
    }

    public void ShowDeathScreen()
{
    if (isFading)
        return;

    // Stop gameplay immediately.
    Time.timeScale = 0f;

    elapsedTime = 0f;
    isFading = true;
}

    private void Update()
    {
        if (!isFading)
            return;

        elapsedTime += Time.unscaledDeltaTime;

        float alpha = fadeDuration <= 0f
            ? 1f
            : Mathf.Clamp01(elapsedTime / fadeDuration);

        SetAlpha(alpha);

        if (alpha >= 1f)
        {
            isFading = false;
            Time.timeScale = 0f;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (blackScreen != null)
        {
            Color imageColor = blackScreen.color;
            imageColor.a = alpha;
            blackScreen.color = imageColor;
        }

        if (youDiedText != null)
        {
            Color textColor = youDiedText.color;
            textColor.a = alpha;
            youDiedText.color = textColor;
        }
    }
}