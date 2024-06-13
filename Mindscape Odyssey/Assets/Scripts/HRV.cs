using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



public class HRV : MonoBehaviour
{
<<<<<<< Updated upstream
    private string url = "http://localhost:5000/get_value";
    private float checkInterval = 1.0f;
=======
    private float checkInterval = 10.0f;  // Change value every 10 seconds
>>>>>>> Stashed changes
    public Text HRValueDisplay;
    public int HRValue;
    public GaleneMovement playerMovement;
    public Image overlayImage;  // Reference to the UI Image component

    void Start()
    {
        StartCoroutine(FetchData());
        StartCoroutine(ChangeText());
        StartCoroutine(ChangeLighting());  // Start the coroutine for lighting change
    }

    private void UpdateMonsterAttributes(Vector3 scale, float moveSpeed)
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("ForestMonster");
        foreach (GameObject monster in monsters)
        {
            float originalXSign = Mathf.Sign(monster.transform.localScale.x);

            Vector3 newScale = new Vector3(scale.x * originalXSign, scale.y, scale.z);
            monster.transform.localScale = newScale;

            ForestMonster monsterScript = monster.GetComponent<ForestMonster>();
            if (monsterScript != null && monsterScript.moveSpeed > 0.05)
            {
                monsterScript.moveSpeed = moveSpeed;
            }
        }
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
<<<<<<< Updated upstream

            if (HRValue == 1)
            {
                HRValueDisplay.text = HRValue.ToString();
                HRValueDisplay.color = Color.green;
            }
            else
            {
                HRValueDisplay.text = HRValue.ToString();
                HRValueDisplay.color = Color.red;
=======
            if (HRValue == 1)
            {
                playerMovement.moveSpeed = 4f;
                playerMovement.jumpForce = 14f;
                UpdateMonsterAttributes(new Vector3(0.3f, 0.3f, 1f), 2.7f);
                HRValueDisplay.text = "Calm";
                HRValueDisplay.color = Color.green;
            }
            else
            {
                HRValueDisplay.text = "Anxious";
                HRValueDisplay.color = Color.red;
                playerMovement.moveSpeed = 3.6f;
                playerMovement.jumpForce = 13f;
                UpdateMonsterAttributes(new Vector3(0.4f, 0.4f, 1f), 3f);
            }
>>>>>>> Stashed changes

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ChangeLighting()
    {
        float targetAlpha;
        float duration = 1.0f;  // Duration of the change in seconds

        while (true)
        {
            targetAlpha = (HRValue == 1) ? 0f : 0.55f;  // No overlay when calm, darker overlay when anxious
            Debug.Log("Target Alpha: " + targetAlpha);

            float initialAlpha = overlayImage.color.a;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                Color color = overlayImage.color;
                color.a = Mathf.Lerp(initialAlpha, targetAlpha, elapsedTime / duration);
                overlayImage.color = color;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

<<<<<<< Updated upstream
            yield return new WaitForSeconds(1f);
        }
    }

    
  

    [System.Serializable]
    private class ValueResponse
    {
        public int value;
    }
=======
            Color finalColor = overlayImage.color;
            finalColor.a = targetAlpha;
            overlayImage.color = finalColor;
            Debug.Log("Overlay Alpha: " + overlayImage.color.a);
            yield return new WaitForSeconds(checkInterval);  // Wait for the next value change
        }
    }
>>>>>>> Stashed changes

