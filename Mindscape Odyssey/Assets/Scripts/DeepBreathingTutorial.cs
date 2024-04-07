using UnityEngine;
using UnityEngine.UI; // Import the Unity UI namespace

public class DeepBreathingTutorial : MonoBehaviour
{
    public GameObject textBox; 
    public GameObject dialogueBox; 
    public Animator dialogueAnimator; 
    public Text dialogueText; 
    private bool inRange = false;
    private bool dialogueStarted = false;
    private int dialogueIndex = 0; 
    public GaleneMovement GaleneMovement;

    public GameObject chart;

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
        GaleneMovement.canMove = false;
        dialogueStarted = true;
        textBox.SetActive(false);
        dialogueBox.SetActive(true);
        

        
        dialogueText.text = GetDialogueText(dialogueIndex);
    }

    private void NextDialogue()
    {
        dialogueIndex++; 
        
        if (dialogueIndex < 3)
        {
            
            dialogueText.text = GetDialogueText(dialogueIndex);
            if(dialogueIndex ==2){
                chart.SetActive(true);
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
        chart.SetActive(false);
    }

    
    private string GetDialogueText(int index)
    {
        
        switch (index)
        {
            case 0:
                return "The tree whispers to you...";
            case 1:
                return "Deep Breathing blah blah blah ";
            case 2:
                return "blah blah blah try it ";
            default:
                return "End of dialogue.";
        }
    }

    

}
