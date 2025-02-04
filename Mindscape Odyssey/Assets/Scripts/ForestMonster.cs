using UnityEngine;
using System;

public class ForestMonster : MonoBehaviour
{
    public float moveSpeed = 2.7f;
    public float detectionRange = 5f;
    public float dashDistance = 3f;
    public float dashChance = 0.5f;
    public float changeDirectionInterval = 3f;
    private Transform player;

    private bool isPatrolling = true;
    private bool isCombatMode = false;
    private bool isFreezeMode = false;
    private bool isAttacking = false;  // New flag to track ongoing attacks
    private Vector3 patrolDirection = Vector3.left;
    private float lastDirectionChangeTime;
    private float lastDashTime = 0;
    private float lastDamage = 0;
    private float dashCooldown = 0;

    private HealthManager health;
    private Animator anim;
    public float Damage = 5;
    private Rigidbody2D rbPlayer;
    private bool dying = false;
    public GaleneMovement galene;
    

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
        else if (isCombatMode && !isAttacking) // Prevent re-entering combat if currently attacking
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
        anim.SetBool("walk", true);
        transform.Translate(patrolDirection * moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.position) <= detectionRange && Math.Abs(player.position.y - transform.position.y) < 2)
        {
            isPatrolling = false;
            isCombatMode = true;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, patrolDirection, 2f);
        if ((hit.collider == null || Time.time - lastDirectionChangeTime >= changeDirectionInterval) && !dying)
        {
            patrolDirection = -patrolDirection;
            lastDirectionChangeTime = Time.time;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void Combat()
    {
        if (Vector3.Distance(transform.position, player.position) <= 1.2f)
        {
            isCombatMode = false;
            isFreezeMode = true;
            isAttacking = true;  // Set attacking flag to prevent re-entering combat mode
            return;
        }

        anim.SetBool("walk", true);

        if (Vector3.Distance(transform.position, player.position) > detectionRange || (Math.Abs(player.position.y - transform.position.y) >= 2 && galene.isGrounded==true))
        {
            isPatrolling = true;
            isCombatMode = false;
        }

        if (isAttacking) return;

        // Move towards the player
        transform.position = Vector3.MoveTowards(transform.position, player.position, 1.5f * moveSpeed * Time.deltaTime);
    }

    private void Freeze()
    {
        anim.SetBool("walk", false);
        anim.Play("attack");

        if (Time.time - lastDamage >= 1.0f)
        {
            health.TakeDamage(Damage);
            lastDamage = Time.time;
        }

        if (Vector3.Distance(transform.position, player.position) > 1.2f)
        {
            isFreezeMode = false;
            isCombatMode = true;
            isAttacking = false;  // Reset attacking flag after finishing attack
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            anim.SetBool("walk", false);
            anim.Play("attack");

            if (collision.contacts[0].normal.y < -0.85f && !dying)
            {
                dying = true;
                rbPlayer.velocity = new Vector2(rbPlayer.velocity.x, 0f);
                rbPlayer.AddForce(Vector2.up * 15, ForceMode2D.Impulse);
                anim.Play("demage");
                Destroy(gameObject, 0.5f);
            }
            else
            {
                health.TakeDamage(Damage);
                lastDamage = Time.time;
            }
        }
    }
}
