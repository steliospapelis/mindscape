using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Boss : MonoBehaviour
{
    public CameraMovement cameraShake;
    public float speed = 1.5f; 
    public Transform targetPosition; 
    public float attackInterval = 4f; 
    public Animator anim; 
    public HealthManager health;
    private float lastAttackTime = 0f;
    private bool reachedTarget = false;

    public DeepBreathing deepBr;

    public Transform player;

     public GameObject rock;

    private void Start()
    {
        anim.SetBool("walk", true); // Start walking animation
    }

    private void Update()
    {
        if(deepBr){
            anim.SetBool("walk", false);
        }
        else{
            anim.SetBool("walk", true);
        }
        if(transform.position.x>225){
            health.bossOffset = 75;
        }
        else if(player.position.x-transform.position.x>55f){
            float newX = player.position.x - 50f;
            float currentY = transform.position.y;
            transform.position = new Vector3(newX, currentY, transform.position.z);
        }

        if(transform.position.x<150){
            float newY = 86f;
            float currentX = transform.position.x;
            transform.position = new Vector3(currentX, newY, transform.position.z);
        }
        
        if (!reachedTarget)
        {
            MoveTowardsTarget();
        }

        if (Time.time - lastAttackTime >= attackInterval && !deepBr.isBreathing)
        {
            PlayAttackAnimation();
            lastAttackTime = Time.time;
        }
    }

    private void FallingObjects()
    {
        for (int i = 0; i < 2; i++)
        {
            float randomX = player.position.x +Random.Range(0f, 30f);
            float randomY = player.position.y + 20f;
            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);
            
            Instantiate(rock, spawnPosition, Quaternion.identity);
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, speed * Time.deltaTime);

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
            FallingObjects();
            cameraShake.ShakeCamera(2f, 0.8f); 
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            health.TakeDamage(150);
            
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            health.TakeDamage(150);
            
        }
    }
}

    

