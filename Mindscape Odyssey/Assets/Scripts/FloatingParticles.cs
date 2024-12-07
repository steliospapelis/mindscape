using UnityEngine;
using System.Collections;

public class FloatingParticles : MonoBehaviour
{
    public CombinedHRV hrvScript; // Reference to the HRV script to check Galene's state
    public Rigidbody2D galeneRb; // Reference to Galene's Rigidbody2D
    public ParticleSystem particleSystem; // Particle system for visual effects
    public float floatForce = 55f; // Adjustable force applied when calm/anxious

    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    private bool up;

    

    private void Start()
    {
        // Initialize particle system modules for easy access
        mainModule = particleSystem.main;
        emissionModule = particleSystem.emission;

        // Activate particle emission by default
        emissionModule.enabled = true;
        if (hrvScript.HRValue == 0){
            up = true;
            SetParticleSystem(Vector2.up, Color.green, transform.position + new Vector3(0, -5, 0));
        }
        else{
            up = false;
            SetParticleSystem(Vector2.down, Color.red, transform.position + new Vector3(0, 15, 0));
        }

        
    }

    private void Update()
    {
        // Update particle properties based on the HRV state every frame
        if (hrvScript.HRValue == 0 && !up)
        {
            SetParticleSystem(Vector2.up, Color.green, transform.position + new Vector3(0, -5, 0));
            up=true;
        }
        else if(hrvScript.HRValue == 1 && up)
        {
            SetParticleSystem(Vector2.down, Color.red, transform.position + new Vector3(0, 15, 0));
            up=false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hrvScript.HRValue == 0)
            {
                Vector2 currentVelocity = galeneRb.velocity;
                galeneRb.velocity = new Vector2(currentVelocity.x, 0f);
                // Apply upward force when Galene is within the float zone
                galeneRb.AddForce(Vector2.up * floatForce, ForceMode2D.Force);
            }
            else
            {
                // Apply downward force when Galene is within the float zone
                galeneRb.AddForce(Vector2.down * floatForce/50, ForceMode2D.Force);
            }
        }
    }
private void SetParticleSystem(Vector2 direction, Color color, Vector3 position)
{

    particleSystem.Clear();
    // Update particle system direction, color, and position
    mainModule.startColor = color;
    particleSystem.transform.position = position;

    var shape = particleSystem.shape;
    shape.rotation = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, direction));
    particleSystem.Clear();

    // Re-enable the particle system after a delay
}

}
