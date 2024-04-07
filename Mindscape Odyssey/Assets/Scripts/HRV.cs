using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HRV : MonoBehaviour
{
    public Text HRValue;
    private int random;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeText());
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            
            random = Random.Range(600, 700);
            // Update text
            HRValue.text = "HRV: " + random.ToString() + " ms";
            
            yield return new WaitForSeconds(30f);
        }
    }
}

