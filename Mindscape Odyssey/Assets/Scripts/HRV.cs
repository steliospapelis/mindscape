using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HRV : MonoBehaviour
{
    public Text HRValueDisplay;
    public int HRValue;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeText());
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            
            HRValue = Random.Range(600, 700);
            // Update text
            HRValueDisplay.text = "HRV: " + HRValue.ToString() + " ms";
            
            yield return new WaitForSeconds(30f);
        }
    }
}

