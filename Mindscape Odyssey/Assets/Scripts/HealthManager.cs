using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public float health;
    public float maxHealth;

    public Renderer healthbar;


    
    void Start()
    {
        maxHealth=100;
        health=50;
        healthbar.sharedMaterial.SetFloat("_Progress", health/100);

    }

    
    void Update()
    {
    if(health<=0){
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
    }

}
