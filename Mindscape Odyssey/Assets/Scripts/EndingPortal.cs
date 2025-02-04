using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // For UI elements like Image
using System.Collections;

public class EndingPortal : MonoBehaviour
{
    public float rotationSpeed = 20f; // Rotation speed of the portal
    

    public Image blackScreen;  // Assign the black screen image from the Canvas in the inspector
    public float fadeDuration = 3f;  // Time for the fade effect

    private DataLogging dataLogger;

     private void Start()
    {
        dataLogger = FindObjectOfType<DataLogging>();
    }

    // OnTriggerEnter2D will detect when the player enters the trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Check if the player triggered it
        {
            StartCoroutine(FadeOutAndLoadScene());  // Replace "SceneName" with your actual scene name
        }
    }

    // Coroutine to fade out the screen and load a new scene
    IEnumerator FadeOutAndLoadScene()
    {
        // Fade to black
        float elapsedTime = 0f;
        Color color = blackScreen.color;
        color.a = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);  // Gradually increase alpha from 0 to 1
            blackScreen.color = color;  // Apply the color with updated alpha
            yield return null;  // Wait for the next frame
        }

        if (dataLogger != null)
        {
            dataLogger.SaveLogToFile();
        }
        
        
        // Load the new scene once the fade is complete
        SceneManager.LoadScene("Ending");
    }

    private void Update()
    {
        // Rotate the portal continuously
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    
}
