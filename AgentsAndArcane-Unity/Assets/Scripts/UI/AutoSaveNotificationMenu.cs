using System.Collections;
using UnityEngine;
using TMPro;

public class AutoSaveNotificationMenu : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fadeDuration = 0.5f;  // blinking duration
    public float cycleDuration;  // blink cycle duration
    private CanvasGroup canvasGroup;

    void Start()
    {
        cycleDuration = DataPersistenceManager.instance.autoSaveInterval;
        canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        StartCoroutine(BlinkingEffect());
    }

    IEnumerator BlinkingEffect()
    {
        while (true)
        {
            // 60 secs cycle
            yield return new WaitForSeconds(cycleDuration - 1f);  // minus 1 second to avoid blinking at the same time as the autosave
            
            // blink twice
            for (int i = 0; i < 2; i++)
            {
                // fade in
                yield return StartCoroutine(FadeText(0f, 1f));
                // fade out
                yield return StartCoroutine(FadeText(1f, 0f));
            }

            // 保持消失状态
            canvasGroup.alpha = 0f;
        }
    }

    IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}
