using System.Collections;
using UnityEngine;

public class GaleneMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float wallSlideSpeed = 2f;
    public float wallJumpXForce = 400f;
    public float wallJumpYForce = 300f;
    public bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = true;
    public bool canMove = false;
    private bool isRecover = false;

    public Transform chart;
    public Transform wallCheck;
    public LayerMask wallLayer;
    public LayerMask Ground;

    public float initialGravityScale = 3.5f;
    public float maxGravityScale = 6f;
    public float gravityIncreaseSpeed = 2f;
    private bool isJumping = false;
    private bool isWallJumping = false;
    private float wallJumpDuration = 0.2f;

    private bool isBeingKnockedBack = false;
    private float knockbackTimer = 0f;

    private float wallTouchTime = 0f; // Timer to track time spent on wall
    public float wallJumpCooldown = 0.5f; // Time required on the wall before wall jump is allowed

     public bool isKnockedDown = false;

     private bool SlowMoHappened=false;

     private Vector2 targetVelocity;
      private bool wasGrounded = false;


    public AudioSource movementAudioSource; // Audio for movement
    public AudioSource landingAudioSource;  // Audio for landing
    private float movementAudioTime = 0f;
    private float fallThreshold = -18f;
    private float maxFallVelocity = -35f;

    public AudioClip[] jumpSounds; // Array to store the jumping sounds
    public AudioSource audioSource;
     

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = initialGravityScale;
        anim.Play("recover", 0, 0);
        Time.timeScale = 1f;
        canMove = false;
    }

    void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("recover"))
        {
            isRecover = true;
        }

        if(transform.position.x>320f && transform.position.y<140f && !SlowMoHappened){
            StartCoroutine(OverrideGravity(0.4f, 1.2f, 1f));
            SlowMoHappened = true;
        }

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("recover") && isRecover)
        {
            isRecover = false;
        }

         if (knockbackTimer > 0)
        {
        knockbackTimer -= Time.deltaTime;
        }
        else if (isBeingKnockedBack)
        {
        isBeingKnockedBack = false;
        }

        

        isGrounded = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.6f), 0.25f, Ground);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);

        // Wall jumping logic with cooldown
            if (isTouchingWall)
            {
                wallTouchTime += Time.deltaTime; // Increase the timer while touching the wall
                if (wallTouchTime >= wallJumpCooldown && !isGrounded && Input.GetKeyDown(KeyCode.W) && rb.velocity.y<=0)
                {
                    StartCoroutine(PerformWallJump());
                }
            }
            else
            {
                wallTouchTime = 0f; // Reset timer when not touching the wall
            }

            if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
            {
                isWallSliding = true;
                WallSlide();
            }
            else
            {
                isWallSliding = false;
            }


        if (canMove && !isBeingKnockedBack && !isKnockedDown)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (!isWallJumping && (!isTouchingWall || isGrounded))
            {
                Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
                rb.velocity = movement;
            }

            if (horizontalInput > 0 && !isFacingRight && !isTouchingWall)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight && !isTouchingWall)
            {
                Flip();
            }

            if (Input.GetKeyDown(KeyCode.W) && isGrounded && rb.velocity.y==0)
            {
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                isJumping = true;
                wallTouchTime = 0f;
                PlayRandomJumpSound();
            }

            
            ApplyCustomGravity();

            anim.SetBool("Run", horizontalInput != 0);
        }

        if(!isKnockedDown){

        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Climbing", isWallSliding);
        }

        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0;

        if (isMoving && isGrounded && canMove)
        {
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.time = movementAudioTime; // Resume from the last position
                movementAudioSource.Play();
            }
        }
        else
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioTime = movementAudioSource.time; // Save the current playback position
                movementAudioSource.Pause();
            }
        }

        // Check if the player has landed
        if (!wasGrounded && isGrounded && rb.velocity.y <= fallThreshold)
        {
            float fallVelocity = Mathf.Abs(rb.velocity.y); // Get absolute fall velocity
            float normalizedVolume = Mathf.InverseLerp(Mathf.Abs(fallThreshold), Mathf.Abs(maxFallVelocity), fallVelocity);
            Debug.Log(fallVelocity);
            Debug.Log(normalizedVolume);
            landingAudioSource.volume = Mathf.Clamp(normalizedVolume, 0.1f, 1f);
            landingAudioSource.Play(); // Play landing sound once
        }

        wasGrounded = isGrounded; // Update grounded state for the next frame
    }

    private void PlayRandomJumpSound()
    {
        // 70% chance to skip playing sound
       
            // Only play a sound if the random value is <= 0.3 (30% chance)
            if (jumpSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, jumpSounds.Length); // Select a random index
                audioSource.clip = jumpSounds[randomIndex]; // Set the random clip
                audioSource.Play(); // Play the sound
            }
        
    }


    public void StartKnockback(float duration)
    {
    isBeingKnockedBack = true;
    knockbackTimer = duration;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        
    }

    private void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    private IEnumerator PerformWallJump()
{
    
    rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    isWallJumping = true;
    canMove = false;
     PlayRandomJumpSound();

    float jumpDirection = isFacingRight ? -1 : 1;
    float initialHorizontalForce = jumpDirection * wallJumpXForce;
    float initialVerticalForce = wallJumpYForce;

    // Apply initial impulse for a snappy wall jump
    rb.velocity = new Vector2(initialHorizontalForce * 0.8f, initialVerticalForce * 0.6f);
    Flip();

    float duration = 0.15f; // Shortened duration for added responsiveness
    float elapsed = 0f;

    while (elapsed < duration)
    {
        // Apply a small, decreasing horizontal force to keep jump natural
        rb.AddForce(new Vector2(initialHorizontalForce * 0.2f * (1 - (elapsed / duration)), 0), ForceMode2D.Force);

        // Slight upward force to maintain jump height without floating
        if (rb.velocity.y < initialVerticalForce * 0.6f)
        {
            rb.AddForce(new Vector2(0, 5f), ForceMode2D.Force);
        }

        elapsed += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }

    // Cooldown to prevent immediate re-jumping
    yield return new WaitForSeconds(wallJumpDuration);
    isWallJumping = false;
    yield return new WaitForSeconds(0.3f); // Cooldown before allowing horizontal input
    canMove = true;
    
}

    private void ApplyCustomGravity()
    {
        if (isGrounded)
        {
            rb.gravityScale = initialGravityScale;
            isJumping = false;
        }
        else if (rb.velocity.y > 0 && isJumping)
        {
            rb.gravityScale = Mathf.Lerp(rb.gravityScale, maxGravityScale, gravityIncreaseSpeed * Time.deltaTime);
        }
        else if (rb.velocity.y < 0)
        {
            rb.gravityScale = maxGravityScale;
        }
    }

    private bool isGravityOverridden = false; // To prevent multiple coroutine calls

