using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



public class HRV : MonoBehaviour
{
    private string url = "http://localhost:5000/get_value";
    private float checkInterval = 1.0f;
    public Text HRValueDisplay;
    public int HRValue;

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
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    // Parse the JSON response
                    var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                    Debug.Log("Computed Value: " + result.value);

                    // Use the value in your game logic
                    HRValue = result.value;
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    IEnumerator ChangeText()
    {
        while (true)
        {

            if (HRValue == 1)
            {
                HRValueDisplay.text = HRValue.ToString();
                HRValueDisplay.color = Color.green;
            }
            else
            {
                HRValueDisplay.text = HRValue.ToString();
                HRValueDisplay.color = Color.red;

            }

            yield return new WaitForSeconds(1f);
        }
    }

    
  

    [System.Serializable]
    private class ValueResponse
    {
        public int value;
    }
}
