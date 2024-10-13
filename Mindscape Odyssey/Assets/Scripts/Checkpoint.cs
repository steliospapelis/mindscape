using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    public HealthManager healthScript;

    public Transform respawn;
    
     private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            healthScript.respawnPoint = respawn;
        }
    }
}
