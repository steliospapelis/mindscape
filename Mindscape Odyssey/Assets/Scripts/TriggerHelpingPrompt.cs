using UnityEngine;
using TMPro;
using System.Collections;

public class TriggerHelpingPrompt : MonoBehaviour
{
    public TextMeshProUGUI helpText; // Reference to the TextMeshProUGUI element
    public string message; // Message to display when triggered
    
    public float fadeDuration = 1f; // Duration of fade in/out

    private bool hasTriggered = false;

    void Start()
    {
        // Ensure the text is fully invisible at the start
        helpText.alpha = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            helpText.text = message;
            StartCoroutine(FadeTextIn());
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FadeTextOut());
            
        }
    }

    // Coroutine to fade in the text
    private IEnumerator FadeTextIn()
    {
        helpText.alpha=0;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alphaValue = Mathf.Clamp01(elapsedTime / fadeDuration);
            helpText.alpha = alphaValue;
            yield return null;
        }
        helpText.alpha = 1f; // Ensure it's fully visible at the end
    }

    // Coroutine to fade out the text
    private IEnumerator FadeTextOut()
    {
        float initialAlpha = helpText.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alphaValue = Mathf.Clamp01(initialAlpha - (elapsedTime / fadeDuration));
            helpText.alpha = alphaValue;
            yield return null;
        }
        helpText.alpha = 0f; // Ensure it's fully invisible at the end
    }
}
