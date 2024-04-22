using System.Collections;
using System.Collections.Generic;
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

    public float minYposition=-10;


    
    void Start()
    {
        screenFadeImage.color = Color.clear;
        maxHealth=100;
        health=50;
        healthbar.sharedMaterial.SetFloat("_Progress", health/100);

    }

    
    void Update()
    {
    if(health<=0){
        
        Respawn();
    }

    if (!isRespawning && transform.position.y < minYposition)
        {
            Respawn();
        }
        
    }

     public void TakeDamage(float Damage){
         health -=Damage;  
         healthbar.sharedMaterial.SetFloat("_Progress", health/100);
    }

    public void  Healing(float healPoints){

        health += healPoints;
        health = Mathf.Clamp(health,0,maxHealth);
        healthbar.sharedMaterial.SetFloat("_Progress", health/100);
    }

    public void Respawn(){
        health=50;
        healthbar.sharedMaterial.SetFloat("_Progress", health/100);
        isRespawning = true;
        StartCoroutine(FadeOutAndRespawn());
    }

     IEnumerator FadeOutAndRespawn()
    {
        
        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            float normalizedTime = (Time.time - startTime) / 1f;
            fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

            Color currentColor = Color.Lerp(Color.clear, fadeColor, normalizedTime);
            screenFadeImage.color = currentColor;
            yield return null;
        }

        
        transform.position = respawnPoint.position;

        
        yield return new WaitForSeconds(3.5f);

        
        startTime = Time.time;
        while (Time.time - startTime < 0.5f)
        {
            float normalizedTime = (Time.time - startTime) / 0.5f;
            

            Color currentColor = Color.Lerp(fadeColor, Color.clear, normalizedTime);
            
            screenFadeImage.color = currentColor;
            yield return null;
        }

        
        isRespawning = false;
    }

}
