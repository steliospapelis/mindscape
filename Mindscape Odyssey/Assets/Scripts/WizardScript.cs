using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardScript : MonoBehaviour
{
    public float moveSpeed = 2f; // Adjust the speed of the wizard's movement
    public GameObject galene; // Assign Galene's GameObject here in the editor
    private Animator anim; // Reference to the animator component
    private bool hasMoved = false;
    private float moveDistance = 6f; // The distance the wizard will move initially
    private float moveBackDistance = 2f; // The distance the wizard will move back
    private Vector3 startPosition;
    private bool isMovingBack = false;

    void Start()
    {
        anim = GetComponent<Animator>(); // Get the animator component
        anim.SetBool("idle", true); // Start idle animation
        startPosition = transform.position; // Remember start position
    }

    void Update()
    {
        if (!hasMoved && galene.transform.position.x >= -12)
        {
            MoveWizard(Vector3.left, moveDistance);
        }

        if (isMovingBack)
        {
            MoveWizard(Vector3.right, moveBackDistance);
        }
    }

    private void MoveWizard(Vector3 direction, float distance)
    {
        anim.SetBool("idle", false);
        anim.SetBool("isRun", true); // Start moving animation

        // Move the wizard in the specified direction
        if (Vector3.Distance(transform.position, startPosition) < distance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else if (!hasMoved)
        {
            // Once the initial movement distance is reached, stop and idle
            hasMoved = true; // Prevent further initial movement
            anim.SetBool("isRun", false);
            anim.SetBool("idle", true); // Switch back to idle
            StartCoroutine(WaitAndMoveBack());
        }
        else if (isMovingBack)
        {
            // Make the wizard inactive after moving back
            gameObject.SetActive(false);
        }
    }

    IEnumerator WaitAndMoveBack()
    {
        yield return new WaitForSeconds(3);
        startPosition = transform.position; // Reset start position for moving back
        Flip(); // Flip the wizard to face the other direction
        isMovingBack = true; // Enable moving back
    }

    void Flip()
    {
        // Flip the wizard by multiplying the x scale by -1
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
