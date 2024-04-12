using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WizardScript : MonoBehaviour
{
    public float moveSpeed = 2f; 
    public GameObject galene; 
    public GameObject dialogueBox;
    public Text dialogueText;
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

    void Start()
    {
        anim = GetComponent<Animator>();
        galeneMovement = galene.GetComponent<GaleneMovement>();
        galeneAnim = galene.GetComponent<Animator>();
        anim.SetBool("idle", true);
        startPosition = transform.position;
        dialogueBox.SetActive(false);
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

        if (dialogueStarted && Input.GetKeyDown(KeyCode.Return))
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

        if (Vector3.Distance(transform.position, startPosition) < distance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else if (!hasMoved)
        {
            hasMoved = true;
            anim.SetBool("isRun", false);
            anim.SetBool("idle", true);
            StartDialogue();
        }
        else if (isMovingBack)
        {
            gameObject.SetActive(false);
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

    private void EndDialogue()
    {
        dialogueBox.SetActive(false);
        dialogueStarted = false;
        dialogueIndex = 0;
        startPosition = transform.position;
        Flip();
        isMovingBack = true;
    }

    private string GetDialogueText(int index)
    {
        switch (index)
        {
            case 0:
                return "The wizard speaks...";
            case 1:
                return "Another piece of wisdom...";
            case 2:
                return "End of the wizard's tales.";
            default:
                return "End of dialogue.";
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
