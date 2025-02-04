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

    public AudioClip buttonClickSound;   // Sound clip for button clicks
    private AudioSource audioSource;    // AudioSource to play the sound

    public AudioController music;

    void Start()
    {
        // Initialize the fade image and other UI elements
        fadeImage.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        hrvModeSelection.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        // Get or add an AudioSource component
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Add listeners to buttons and include sound playback
        newGameButton.onClick.AddListener(() => { PlayButtonClickSound(); OnNewGameClicked(); });
        callibrationButton.onClick.AddListener(() => { PlayButtonClickSound(); OnCallibrationClicked(); });
        quitButton.onClick.AddListener(() => { PlayButtonClickSound(); OnQuitClicked(); });
        startGameButton.onClick.AddListener(() => { PlayButtonClickSound(); OnStartGameClicked(); });
        backButton.onClick.AddListener(() => { PlayButtonClickSound(); OnBackButtonClicked(); });
    }

    void OnNewGameClicked()
    {
        // Update UI to show new game options
        newGameButton.gameObject.SetActive(false);
        callibrationButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(true);
        hrvModeSelection.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    void OnBackButtonClicked()
    {
        // Reset UI to main menu state
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
        music.FadeOut(2f);
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
        // Zoom in the camera to a target size
        float initialSize = mainCamera.orthographicSize;
        float targetSize = 2f; // Adjust as needed

        while (mainCamera.orthographicSize > targetSize)
        {
            mainCamera.orthographicSize -= zoomSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        // Fade in the fadeImage to full opacity
        Color fadeColor = fadeImage.color;

        while (fadeImage.color.a < 1)
        {
            fadeColor.a += fadeSpeed * Time.deltaTime;
            fadeImage.color = fadeColor;
            yield return null;
        }
    }

    void OnCallibrationClicked()
    {
        // Load the calibration scene
        SceneManager.LoadScene("Callibration");
    }

    void OnQuitClicked()
    {
        // Quit the application
        Application.Quit();
    }

    void PlayButtonClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
