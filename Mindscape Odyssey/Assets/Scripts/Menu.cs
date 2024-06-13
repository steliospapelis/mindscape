using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu: MonoBehaviour
{
    public GameObject menuPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    private Vector3 originalTextPosition;


    void Start()
    {
        // Initially hide the menu
        menuPanel.SetActive(false);

        // Add listeners to the buttons
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
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
        
        Time.timeScale = 0; // Pause the game
        menuPanel.SetActive(true); // Show the menu
    }

    void ResumeGame()
    {
        Time.timeScale = 1; // Resume the game
        menuPanel.SetActive(false); // Hide the menu
    }

    void RestartGame()
    {
        // Restart the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}
    
