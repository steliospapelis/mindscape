using System.Collections;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public int startTime=0;

    private void Start()
    {
            audioSource.time = startTime; // Set the playback position
            audioSource.Play();
        
    }

        

    // Public method to fade out the volume over a duration
    public void FadeOut(float fadeDuration)
    {
        StartCoroutine(FadeOutRoutine(fadeDuration));
    }

    private IEnumerator FadeOutRoutine(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}
