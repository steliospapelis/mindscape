using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Add this to access TextMeshPro
using UnityEngine.Rendering.Universal; // Add this to access Light2D

public class HealthManager : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public Transform respawnPoint;
    public Renderer healthbar;
    private bool isRespawning = false;
    public Image fadeImage;
    private Animator anim;
    public float minYposition = -10;
    public float fadeSpeed = 0.5f;
    private float lastDamageTime; // Track the time since last damage
    public float damageCooldown = 1f; // Cooldown time in seconds
    public GaleneMovement galene;

    public DeepBreathing deepBr;

    private GameObject boss;
    public FallingBridge bridge1;
    public FallingBridge bridge2;


    // New variables for TextMeshPro and Light2D
    public TextMeshProUGUI healthWarningText;
    public Light2D globalLight;

    public int bossOffset;
   

    public bool breathingUnlocked=false;

    void Start()
    {
        bossOffset=30;
        anim = GetComponent<Animator>();
        maxHealth = 100;
        health = 85;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
        lastDamageTime = -damageCooldown; // Allow immediate damage at the start

        // Set initial states
        healthWarningText.gameObject.SetActive(false);
        UpdateLightIntensity();
    }

    void Update()
    {
        if (!isRespawning && (health <= 5 || transform.position.y < minYposition))
        {
            Respawn();
        }

        // Check health to activate TextMeshPro warning and update light intensity
        if (health < maxHealth / 2.5  && breathingUnlocked && !deepBr.isBreathing && deepBr.hasHealed)
        {
            healthWarningText.gameObject.SetActive(true);
            
        }
        else if (health >= maxHealth / 2.5 || !deepBr.hasHealed)
        {
            healthWarningText.gameObject.SetActive(false);
            
        }

        UpdateLightIntensity();
    }

    public void TakeDamage(float Damage)
    {
        // Check if enough time has passed since the last damage
        if (Time.time - lastDamageTime >= damageCooldown && !deepBr.isBreathing )
        {
            anim.Play("hit light",0,0f);
            health -= Damage;
            health = Mathf.Clamp(health, 0, maxHealth); // Ensure health stays within bounds
            healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
            lastDamageTime = Time.time; // Update the last damage time
        }
    }

    public void Healing(float healPoints)
    {
        health += healPoints;
        health = Mathf.Clamp(health, 0, maxHealth);
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
    }

    private void UpdateLightIntensity()
    {
        if (globalLight != null)
        {
            if (health > maxHealth / 1.5)
            {
                globalLight.intensity = 1f; // Brightest
            }
            else if (health > maxHealth / 2.5)
            {
                globalLight.intensity = 0.7f; // Medium brightness
            }
            else
            {
                globalLight.intensity = 0.40f; // Dimmest
            }
        }
    }

    public void Respawn()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            galene.canMove = false;
            
            galene.isKnockedDown = true;
            anim.SetBool("Grounded",true);
            anim.Play("knockdown", 0, 0f);  
            anim.SetBool("Down",true);
            StartCoroutine(FadeOutAndRespawn());
        }
    }

    IEnumerator FadeOutAndRespawn()
    {
        Color fadeColor = fadeImage.color;

        // Fade out screen
        while (fadeImage.color.a < 1)
        {
            fadeColor.a += fadeSpeed * Time.deltaTime;
            fadeImage.color = fadeColor;
            yield return null;
        }

        // Move player to the respawn point and reset health
        transform.position = respawnPoint.position;
        anim.SetBool("Down",false);
        anim.Play("idle", 0, 0);
        

        yield return new WaitForSeconds(1f);
        health = maxHealth/4;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
        galene.isKnockedDown = false;

        galene.canMove = true;

        boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            Vector3 newPos = boss.transform.position; 
            newPos.x = transform.position.x - bossOffset;     
            boss.transform.position = newPos;
        }
        if (bridge1 != null) bridge1.ResetBridge();
        if (bridge2 != null) bridge2.ResetBridge();
        // Fade in screen
        while (fadeImage.color.a > 0)
        {
            fadeColor.a -= fadeSpeed * Time.deltaTime;
            fadeImage.color = fadeColor;
            yield return null;
        }

        isRespawning = false;
    }
}
