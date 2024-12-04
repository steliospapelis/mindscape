using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.SceneManagement;
using System.Collections;

public class StartMenu : MonoBehaviour
{

    public Button newGameButton;
    public Button quitButton;
    public Button callibrationButton;
    public Button startGameButton;
    public Button backButton;
    public TMP_Dropdown hrvModeSelection;

    public Camera mainCamera;
    public Image fadeImage; 

    public float zoomSpeed = 2.5f;
    public float fadeSpeed = 0.5f;


    void Start()
    {

        fadeImage.gameObject.SetActive(false);
        
        startGameButton.gameObject.SetActive(false);
        hrvModeSelection.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        
        newGameButton.onClick.AddListener(OnNewGameClicked);
        callibrationButton.onClick.AddListener(OnCallibrationClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        
    }

    
    void OnNewGameClicked()
    {
        
        newGameButton.gameObject.SetActive(false);
        callibrationButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        
        startGameButton.gameObject.SetActive(true);
        hrvModeSelection.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    void OnBackButtonClicked()
    {
        
        newGameButton.gameObject.SetActive(true);
        callibrationButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        
        startGameButton.gameObject.SetActive(false);
        hrvModeSelection.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
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
        SceneManager.LoadScene("Tutorial Level"); 
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

    // Function for "Options" button click
    void OnCallibrationClicked()
    {
        // Restart the current scene or open the options menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("Callibration");
    }

    // Function for "Quit" button click
    void OnQuitClicked()
    {
        // Quit the application
        Application.Quit();
    }
}
