using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WizardScript : MonoBehaviour
{
    public float moveSpeed = 2f; 
    public GameObject galene; 
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public float stopXPosition = -12f; 
    private Animator anim; 
    private Animator galeneAnim; 
    private GaleneMovement galeneMovement; 
    private bool hasMoved = false;
    private float moveDistance = 6f; 
    private float moveBackDistance = 2f; 
    private Vector3 startPosition;
    private bool isMovingBack = false;
    private bool dialogueStarted = false;
    private int dialogueIndex = 0; 

    public ParticleSystem disappearEffect;

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
        if (galene.transform.position.x >= stopXPosition && galeneMovement.canMove)
        {
            galeneAnim.SetBool("Run", false);
            galeneAnim.SetBool("Grounded", true);
            galeneMovement.canMove = false;
            if (galene.transform.localScale.x < 0)
            {
                galene.transform.localScale = new Vector3(galene.transform.localScale.x * -1, galene.transform.localScale.y, galene.transform.localScale.z);
            }
        }

        if (!hasMoved && galene.transform.position.x >= stopXPosition)
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

        if(dialogueIndex==0){
        StartDialogue();
        }

        if (Vector3.Distance(transform.position, startPosition) < distance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
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
    // Only allow advancing the dialogue if the wizard has stopped moving
    if (hasMoved || dialogueIndex == 0) 
    {
        dialogueIndex++;
        if (dialogueIndex < 5)
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
        
    }

    private string GetDialogueText(int index)
    {
        switch (index)
        {
            case 0:
                return "As you step into the clearing, you spot a figure in a long, tattered robe standing before you. The wizard walks towards you slowly.";
            case 1:
                return "'Welcome Galene!', he exclaims, 'we have been expecting you!'. Before you have time to answer, he continues";
            case 2:
                return "'Do you sense the darkness? It spreads like a plague upon our realm. Only you can bring back the light. But remember...";
            case 3:
                return "'This isn’t a battle of strength, but of the mind. The shadows you face come from fear, from doubt. To find peace out there, you must first find it within.'";
            case 4:
                return "The wizard begins to walk away, ignoring your questions. The answers you seek, will have to wait.";
            default:
                return "End of dialogue";
        }
    }

    void Flip()
    {
        if (transform != null)
        {
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
        else
        {
            Debug.LogError("Transform component missing on the wizard object.");
        }
    }
}
