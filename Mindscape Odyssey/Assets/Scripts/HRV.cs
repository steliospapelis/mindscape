using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HRV : MonoBehaviour
{
    public Text HRValueDisplay;
    public int HRValue;

    
    void Start()
    {
        StartCoroutine(ChangeText());
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            
            HRValue = Random.Range(600, 700);
            
            if(HRValue>=650){
            HRValueDisplay.text = "Calm";
            HRValueDisplay.color = Color.green;
            }
            else{
            HRValueDisplay.text = "Anxious";
            HRValueDisplay.color = Color.red;

            }
            
            yield return new WaitForSeconds(15f);
        }
    }
}

