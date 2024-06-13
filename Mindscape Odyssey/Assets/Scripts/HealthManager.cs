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

    public Image screenFadeImage;
    public Color fadeColor = Color.black;

    private Animator anim;

    public float minYposition = -10;

    void Start()
    {
        anim = GetComponent<Animator>();
        screenFadeImage.color = Color.clear;
        maxHealth = 85;
        health = 50;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
    }

    void Update()
    {
        if (health <= 15)
        {
            anim.Play("knockdown");
            Respawn();
        }

        if (!isRespawning && transform.position.y < minYposition)
        {
            Respawn();
        }
    }

    public void TakeDamage(float Damage)
    {
        anim.Play("hit light");
        health -= Damage;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);
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
            StartCoroutine(FadeOutAndRespawn());
        }
    }

    IEnumerator FadeOutAndRespawn()
    {
        

        // Fade out screen
        float startTime = Time.time;
        while (Time.time - startTime < 0.7f)
        {
            float normalizedTime = (Time.time - startTime) / 0.7f;
            fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            Color currentColor = Color.Lerp(Color.clear, fadeColor, normalizedTime);
            screenFadeImage.color = currentColor;
            yield return null;
        }

        // Move player to respawn point
        transform.position = respawnPoint.position;
        health = 50;
        healthbar.sharedMaterial.SetFloat("_Progress", health / 100);

        // Wait for a moment before fading in
        yield return new WaitForSeconds(1f);

        anim.Play("recover");
        // Fade in screen
        startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            float normalizedTime = (Time.time - startTime) / 1f;
            Color currentColor = Color.Lerp(fadeColor, Color.clear, normalizedTime);
            screenFadeImage.color = currentColor;
            yield return null;
        }
        

        isRespawning = false;
    }
}
