using System.Collections;
using UnityEngine;

public class FallingBridge : MonoBehaviour
{
    public float trembleDuration = 1.0f;    // How long the bridge trembles
    public float trembleIntensity = 0.1f;  // How much the bridge trembles
    public float fallDistance = 2.0f;      // How far the bridge moves down
    public float fallSpeed = 2.0f;         // Speed of the fall
    public float disableDelay = 0.5f;      // Time before disabling the bridge after falling

    public bool disablePlayerMovement = false;

    
    public Animator playerAnimator;

    public GaleneMovement player;

    private Vector3 initialPosition;       // The bridge's starting position
    private bool isFalling = false;        // Prevents re-triggering

    void Start()
    {
        initialPosition = transform.position; // Save the starting position
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFalling && other.CompareTag("Player") && player.isGrounded) // Trigger only for the player
        {
            StartCoroutine(HandleBridgeFall());
        }
    }

      void OnTriggerStay2D(Collider2D other)
    {
        if (!isFalling && other.CompareTag("Player") && player.isGrounded) // Trigger only for the player
        {
            StartCoroutine(HandleBridgeFall());
        }
    }



    private IEnumerator HandleBridgeFall()
    {
        isFalling = true;

        if (disablePlayerMovement)
    {
        
            player.canMove = false;
            playerAnimator.SetBool("Run", false);
            playerAnimator.Play("panic");
    
    }



        // Tremble phase
        float elapsedTime = 0f;
        while (elapsedTime < trembleDuration)
        {
            transform.position = initialPosition + (Vector3)Random.insideUnitCircle * trembleIntensity;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to initial position before falling
        transform.position = initialPosition;

        // Move the bridge down
        Vector3 targetPosition = initialPosition + new Vector3(0, -fallDistance, 0);
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }

        // Delay before disabling
        yield return new WaitForSeconds(disableDelay);

        // Disable the bridge (make it disappear)
        gameObject.SetActive(false);
    }

    public void ResetBridge()
    {
        // Optional: Reset the bridge to its original state for reuse
        transform.position = initialPosition;
        gameObject.SetActive(true);
        isFalling = false;
    }
}
