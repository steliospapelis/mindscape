using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damagePerSecond = 10; 
    private HealthManager healthManager;
    private bool playerInside = false;

    void Start()
    {
        
        healthManager = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthManager>();
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            healthManager.TakeDamage(damagePerSecond);
            playerInside = true;

            InvokeRepeating("DealDamageOverTime", 1f, 1f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            
            CancelInvoke("DealDamageOverTime");
        }
    }

    void DealDamageOverTime()
    {
        if (playerInside)
        {
            
            healthManager.TakeDamage(damagePerSecond);
        }
    }
}

