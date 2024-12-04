using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WizardScript2 : MonoBehaviour
{
    public float moveSpeed = 2f; 
    public GameObject galene; 
    public GameObject boss; 
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
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
    public CameraMovement cameraShake;

    public Transform skeletal;

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
       

        // Check if the player is within the desired range
        if (galene.transform.position.x >110f && galene.transform.position.y>100f && galeneMovement.canMove)
        {
            galeneAnim.SetBool("Run", false);
            galeneAnim.SetBool("Grounded", true);
            galeneMovement.canMove = false;

            if (galene.transform.localScale.x < 0)
            {
                galene.transform.localScale = new Vector3(galene.transform.localScale.x * -1, galene.transform.localScale.y, galene.transform.localScale.z);
            }

           
        }

         if (galene.transform.position.x >110f && galene.transform.position.y>100f && !hasMoved)
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
        if (dialogueIndex == 4)
        {
            cameraShake.ShakeCamera(2f, 0.8f); 
        }
        
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
        boss.SetActive(true);
    }

    private string GetDialogueText(int index)
    {
        switch (index)
        {
            case 0:
                return "The wizard stands before you once more, his expression a mix of pride and urgency.";
            case 1:
                return "'Well done making it this far, Galene,' he says, his voice calm yet firm. 'You’ve shown great resilience.'";
            case 2:
                return "'But remember, your breathing is your anchor. Use the exercise, it will help you steady your mind as the trials grow more intense.'";
            case 3:
                return "'You’re almost there. The light is within reach.'";
            case 4:
                return "'Oh no,' he whispers, 'he’s coming!' His voice rises, 'Run, Galene! RUN!'";
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
