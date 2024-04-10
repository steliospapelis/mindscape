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

    public GameObject chart;
    public Animator anim;

    public DeepBreathing deepBreathing;

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
        anim.SetBool("Run",false);
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
