using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingMenu : MonoBehaviour
{
    public Button mainButton;  // Button to play audio and load scene
    public Button exitButton;  // Button to quit the app
    public AudioSource audioSource; // Audio source to play sound
    public AudioClip buttonClickSound; // Sound to play on button press
    public Image fadeImage; // The black image that will fade out

    private void Start()
    {
        // Assign button click listeners
        mainButton.onClick.AddListener(OnMainButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        // Start fading out after 2 seconds
        if (fadeImage != null)
        {
            StartCoroutine(StartFadeOutAfterDelay(4f, 2f)); // Wait 2s, then fade over 2s
        }
    }

    private void OnMainButtonClicked()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        // Load the next scene after a small delay to allow the sound to play
        SceneManager.LoadScene("Starting Menu");
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Application Quit"); // This only appears in the editor
    }

    private IEnumerator StartFadeOutAfterDelay(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay); // Wait before starting fade
        yield return StartCoroutine(FadeOutImage(fadeDuration));
        Destroy(fadeImage.gameObject); // Destroy the image after fade out
    }

    private IEnumerator FadeOutImage(float duration)
    {
        float timer = 0f;
        Color imageColor = fadeImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            imageColor.a = Mathf.Lerp(1f, 0f, timer / duration); // Fade alpha from 1 to 0
            fadeImage.color = imageColor;
            yield return null;
        }

        // Ensure it's fully transparent before destroying
        imageColor.a = 0f;
        fadeImage.color = imageColor;
    }
}
