using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;

public class ServerNotifications : MonoBehaviour
{

  public IEnumerator NotifyServer(bool isBreathing, bool isCalming, bool isStressing, bool isStressing2, bool isStressing3, bool startAnalysis)
{
    CalibrationData data = new CalibrationData
    {
        Breathing = isBreathing,
        CalmCalib = isCalming,
        StressedCalib = isStressing,
        StressedCalib2 = isStressing2,
        StressedCalib3= isStressing3,
        DataAnalysis = startAnalysis
    };
    Debug.Log("data:"+data);
    
    string jsonData = JsonUtility.ToJson(data);
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

    using (UnityWebRequest webRequest = new UnityWebRequest("http://localhost:5000/game_flags", "POST"))
    {
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        Debug.Log("JSON data: " + jsonData);


        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Flags sent successfully!");
        }
        else
        {
            Debug.LogError("Error sending flags: " + webRequest.error);
        }
    }
}

[System.Serializable]
public class CalibrationData
{
    public bool Breathing;
    public bool CalmCalib;
    public bool StressedCalib;
    public bool StressedCalib2;

    public bool StressedCalib3;

    public bool DataAnalysis;
}
}
