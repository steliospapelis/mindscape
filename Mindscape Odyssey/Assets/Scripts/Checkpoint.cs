using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public HealthManager healthScript;
    public Transform respawn;
    
    private bool checkpointReached = false; // Ensures it only triggers once

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !checkpointReached)
        {
            healthScript.respawnPoint = respawn;
            checkpointReached = true; // Mark this checkpoint as activated
            
            // Find the GameLogger and increment checkpoint count
            DataLoggingTutorial loggerT = FindObjectOfType<DataLoggingTutorial>();
            if (loggerT != null)
            {
                loggerT.checkpoint+=1;
            }
            DataLogging logger = FindObjectOfType<DataLogging>();
            if (logger != null)
            {
                logger.checkpoint+=1;
            }
        }
    }
}
