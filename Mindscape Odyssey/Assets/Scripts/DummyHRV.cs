using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class DummyHRV : MonoBehaviour
{
    public Text HRValueDisplay;
    public GaleneMovement playerMovement;

    public enum HRVState { Calm, Anxious }
    public HRVState state = HRVState.Calm; // Initialize state as calm

    public Light2D globalLight;
    public Light2D cameraLight;

    private void Start()
    {
        StartCoroutine(ChangeText());
    }

    private void Update()
    {
        // Toggle state between Calm and Anxious when pressing H
        if (Input.GetKeyDown(KeyCode.H))
        {
            state = (state == HRVState.Calm) ? HRVState.Anxious : HRVState.Calm;
        }
    }

   

    private IEnumerator ChangeText()
    {
        while (true)
        {
            if (state == HRVState.Calm)
            {
                // Adjust player and monster attributes for calm state
                HRValueDisplay.text = "Calm";
                HRValueDisplay.color = Color.green;
                
                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor); 
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor); 
                cameraLight.color = cameraColor;
            }
            else
            {
                // Adjust player and monster attributes for anxious state
                HRValueDisplay.text = "Anxious";
                HRValueDisplay.color = Color.red;
                ColorUtility.TryParseHtmlString("#B50F10", out Color anxiousColor); 
                globalLight.color = anxiousColor;
                cameraLight.color = anxiousColor;
            }

            yield return new WaitForSeconds(1f); // Adjust as needed for testing
        }
    }
}
