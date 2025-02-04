using UnityEngine;
using System.Collections;

public class Trampoline : MonoBehaviour
{
    public float bounceForce = 20f; // The force with which the player will be launched upwards.
    private Animator trampolineAnimator; // Reference to the Animator component.
    private bool isBouncing = false; // Track if the player is currently in a bounce state.
    public AudioSource boingAudioSource;

    private void Start()
    {
        // Get the Animator component attached to the trampoline.
        trampolineAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object that collided is the player and ensure bounce isn’t already active.
        if (collision.CompareTag("Player") && !isBouncing)
        {
            
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.y < 0)
            {
                boingAudioSource.Play();
                playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
                // Apply an upward force to the player to simulate the trampoline bounce.
                playerRb.velocity = new Vector2(playerRb.velocity.x, bounceForce);

                // Trigger the animation and mark the bounce state as active.
                trampolineAnimator.SetBool("Bounce", true);
                isBouncing = true;
                

                // Start coroutine to reset the animation and allow re-bounce.
                StartCoroutine(ResetLaunchAnimation());
            }
        }
    }

    // Coroutine to reset the launch animation and return to the idle animation.
    private IEnumerator ResetLaunchAnimation()
    {
        // Wait for the launch animation to finish.
        yield return new WaitForSeconds(0.75f);

        // Reset the animator to the idle state and allow for another bounce.
        trampolineAnimator.SetBool("Bounce", false);
        isBouncing = false;
    }
}
