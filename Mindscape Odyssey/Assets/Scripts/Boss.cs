using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;




public class Boss : MonoBehaviour
{
    public CameraMovement cameraShake;
    public float speed = 1.5f; 
    public Transform targetPosition; 
    public float attackInterval = 4f; 
    public Animator anim; 
    public HealthManager health;
    private float lastAttackTime = 0f;
    public bool reachedTarget = false;

    public DeepBreathing deepBr;

    public Transform player;

    public GameObject rock;

    public float slowMotionFactor = 0.5f;
    public float slowMotionDuration = 2f;

    private float originalTimeScale;

    private bool hasFallen = false;

    public AudioSource RoarAudioSource;
    public AudioSource AttackAudioSource;
    public AudioClip[] soundClips;
    private int currentClipIndex = 0;

    
    

    public void ActivateSlowMotion()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionFactor;
    }

    public void DeactivateSlowMotion()
    {
        Time.timeScale = originalTimeScale;
    }

    private void Start()
    {
        anim.SetBool("walk", true); // Start walking animation
        StartCoroutine(PlaySoundLoop());
    }

    private IEnumerator PlaySoundLoop()
    {
        while (true) // Loop indefinitely
        {
            if (soundClips.Length > 0 && !deepBr.isBreathing)
            {
                RoarAudioSource.clip = soundClips[currentClipIndex];
                RoarAudioSource.Play();

                // Move to the next clip index
                currentClipIndex = (currentClipIndex + 1) % soundClips.Length;
            }
            yield return new WaitForSeconds(10f); // Wait for 10 seconds
        }
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
            health.bossOffset = 95;
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

        

        if(player.position.x>317 && player.position.y>155 &&!reachedTarget){
            reachedTarget=true;
            transform.position = new Vector3(342f, 108f, transform.position.z);
            Vector3 Scale = transform.localScale;
                    Scale.x = -Scale.x;
                    transform.localScale = Scale;
           SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.sortingLayerName = "Enemies";
        }
            
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
            
        }
    }

    private void PlayAttackAnimation()
    {
        
        
            anim.Play("attack");
            AttackAudioSource.Play();
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

    

