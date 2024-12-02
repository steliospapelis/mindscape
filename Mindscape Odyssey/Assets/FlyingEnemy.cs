using UnityEngine;
using System.Collections;

public class FlyingEnemy : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chargeSpeed = 8f;
    public float chargeRange = 10f;
    public float patrolAmplitude = 1f;
    public float patrolFrequency = 2f;
    public float pauseTime = 1f;
    public float attackDelay = 2f;

    public Rigidbody2D playerRB;

    private Transform player;

    private GaleneMovement playerMovement;
    private HealthManager health;
    private Vector3 StartingPosition;
    private Vector3 chargeStartingPosition;
    private Vector3 chargeTargetPosition;
    private bool isCharging = false;
    private bool isReturning = false;
    private float attackCooldown = 0f;
    private bool facingRight = true;
    private float previousPatrolX;
    private bool isPausing = false;
    private float patrolOffset; // Track patrol offset when charge starts
    private float chargeStartTime; // Track when the charge started

    private Vector3 returningPosition;

    public float horizontalKnockbackForce = 5f; // Adjust for horizontal knockback
    

    public int Damage = 20;

    public float bounceForce = 10f;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        StartingPosition = transform.position;
        

        playerRB = player.GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<GaleneMovement>();
        health = player.GetComponent<HealthManager>();
    }

    void Update()
{
    if (isCharging || isReturning)
    {
        HandleMovement();
    }
    else
    {
        if (Vector3.Distance(transform.position, player.position) <= chargeRange && attackCooldown <= 0f && !isPausing)
        {
            // Lock positions and patrol state
            chargeStartingPosition = transform.position;
            chargeTargetPosition = player.position;
            patrolOffset = Mathf.PingPong(Time.time * patrolSpeed, 12f); // Save patrol offset
            chargeStartTime = Time.time; // Save charge start time
            isCharging = true;
        }
        else if (!isPausing)
        {
            Patrol();
        }
    }

    if (attackCooldown > 0f)
    {
        attackCooldown -= Time.deltaTime;
    }
}

private void Patrol()
{
    float patrolX = Mathf.PingPong((Time.time * patrolSpeed), 12f) - 6f;
    float patrolY = Mathf.Sin((Time.time * patrolFrequency)) * patrolAmplitude;

    transform.position = new Vector3(StartingPosition.x + patrolX, StartingPosition.y + patrolY, StartingPosition.z);

    // Flip the enemy based on patrol movement direction
    if ((patrolX > previousPatrolX && !facingRight) || (patrolX < previousPatrolX && facingRight))
    {
        Flip();
    }

    previousPatrolX = patrolX;
}

void HandleMovement()
{
    if (isCharging)
    {
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, chargeTargetPosition, chargeSpeed * Time.deltaTime);

        // Flip based on charging direction
        if (chargeTargetPosition.x > transform.position.x && !facingRight)
        {
            Flip();  // Flip if moving right
        }
        else if (chargeTargetPosition.x < transform.position.x && facingRight)
        {
            Flip();  // Flip if moving left
        }

        if (Vector3.Distance(transform.position, chargeTargetPosition) < 1f)
        {
            StartCoroutine(PauseAtTarget());
        }
    }
    else if (isReturning)
    {
        // Calculate the position to return to
        float patrolX = Mathf.PingPong((Time.time * patrolSpeed), 12f) - 6f;
        float patrolY = Mathf.Sin((Time.time * patrolFrequency)) * patrolAmplitude;
        returningPosition = new Vector3(StartingPosition.x + patrolX, StartingPosition.y + patrolY, StartingPosition.z);

        // Move towards the returning position
        transform.position = Vector3.MoveTowards(transform.position, returningPosition, chargeSpeed * Time.deltaTime);

        // Flip based on returning direction
        if (returningPosition.x > transform.position.x && !facingRight)
        {
            Flip();  // Flip if moving right
        }
        else if (returningPosition.x < transform.position.x && facingRight)
        {
            Flip();  // Flip if moving left
        }

        if (Vector3.Distance(transform.position, returningPosition) < 0.2f)
        {
            isReturning = false;
            attackCooldown = attackDelay;
        }
    }
}

void Flip()
{
    facingRight = !facingRight;
    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
}

    IEnumerator PauseAtTarget()
    {
        isPausing = true;
        isCharging = false;
        yield return new WaitForSeconds(pauseTime);
        isReturning = true;
        isPausing = false;
    }

    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            // Check if player collided from the top
            if (collision.contacts[0].normal.y < -0.8f )
            {
                
              
                playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
                
                playerRB.velocity = new Vector2(playerRB.velocity.x, bounceForce);
                GetComponent<Collider2D>().enabled = false;
                patrolSpeed = 0f;
                chargeSpeed = 0f;
                StartCoroutine(Destroy()); 
            }
            else
            {
                
                    StartCoroutine(AttackPlayer()); 
                }
            }
        }
    

    private IEnumerator AttackPlayer()
{
    yield return new WaitForSeconds(0.1f); // Delay before dealing damage

    playerMovement.StartKnockback(1f);
    health.TakeDamage(Damage); // Deal damage to player

    // Stop player's current movement
    playerRB.velocity = Vector2.zero;

    // Get the direction of the knockback based on the enemy's velocity
    Vector2 knockbackDirection = (playerRB.position - (Vector2)transform.position).normalized;

    // Scale the knockback by combining enemy velocity and knockback forces
    Vector2 combinedKnockback = knockbackDirection * horizontalKnockbackForce;

    // Apply the knockback force
    playerRB.AddForce(combinedKnockback, ForceMode2D.Impulse);
}


    private IEnumerator Destroy()
{
    float duration = 1f;  // Total duration of the rotation before destruction
    float elapsedTime = 0f;
    float rotationSpeed = 120f;  // Degrees per second

    while (elapsedTime < duration)
    {
        // Rotate around the Z axis (or adjust axis as needed)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        elapsedTime += Time.deltaTime;
        yield return null;  // Wait for the next frame
    }

    Destroy(gameObject);  // Destroy enemy after rotation
}

}

