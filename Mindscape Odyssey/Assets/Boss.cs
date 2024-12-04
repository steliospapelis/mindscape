using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Boss : MonoBehaviour
{
    public CameraMovement cameraShake;
    public float moveSpeed = 1.5f; 
    public Transform targetPosition; 
    public float attackInterval = 4f; 
    public Animator anim; 
    public HealthManager health;
    private float lastAttackTime = 0f;
    private bool reachedTarget = false;

    private void Start()
    {
        anim.SetBool("walk", true); // Start walking animation
    }

    private void Update()
    {
        if(transform.position.x>225){
            health.bossOffset = 50;
        }
        if (!reachedTarget)
        {
            MoveTowardsTarget();
        }

        if (Time.time - lastAttackTime >= attackInterval)
        {
            PlayAttackAnimation();
            lastAttackTime = Time.time;
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);

        // Check if the monster has reached the target position
        if (Vector3.Distance(transform.position, targetPosition.position) < 0.1f)
        {
            anim.SetBool("walk", false); // Stop walking animation
            reachedTarget = true;
        }
    }

    private void PlayAttackAnimation()
    {
        
        
            anim.Play("attack");
            cameraShake.ShakeCamera(2f, 0.8f); 
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            health.TakeDamage(150);
            
        }
    }
}

    

