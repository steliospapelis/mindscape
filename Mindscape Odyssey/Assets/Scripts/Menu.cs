using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    public GameObject menuPanel;          // The pause menu panel
    public Button resumeButton;          // Resume button
    public Button restartButton;         // Restart button
    public Button quitButton;            // Quit button
    public AudioClip buttonClickSound;   // Sound clip for button clicks
    private AudioSource audioSource;     // AudioSource to play the sound

    void Start()
    {
        // Initially hide the menu
        menuPanel.SetActive(false);

        // Get or add an AudioSource component
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Assign listeners to the buttons and include sound playback
        resumeButton.onClick.AddListener(() => { PlayButtonClickSound(); ResumeGame(); });
        restartButton.onClick.AddListener(() => { PlayButtonClickSound(); RestartGame(); });
        quitButton.onClick.AddListener(() => { PlayButtonClickSound(); QuitGame(); });
    }

    void Update()
    {
        // Check for the Escape key to pause/unpause the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;               // Pause the game
        menuPanel.SetActive(true);        // Show the menu
    }

    void ResumeGame()
    {
        Time.timeScale = 1;               // Resume the game
        menuPanel.SetActive(false);       // Hide the menu
    }

    void RestartGame()
    {
        // Restart the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    void QuitGame()
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
