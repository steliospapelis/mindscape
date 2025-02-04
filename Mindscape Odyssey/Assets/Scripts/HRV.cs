using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Rendering.Universal;

public class HRV : MonoBehaviour
{
    private float checkInterval = 10.0f;  // Change value every 10 seconds
    public TextMeshProUGUI HRValueDisplay;
    public int HRValue;

    public int HRnumber;
    public GaleneMovement playerMovement;

    public Light2D globalLight;
    public Light2D cameraLight;

    public AudioSource calmAudioSource;
    public AudioSource anxiousAudioSource;
    private float fadeDuration = 2.0f; // Duration for fading audio

    private string url = "http://localhost:5000/results_json";

    private bool isCalm = true; // Track the current state

    void Start()
    {
        StartCoroutine(FetchData());
        StartCoroutine(ChangeText());
    }

    IEnumerator FetchData()
    {
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError("Error fetching data: " + webRequest.error);
                }
                else
                {
                    // Parse the JSON response
                    var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                    Debug.Log("Computed Value: " + result.binary_output);

                    HRValue = result.binary_output;
                    HRnumber = result.current_hrv;

                    // Handle state changes and audio transitions
                    if (HRValue == 0 && !isCalm)
                    {
                        StartCoroutine(SwitchToCalmState());
                        isCalm = true;
                    }
                    else if (HRValue != 0 && isCalm)
                    {
                        StartCoroutine(SwitchToAnxiousState());
                        isCalm = false;
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            if (HRValue == 0)
            {
                HRValueDisplay.text = "Calm " + HRnumber.ToString();
                HRValueDisplay.color = Color.green;

                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor);
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor);
                cameraLight.color = cameraColor;
            }
            else
            {
                HRValueDisplay.text = "Anxious " + HRnumber.ToString();
                HRValueDisplay.color = Color.red;

                ColorUtility.TryParseHtmlString("#B50F10", out Color anxiousColor);
                globalLight.color = anxiousColor;
                cameraLight.color = anxiousColor;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SwitchToCalmState()
    {
        // Fade out anxious audio and fade in calm audio
        yield return StartCoroutine(FadeOut(anxiousAudioSource));
        yield return StartCoroutine(FadeIn(calmAudioSource));
    }

    IEnumerator SwitchToAnxiousState()
    {
        // Fade out calm audio and fade in anxious audio
        yield return StartCoroutine(FadeOut(calmAudioSource));
        yield return StartCoroutine(FadeIn(anxiousAudioSource));
    }

    IEnumerator FadeOut(AudioSource audioSource)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for future use
    }

    IEnumerator FadeIn(AudioSource audioSource)
    {
        audioSource.Play();
        float startVolume = 0f;
        audioSource.volume = startVolume;

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = 1f; // Ensure volume is fully restored
    }

    [System.Serializable]
    public class ValueResponse
    {
        public int binary_output;
        public int current_hrv;
    }
}
