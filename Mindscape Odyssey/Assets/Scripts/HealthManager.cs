using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        anim = GetComponent<Animator>();
        maxHealth = 85;
        health = 50;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
        lastDamageTime = -damageCooldown; // Allow immediate damage at the start
    }

    void Update()
    {
        if (!isRespawning && (health <= 15 || transform.position.y < minYposition))
        {
            Respawn();
        }
    }

    public void TakeDamage(float Damage)
    {
        // Check if enough time has passed since the last damage
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            anim.Play("hit light");
            health -= Damage;
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

    public void Respawn()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            anim.Play("knockdown");
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
        health = 50;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);

        yield return new WaitForSeconds(1f);

        anim.Play("recover");

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
