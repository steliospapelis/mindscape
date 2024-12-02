using UnityEngine;

public class HorFloatingParticles : MonoBehaviour
{
    public CombinedHRV hrvScript; // Reference to the HRV script to check Galene's state
    public Rigidbody2D galeneRb; // Reference to Galene's Rigidbody2D
    public ParticleSystem particleSystem; // Particle system for visual effects
    public float floatForce = 55f; // Adjustable force applied when calm/anxious

    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    public GaleneMovement galeneMovement;

    private void Start()
    {
        // Initialize particle system modules for easy access
        mainModule = particleSystem.main;
        emissionModule = particleSystem.emission;

        // Activate particle emission by default
        emissionModule.enabled = true;
    }

    private void Update()
    {
        // Update particle properties based on the HRV state every frame
        if (hrvScript.HRValue == 0)
        {
            SetParticleSystem(Vector2.right, Color.green, transform.position + new Vector3(8, 0, 0)); // Move right
        }
        else
        {
            SetParticleSystem(Vector2.left, Color.red, transform.position + new Vector3(-18, 0, 0)); // Move left
        }
    }

 private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        // Get the upward force from maxGravityScale
        Vector2 upwardForce = Vector2.up * galeneMovement.maxGravityScale;

        if (hrvScript.HRValue == 0)
        {
            // Apply an impulse force to the right and upward when Galene enters the float zone
            galeneMovement.StartKnockback(2f);
            galeneRb.AddForce(Vector2.left * floatForce + upwardForce, ForceMode2D.Impulse);
        }
        else
        {
            galeneMovement.StartKnockback(2f);
            // Apply an impulse force to the left and upward when Galene enters the float zone
            galeneRb.AddForce(Vector2.right * floatForce + upwardForce, ForceMode2D.Impulse);
        }
    }
}

    private void SetParticleSystem(Vector2 direction, Color color, Vector3 position)
    {
        // Update particle system direction, color, and position
        mainModule.startColor = color;
        particleSystem.transform.position = position;

        var shape = particleSystem.shape;
        shape.rotation = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, direction)); // Set rotation for horizontal movement
    }
}
