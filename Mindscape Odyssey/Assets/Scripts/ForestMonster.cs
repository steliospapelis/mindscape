using UnityEngine;
using System;

public class ForestMonster : MonoBehaviour
{
    public float moveSpeed = 2.7f; // Speed of patrol movement
    public float detectionRange = 5f; // Range at which enemy detects player
    public float dashDistance = 3f; // Distance the enemy dashes when in combat mode
    public float dashChance = 0.5f; // Chance of dashing when in combat mode
    public float changeDirectionInterval = 3f; // Interval at which patrol direction changes if not reaching an edge
    private Transform player; // Reference to the player object

    private bool isPatrolling = true; // Flag indicating whether enemy is patrolling
    private bool isCombatMode = false; // Flag indicating whether enemy is in combat mode

    private bool isFreezeMode = false;
    private Vector3 patrolDirection = Vector3.left; // Direction of patrol movement
    private float lastDirectionChangeTime; // Time when patrol direction was last changed
    private float lastDashTime =0 ;

    private float lastDamage=0;

    private float dashCooldown =0 ;

    private HealthManager health;

    private Animator anim;

    public float Damage = 5;

    private Rigidbody2D rbPlayer;

    private bool dying =false;


    private void Start()
    {
        anim = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.transform;
        health = playerObject.GetComponent<HealthManager>();
        lastDirectionChangeTime = Time.time;
        rbPlayer = playerObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isPatrolling)
        {
            Patrol();
        }
        else if (isCombatMode)
        {
            Combat();
        }
        else if (isFreezeMode)
        {
            Freeze();
        }
    }

    private void Patrol()
    {
        
        anim.SetBool("walk",true);
        transform.Translate(patrolDirection * moveSpeed * Time.deltaTime);

        // Check for player within detection range
        if (Vector3.Distance(transform.position, player.position) <= detectionRange && (Math.Abs(player.position.y - transform.position.y)<2))
        {
            isPatrolling = false;
            isCombatMode = true;
        }

        // Change patrol direction if reaching the edge of platform or after a certain interval
        
        
            RaycastHit2D hit = Physics2D.Raycast(transform.position, patrolDirection, 2f);
            if (hit.collider == null || (Time.time - lastDirectionChangeTime >= changeDirectionInterval) && dying==false)
            {
                patrolDirection = -patrolDirection;
                lastDirectionChangeTime = Time.time;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
           
        
    }

    private void Combat()
    {
        
        
        if((player.position - transform.position).normalized.x<0 && patrolDirection==Vector3.right && dying==false){
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            patrolDirection = -patrolDirection;
        }
        if((player.position - transform.position).normalized.x>0 && patrolDirection==Vector3.left&& dying==false){
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            patrolDirection = -patrolDirection;
        }

        anim.SetBool("walk",true);
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, 2f);
        if (hit.collider != null){
                    // Move towards the player
        transform.position = Vector3.MoveTowards(transform.position, player.position, 1.5f*moveSpeed * Time.deltaTime);
        }
        else{
            anim.SetBool("walk",false);
        }
        if (Vector3.Distance(transform.position, player.position) > detectionRange || (Math.Abs(player.position.y - transform.position.y)>=2))
        {
            isPatrolling = true;
            isCombatMode = false;
        }

        if (Vector3.Distance(transform.position, player.position) <= 1.2f)
        {
            
            isCombatMode = false;
            isFreezeMode = true;
        }
            
        // Check if the enemy should dash
       /* if (Time.time - lastDashTime >= dashCooldown)
        {
            if (Random.value>0.5){
            transform.position = Vector2.MoveTowards(transform.position, Vector3.right, 120*moveSpeed * Time.deltaTime);
            }
            else{
            transform.position = Vector2.MoveTowards(transform.position, Vector3.left, 120*moveSpeed * Time.deltaTime);

            }
            lastDashTime = Time.time;
            dashCooldown = Random.Range(3f, 5f);
        } */
        
        }    

    private void Freeze(){
        anim.SetBool("walk",false);
    
        anim.Play("attack");

        if(Time.time - lastDamage >= 1.0f){

        health.TakeDamage(Damage);  
        lastDamage=Time.time;

        }

        if (Vector3.Distance(transform.position, player.position) >= 1.2f)
        {
            
            isCombatMode = true;
            isFreezeMode = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        // Check if player jumps on top of the enemy
        if (collision.gameObject.CompareTag("Player"))
        {
            
            anim.SetBool("walk",false);
            anim.Play("attack");
            if(collision.contacts[0].normal.y < -0.85f && dying==false){
            dying = true;
            rbPlayer.velocity = new Vector2(rbPlayer.velocity.x, 0f); 
            rbPlayer.AddForce(Vector2.up*10, ForceMode2D.Impulse);
            anim.Play("demage");
            Destroy(gameObject,0.5f);
            }
            else{
            health.TakeDamage(Damage); 
            lastDamage=Time.time;   
            }
        }
        
    }

    
}
