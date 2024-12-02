using UnityEngine;

public class MagnetizedParticles : MonoBehaviour
{
    public ParticleSystem particleSystem; // Reference to the ParticleSystem
    public Transform target; // The target object particles move toward
    public float attractionStrength = 5f; // Speed of movement toward the target
    public float stopDistance = 0.1f; // Minimum distance to stop particles near the target

    private ParticleSystem.Particle[] particles;

    void LateUpdate()
    {
        if (particleSystem == null || target == null)
            return;

        // Allocate particle array if necessary
        if (particles == null || particles.Length < particleSystem.main.maxParticles)
            particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];

        // Get the particles
        int particleCount = particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            
            // Calculate the direction toward the target
            Vector3 directionToTarget = (target.position + new Vector3(0,1,0)- particles[i].position);

            // Move particles if they are far enough from the target
            if (directionToTarget.magnitude > stopDistance)
            {
                // Normalize the direction and adjust the position
                Vector3 movement = directionToTarget.normalized * attractionStrength * Time.deltaTime;
                particles[i].position += movement;
            }
        }

        // Apply the updated particles back to the system
        particleSystem.SetParticles(particles, particleCount);
    }
}
