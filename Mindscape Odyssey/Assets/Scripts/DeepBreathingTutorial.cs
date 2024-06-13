using UnityEngine;
using UnityEngine.UI; 

public class DeepBreathingTutorial : MonoBehaviour
{
    public GameObject textBox; 
    public GameObject dialogueBox; 
    public Text dialogueText; 
    private bool inRange = false;
    private bool dialogueStarted = false;
    private int dialogueIndex = 0; 
    public GaleneMovement GaleneMovement;

    public HealthManager healthScript;

    public GameObject chart;
    public Animator anim;

    public DeepBreathing deepBreathing;

    public Transform respawn;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            textBox.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            textBox.SetActive(false);
        }
    }

    private void Update()
    {
        if (inRange && !dialogueStarted && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }

        if (dialogueStarted && Input.GetKeyDown(KeyCode.Return))
        {
            NextDialogue();
        }
    }

    private void StartDialogue()
    {
        healthScript.respawnPoint = respawn;
        GaleneMovement.canMove = false;
        anim.SetBool("Run",false);
        dialogueStarted = true;
        textBox.SetActive(false);
        dialogueBox.SetActive(true);
        

        
        dialogueText.text = GetDialogueText(dialogueIndex);
    }

    private void NextDialogue()
    {
        dialogueIndex++; 
        
        if (dialogueIndex < 8)
        {
            
            dialogueText.text = GetDialogueText(dialogueIndex);
            if(dialogueIndex ==4){
                chart.SetActive(true);
                anim.SetBool("Breathing",true);
            }
        }
        else
        {
            
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        
        dialogueBox.SetActive(false);
        dialogueStarted = false;
        dialogueIndex = 0; 
        GaleneMovement.canMove = true;
        anim.SetBool("Breathing",false);
        chart.SetActive(false);
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
            default:
                return "The tree’s whisper fades, leaving you with a newfound sense of calm and a powerful tool to carry forward on your journey.";
        }
    }

    

}
