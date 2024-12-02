using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LevelLoaderButton : MonoBehaviour
{
    // Field to allow setting the level name in the editor
    public string levelName;
    public Camera mainCamera;
    public Image fadeImage; 

    public float zoomSpeed = 2.5f;
    public float fadeSpeed = 0.5f;

    public void LoadLevel(){

        StartCoroutine(StartLoad());
    }

    
    IEnumerator StartLoad()
    {
        if (!string.IsNullOrEmpty(levelName))
        {
            // Start the fade effect
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeIn());

        // Start zooming the camera
        yield return StartCoroutine(CameraZoomIn());

            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning("Level name is not set for " + gameObject.name);
        }
    }



    IEnumerator CameraZoomIn()
    {
        
        float initialSize = mainCamera.orthographicSize;
        float targetSize = 2f;  // Zoom in value (smaller means more zoom, adjust as needed)

        while (mainCamera.orthographicSize > targetSize)
        {
            mainCamera.orthographicSize -= zoomSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        Color fadeColor = fadeImage.color;

        while (fadeImage.color.a < 1)
        {
            fadeColor.a += fadeSpeed * Time.deltaTime;
            fadeImage.color = fadeColor;
            yield return null;
        }
    }
}