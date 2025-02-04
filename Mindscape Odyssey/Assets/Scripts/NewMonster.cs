using System.Collections;
using UnityEngine;

public class NewMonster : MonoBehaviour
{
    public float speed;
    public float circleRadius;
    private Rigidbody2D enemyRB;
    public GameObject groundCheck;

    public GameObject wallCheck;
    public LayerMask ground;
    public bool facingRight = false;
    public bool isGrounded;
    private Animator anim;

    private Transform player;

    public Rigidbody2D playerRB;
    public float detectionRange = 5f;
    public float yThreshold = 1.0f;
    public float boostedSpeedMultiplier = 1.5f;

    private bool isChasing = false;
    private bool isWalled;
    private bool isAttacking = false; // Flag to check if attack is ongoing

    private DeepBreathing deepBr;

    private GaleneMovement playerMovement;
    private HealthManager health;
    public float Damage = 5f;

    public float horizontalKnockbackForce = 5f; // Adjust for horizontal knockback
    public float verticalKnockbackForce = 3f;   // Adjust for vertical knockback

    public float bounceForce = 10f;

    public AudioSource deathAudioSource;
    public AudioSource hitAudioSource;

    void Start()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        anim.SetBool("walk", true);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.transform;
        playerMovement = playerObject.GetComponent<GaleneMovement>();
        playerRB = playerObject.GetComponent<Rigidbody2D>();
        health = playerObject.GetComponent<HealthManager>();
        deepBr = playerObject.GetComponent<DeepBreathing>();
        
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.transform.position, circleRadius, ground);
        isWalled = Physics2D.OverlapCircle(wallCheck.transform.position, circleRadius, ground);

        if(!facingRight && speed<0){
            speed=-speed;
        }

        if ((!isGrounded || isWalled) && !isAttacking)
        {
            Flip();
        }

        // Check if player is within detection range and at a similar y-level
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool sameYLevel = Mathf.Abs(transform.position.y - player.position.y) < yThreshold;
        bool playerInFront = (!facingRight && player.position.x > transform.position.x) ||
                             (facingRight && player.position.x < transform.position.x);

        if (distanceToPlayer <= detectionRange && sameYLevel && playerInFront && playerMovement.isGrounded && !isAttacking)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (!isAttacking) 
        {
            float currentSpeed = isChasing ? speed * boostedSpeedMultiplier : speed;
            enemyRB.velocity = new Vector2(currentSpeed, enemyRB.velocity.y);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        speed = -speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") &&!deepBr.isBreathing)
        {
            
            // Check if player collided from the top
            if (collision.contacts[0].normal.y < -0.8f )
            {
                anim.Play("demage");
                deathAudioSource.Play();
                isAttacking = true;
                speed=0;
                enemyRB.velocity = Vector2.zero;
                health.Healing(5);
                StartCoroutine(DestroyAfterAnimation());
              
                playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
                
                playerRB.velocity = new Vector2(playerRB.velocity.x, bounceForce);

                GetComponent<Collider2D>().enabled = false;
            }
            else
            {
                // Player collided from the side, trigger attack animation and deal damage
                if (!isAttacking)
                {
                    isAttacking = true;
                    anim.Play("attack");
                    hitAudioSource.Play();
                    enemyRB.velocity = Vector2.zero; // Stop enemy movement
                    StartCoroutine(AttackPlayer()); // Deal damage after a delay
                }
            }
        }
    }

    private IEnumerator AttackPlayer()
    {
       
        playerMovement.StartKnockback(0.7f);
        health.TakeDamage(Damage); // Deal damage to player
        // Determine the horizontal direction of the knockback
        Vector2 currentVelocity = playerRB.velocity;
        playerRB.velocity = new Vector2(0f, 0f);
         Vector2 knockbackDirection = (player.position.x > transform.position.x) ? Vector2.right : Vector2.left;

        // Combine the horizontal and vertical components into a single knockback vector
        Vector2 combinedKnockback = (knockbackDirection * horizontalKnockbackForce) + (Vector2.up * verticalKnockbackForce);

        // Apply the combined knockback force
        playerRB.AddForce(combinedKnockback, ForceMode2D.Impulse);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for attack animation to complete
        isAttacking = false; // Resume movement after attack
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length/2); // Wait for damage animation to complete
        Destroy(gameObject); // Destroy enemy after playing animation
    }
}
