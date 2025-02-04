using UnityEngine;
using System.Collections;

public class ParticlesAudioTrigger : MonoBehaviour
{
    public AudioSource audioSource; // The audio source to play
    public float fadeDuration = 1f; // Duration for fading out the audio

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Check if the triggering object is the player
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.5f; // Reset volume to full
                audioSource.Play();
            }
        }
    }

     private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Check if the triggering object is the player
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 1f; // Reset volume to full
                audioSource.Play();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Check if the exiting object is the player
        {
            StartCoroutine(FadeOutAudio());
        }
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;

        // Gradually reduce the volume
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop(); // Stop the audio when volume reaches zero
        audioSource.volume = startVolume; // Reset volume for the next play
    }
}