private IEnumerator OverrideGravity(float velocityDampingDuration, float gravityHoldTime, float gravityRestoreDuration)
{
    if (isGravityOverridden) yield break; // Avoid multiple triggers
    isGravityOverridden = true;

    float initialGravity = rb.gravityScale;
    Vector2 initialVelocity = rb.velocity;

    // Step 1: Set gravity to 0
    rb.gravityScale = 0;

    // Step 2: Gradually bring velocity to 0
    float elapsedDampingTime = 0f;

    targetVelocity = new Vector2(0f,-2f);

    while (elapsedDampingTime < velocityDampingDuration)
    {
        rb.velocity = Vector2.Lerp(initialVelocity, targetVelocity, elapsedDampingTime / velocityDampingDuration);
        elapsedDampingTime += Time.deltaTime;
        yield return null;
    }

    // Ensure velocity is precisely zero at the end of damping
    rb.velocity = targetVelocity;

    // Step 3: Hold at zero gravity
    yield return new WaitForSeconds(gravityHoldTime);

    // Step 4: Gradually restore gravity
    float elapsedGravityTime = 0f;

    while (elapsedGravityTime < gravityRestoreDuration)
    {
        rb.gravityScale = Mathf.Lerp(0f, initialGravity, elapsedGravityTime / gravityRestoreDuration);
        elapsedGravityTime += Time.deltaTime;
        yield return null;
    }

    // Ensure gravity is fully restored
    rb.gravityScale = initialGravity;
    isGravityOverridden = false; // Allow future overrides
}


}
