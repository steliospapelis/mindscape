using UnityEngine;

public class ForestMonster : MonoBehaviour
{
    private Transform player; // Reference to the player's transform
    public float moveSpeed = 1f; // Speed of movement towards the player
    public float attackRange = 3f; // Distance at which the enemy attacks the player
    public float attackCooldown = 3f; // Cooldown between attacks
    public float dashChance = 0.5f; // Chance of dashing when the player is above the enemy's head

    private Animator animator; // Reference to the animator component
    private bool isFacingRight = true; // Flag to track the direction the enemy is facing
    private bool isAttacking = false; // Flag to track if the enemy is currently attacking
    private float lastAttackTime = 0f; // Time of the last attack

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        
        
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed* Time.deltaTime);
        
        

        // Check attack range and cooldown
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("walk",false);
            Attack();
        }
        else{
            animator.SetBool("walk",true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
            Die();
        
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the player is above the enemy's head and dash randomly
        if (other.gameObject.CompareTag("Player") && Random.value < dashChance)
        {
            // Perform dash
            Vector3 dashDirection = new Vector3(Random.Range(-3f, 3f), 0f, 0f).normalized;
            transform.Translate(dashDirection * moveSpeed * Time.deltaTime * 5f);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void Attack()
    {
        
        isAttacking = true;
        animator.SetTrigger("attack");
        lastAttackTime = Time.time;
    }

    void Die()
    {
        // Play death animation, disable collider, etc.
        Destroy(gameObject);
    }
}
