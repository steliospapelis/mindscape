using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu0PointFive: MonoBehaviour
{

    public Button StartGameButton;


    public Camera mainCamera;
    public Image fadeImage; 

    public float zoomSpeed = 2.5f;
    public float fadeSpeed = 0.5f;


    void Start()
    {

        

        
        StartGameButton.onClick.AddListener(OnStartGameClicked);
        
        
    }

    

   

    
   void OnStartGameClicked()
    {
        // Start the transition effects
        StartCoroutine(StartGameTransition());
    }

    IEnumerator StartGameTransition()
    {
        // Start the fade effect
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeIn());

        // Start zooming the camera
        yield return StartCoroutine(CameraZoomIn());

        // After the effects, load the next scene
        SceneManager.LoadScene("Level 1"); 
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
