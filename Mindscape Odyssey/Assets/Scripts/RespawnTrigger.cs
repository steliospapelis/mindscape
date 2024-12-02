using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    public HealthManager healthManager;  // Reference to the HealthManager script

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that enters the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Call the Respawn function in the HealthManager script
            healthManager.Respawn();
        }
    }
}

