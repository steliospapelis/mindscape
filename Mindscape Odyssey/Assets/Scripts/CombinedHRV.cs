using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Rendering.Universal;

public class CombinedHRV : MonoBehaviour
{
    private float checkInterval = 10.0f;  // Change value every 10 seconds
    public TextMeshProUGUI HRValueDisplay;
    public int HRValue;
    public int HRnumber;  // Added for the HRV number display
    public GaleneMovement playerMovement;

    public Light2D globalLight;
    public Light2D cameraLight;

    private string url = "http://localhost:5000/results_json";
    private bool manualMode = false;  // Fallback to manual mode if server is unreachable

    void Start()
    {
        StartCoroutine(FetchData());
        StartCoroutine(ChangeText());
    }

    void Update()
    {
        // Toggle state manually if in manual mode and 'H' is pressed
        if (manualMode && Input.GetKeyDown(KeyCode.H))
        {
            HRValue = (HRValue == 0) ? 1 : 0;  
        }
    }

    IEnumerator FetchData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            // If there's an error, enable manual mode and exit the coroutine
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error fetching data: " + webRequest.error);
                manualMode = true;
            }
            else
            {
                // Successfully connected; continue fetching data at intervals
                manualMode = false;

                // Parse the JSON response
                var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                Debug.Log("Computed Value: " + result.binary_output);

                // Use the binary_output value in your game logic
                HRValue = result.binary_output;
                HRnumber = result.current_hrv;

                // Continue fetching at intervals
                StartCoroutine(FetchDataLoop());
            }
        }
    }

    IEnumerator FetchDataLoop()
    {
        while (!manualMode)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError("Error fetching data: " + webRequest.error);
                    manualMode = true;
                }
                else
                {
                    var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                    HRValue = result.binary_output;
                    HRnumber = result.current_hrv;
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            if (HRValue == 0)  // Calm state
            {
                HRValueDisplay.text = "Calm " + HRnumber.ToString();
                HRValueDisplay.color = Color.green;

                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor); 
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor); 
                cameraLight.color = cameraColor;
            }
            else  // Anxious state
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

    [System.Serializable]
    public class ValueResponse
    {
        public int binary_output;
        public int current_hrv;
    }
}
