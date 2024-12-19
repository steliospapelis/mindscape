using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Rendering.Universal;

public class CombinedHRV : MonoBehaviour
{
    private float checkInterval = 10.0f;
    public TextMeshProUGUI HRValueDisplay;
    public int HRValue;
    public int HRnumber;
    public GaleneMovement playerMovement;

    public Light2D globalLight;
    public Light2D cameraLight;
    public TextMeshProUGUI anxiousWarningText; // Text to display when anxious

    private string url = "http://localhost:5000/analysis_results";
    private bool manualMode = false;

    void Start()
    {
        anxiousWarningText.gameObject.SetActive(false); // Hide text initially
        StartCoroutine(FetchData());
        StartCoroutine(ChangeText());
    }

    void Update()
    {
        if (manualMode && Input.GetKeyDown(KeyCode.H))
        {
            HRValue = (HRValue == 0) ? 1 : 0;
        }

        AdjustBehavior();
    }

    IEnumerator FetchData()
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
                manualMode = false;
                var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                HRValue = result.binary_output;
                HRnumber = result.current_hrv;
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
            if (HRValue == 0) 
            {
                HRValueDisplay.text = "Calm " + HRnumber.ToString();
                HRValueDisplay.color = Color.green;
                anxiousWarningText.gameObject.SetActive(false); 

                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor);
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor);
                cameraLight.color = cameraColor;
            }
            else 
            {
                HRValueDisplay.text = "Anxious " + HRnumber.ToString();
                HRValueDisplay.color = Color.red;
                anxiousWarningText.gameObject.SetActive(true); 

                ColorUtility.TryParseHtmlString("#B50F10", out Color anxiousColor);
                globalLight.color = anxiousColor;
                cameraLight.color = anxiousColor;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void AdjustBehavior()
{
    FlyingEnemy[] flyingEnemies = FindObjectsOfType<FlyingEnemy>();
    NewMonster[] NewMonsters = FindObjectsOfType<NewMonster>();

    if (HRValue == 1)  // Anxious state
    {
        foreach (FlyingEnemy enemy in flyingEnemies)
        {
            enemy.Damage = 25;
            enemy.chargeSpeed = 9f;  
            enemy.pauseTime = 1f;
        }

        foreach (NewMonster monster in NewMonsters)
        {
            monster.Damage = 15;
             if(Mathf.Abs(monster.speed)<=3.05f){
            monster.speed = monster.speed*1.2f;
            }
        }

        playerMovement.moveSpeed = 5f;  // Anxious state
    }
    else  // Calm state
    {
        foreach (FlyingEnemy enemy in flyingEnemies)
        {
            enemy.Damage = 20;
            enemy.chargeSpeed = 8f;
            enemy.pauseTime = 1.2f;
        }
       

        foreach (NewMonster monster in NewMonsters)
        {
            monster.Damage = 10;
            if(Mathf.Abs(monster.speed)>3.6f){
            monster.speed = monster.speed/1.2f;
            }
        }

        playerMovement.moveSpeed = 5.8f;  // Calm state
    }
}


    [System.Serializable]
    public class ValueResponse
    {
        public int binary_output;
        public int current_hrv;
    }

    
//StartCoroutine(NotifyPlayerState(true, false, false)); // Example usage
}