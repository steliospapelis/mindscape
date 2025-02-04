using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WizardScript3 : MonoBehaviour
{
    public float moveSpeed = 2f; 
    public GameObject galene; 
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    private Animator anim; 
    private Animator galeneAnim; 
    private GaleneMovement galeneMovement; 
    private bool hasMoved = false;
    private float moveDistance = 3f; 
    private float moveBackDistance = 2f; 
    private Vector3 startPosition;
    private bool isMovingBack = false;
    private bool dialogueStarted = false;
    private int dialogueIndex = 0; 
    
    public Transform skeletal;

    public ParticleSystem disappearEffect;

     public TextMeshProUGUI runePrompt;
     

    void Start()
    {
        anim = GetComponent<Animator>();
        galeneMovement = galene.GetComponent<GaleneMovement>();
        galeneAnim = galene.GetComponent<Animator>();
        anim.SetBool("idle", true);
        startPosition = transform.position;
        dialogueBox.SetActive(false);
        dialogueIndex = 0;
    }

    void Update()
    {
       

        // Check if the player is within the desired range
        if (galene.transform.position.x >320f && galene.transform.position.y<113.8f && galeneMovement.canMove)
        {
            galeneAnim.SetBool("Run", false);
            galeneAnim.SetBool("Grounded", true);
            galeneMovement.canMove = false;

            if (galene.transform.localScale.x < 0)
            {
                galene.transform.localScale = new Vector3(galene.transform.localScale.x * -1, galene.transform.localScale.y, galene.transform.localScale.z);
            }

           
        }

         if (galene.transform.position.x >320f && galene.transform.position.y<117f && !hasMoved)
            {
                MoveWizard(Vector3.left, moveDistance);
            }

        if (dialogueStarted && Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }

        if (isMovingBack)
        {
            MoveWizard(Vector3.right, moveBackDistance);
        }
    }

    private void MoveWizard(Vector3 direction, float distance)
    {
        anim.SetBool("idle", false);
        anim.SetBool("isRun", true);

        if (dialogueIndex == 0)
        {
            StartDialogue();
        }

        if ((Vector3.Distance(transform.position, startPosition) < distance))
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            Debug.Log(Vector3.Distance(transform.position, startPosition));
        }
        else if (!hasMoved)
        {
            hasMoved = true;
            anim.SetBool("isRun", false);
            anim.SetBool("idle", true);
        }
        else if (isMovingBack)
        {
            gameObject.SetActive(false);
            disappearEffect.Play();
            galeneMovement.canMove = true;
        }
    }

    private void StartDialogue()
    {
        dialogueStarted = true;
        dialogueBox.SetActive(true);
        dialogueText.text = GetDialogueText(dialogueIndex);
    }

    private void NextDialogue()
    {
        if (hasMoved || dialogueIndex == 0) 
        {
            dialogueIndex++;
        
            if (dialogueIndex < 3)
            {
                dialogueText.text = GetDialogueText(dialogueIndex);
            }
            else
            {
                EndDialogue();
            }
        }
        else
        {
            Debug.Log("Wizard is still moving. Please wait.");
        }
    }

    private void EndDialogue()
    {
        dialogueBox.SetActive(false);
        dialogueStarted = false;
        startPosition = transform.position;
        isMovingBack = true;
        
        Flip();
        runePrompt.gameObject.SetActive(true);
        
    }

    private string GetDialogueText(int index)
    {
        switch (index)
        {
            case 0:
                return "The wizard appears again, shining like a beacon in the dark storm.";
            case 1:
                return "'They say that runes respond to the rhythm of one’s breath. Perhaps a steady, measured inhale and exhale near them might awaken their power.'";
            case 2:
                return "Keep your focus, Galene. You have the strength to overcome this.";
            default:
                return "End of dialogue";
        }
    }

    void Flip()
    {
        if (transform != null)
        {
            Vector3 theScale = skeletal.localScale;
            theScale.x *= -1;
            skeletal.localScale = theScale;
        }
        else
        {
            Debug.LogError("Transform component missing on the wizard object.");
        }
    }
}
