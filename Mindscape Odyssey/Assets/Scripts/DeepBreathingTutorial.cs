using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeepBreathingTutorial : MonoBehaviour
{
    public GameObject textBox;
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI SpacePrompt;
    private bool inRange = false;
    private bool dialogueStarted = false;
    private int dialogueIndex = 0;

    private bool dialogueCompleted = false;
    public GaleneMovement GaleneMovement;

    public GameObject breathingIcon;

    public HealthManager healthScript;

    public GameObject chart;
    public Animator anim;

    public DeepBreathing deepBreathing;

    public Transform respawn;

    private bool waitingForBreathingSession = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
    }

    private void Update()
    {
        if (inRange && !dialogueStarted && !dialogueCompleted)
        {
            StartDialogue();
        }

        if (dialogueStarted && Input.GetKeyDown(KeyCode.Space) && !waitingForBreathingSession)
        {
            NextDialogue();
        }
    }

    private void StartDialogue()
    {
        healthScript.respawnPoint = respawn;
        GaleneMovement.canMove = false;
        anim.SetBool("Run", false);
        dialogueStarted = true;
        textBox.SetActive(false);
        dialogueBox.SetActive(true);

        dialogueText.text = GetDialogueText(dialogueIndex);
    }

    private void NextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex < 9)
        {
            dialogueText.text = GetDialogueText(dialogueIndex);

            if (dialogueIndex == 4)
            {
                deepBreathing.enabled = true;
                StartBreathingSession();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void StartBreathingSession()
    {
        
        SpacePrompt.text="";
        // Start the breathing session
        deepBreathing.StartBreathing(6f);

        // Wait for the breathing session to finish before continuing
        StartCoroutine(WaitForBreathingSession());
    }

    private IEnumerator WaitForBreathingSession()
    {
        waitingForBreathingSession = true;

        // Wait for the duration of the breathing session
        yield return new WaitForSeconds(deepBreathing.totalTime+2.6f);

        // Advance the dialogue index and update the text
        dialogueIndex++;
        dialogueText.text = GetDialogueText(dialogueIndex);

        // Allow dialogue to continue
        waitingForBreathingSession = false;
        SpacePrompt.text="Press Space to continue";
        GaleneMovement.canMove=false;
        anim.SetBool("Run", false);
        deepBreathing.enabled = false;


    }

    private void EndDialogue()
    {
        dialogueBox.SetActive(false);
        dialogueStarted = false;
        dialogueIndex = 0;
        GaleneMovement.canMove = true;
        healthScript.breathingUnlocked = true;
        breathingIcon.SetActive(true);
        dialogueCompleted = true;
        deepBreathing.enabled = true;
    }

    private string GetDialogueText(int index)
    {
        switch (index)
        {
            case 0:
                return "As you approach the ancient tree, its leaves rustle softly, and a calm, soothing voice fills your mind.";
            case 1:
                return "Tree: 'Welcome, weary traveler. I see you carry the weight of the world on your shoulders. Allow me to share a secret of the forest, a gift to help you find peace within.";
            case 2:
                return "Quiet your mind and listen closely. This is the art of deep breathing, a simple yet powerful technique to calm your mind and body.";
            case 3:
                return "Inhale slowly through your nose, filling your lungs completely. Feel the breath travel deep into your belly. Hold for a moment, then gently exhale through your mouth, letting go of all your tension.";
            case 4:
                return "Repeat this with me: Breathe in… hold… breathe out. Imagine the stress and anxiety leaving your body with each breath, like leaves falling in the autumn breeze.";
            case 5:
                return "Deep breathing slows your heart rate and lowers blood pressure, helping you feel more grounded and in control. It’s a way to anchor yourself in the present, no matter the storm around you.";
            case 6:
                return "Remember, in the midst of chaos, your breath is your refuge. Practice this, and you will find serenity within.'";
            case 7:
                return "The tree’s whisper fades, leaving you with a newfound sense of calm and a powerful tool to carry forward on your journey.";
            default:
                return "You can now press B to freeze time and initiate a breathing session to calm down and replenish Galene's health.";
        }
    }
}
