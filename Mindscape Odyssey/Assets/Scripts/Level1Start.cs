using System.Collections;
using UnityEngine;
using TMPro;  // For TextMesh Pro elements
using UnityEngine.UI;

public class level1Start : MonoBehaviour
{
    public TextMeshProUGUI textObject;  // Assign the first message in the Inspector
    public float fadeDuration = 1f;       // Duration for fade-in/out
    public float messageDuration = 5f;    // How long the message stays visible
    public GaleneMovement movementScript;   // Reference to the other script with canMove
    public string firstMessageText = "";  // The first message
    public string secondMessageText = "";  // The second message

    private bool hasTriggered = false;    // Flag to track if the sequence has already started

    public Image blackScreen;

    public float imageFadeDuration = 1f;

    private void Start(){
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
{
    // Fade from black to transparent
    float elapsedTime = 0f;
    Color color = blackScreen.color;
    color.a = 1f;  // Start with full opacity (black)

    while (elapsedTime < imageFadeDuration)
    {
        elapsedTime += Time.deltaTime;
        color.a = Mathf.Clamp01(1f - (elapsedTime / imageFadeDuration));  // Gradually decrease alpha from 1 to 0
        blackScreen.color = color;  // Apply the color with updated alpha
        yield return null;  // Wait for the next frame
    }
    
    // Ensure the screen is fully transparent at the end
    color.a = 0f;
    blackScreen.color = color;

    yield return null;
    
}


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))  // Only run if not triggered before
        {
            hasTriggered = true;  // Set flag to true
            movementScript.canMove = false;  // Set canMove to false in the other script
            StartCoroutine(ShowMessageSequence());  // Start fading text
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeText(textObject, false));  // Fade out the second message when player exits trigger
        }
    }

    // Sequence to fade in/out messages
    IEnumerator ShowMessageSequence()
    {
        textObject.text = firstMessageText;

        // Fade in the first message
        yield return StartCoroutine(FadeText(textObject, true));

        // Wait for the message duration
        yield return new WaitForSeconds(messageDuration);

        // Fade out the first message
        yield return StartCoroutine(FadeText(textObject, false));

        textObject.text = secondMessageText;
        movementScript.canMove = true;  // Allow movement again

        // Fade in the second message
        yield return StartCoroutine(FadeText(textObject, true));
    }

    // Coroutine to fade text in or out
    IEnumerator FadeText(TextMeshProUGUI textMeshPro, bool fadeIn)
    {
        float elapsedTime = 0f;
        Color color = textMeshPro.color;

        // Determine fade direction
        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = color.a;

        // Fade in or out over time
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            textMeshPro.color = color;
            yield return null;  // Wait until the next frame
        }

        // Ensure the final alpha is exactly the target
        color.a = targetAlpha;
        textMeshPro.color = color;
    }
}

