using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
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
    
    private string url = "http://localhost:5000/results_json";

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

                    // Use the binary_output value in your game logic
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
            if (HRValue == 0)
            {
                //playerMovement.moveSpeed = 5.5f;
                //playerMovement.jumpForce = 16.5f;
                
                HRValueDisplay.text = "Calm"+ HRnumber.ToString() ;
                HRValueDisplay.color = Color.green;

                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor); 
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor); 
                cameraLight.color = cameraColor;
            }
            else
            {
                HRValueDisplay.text = "Anxious"+ HRnumber.ToString();
                HRValueDisplay.color = Color.red;
                //playerMovement.moveSpeed = 5f;
                //playerMovement.jumpForce = 18f;

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

