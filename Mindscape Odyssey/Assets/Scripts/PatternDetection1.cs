using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro;

public class PatternDetection1 : MonoBehaviour
{
    float holdTimer;
    float timer;

    public TextMeshProUGUI help;

    public HealthManager health;

    public GameObject particles;

    void OnEnable()
    {
        holdTimer = 0;
        StartCoroutine(Pattern());
    }

    void OnDisable()
    {
        particles.SetActive(false);
    }

    void Update(){
        timer += Time.deltaTime;

        if(holdTimer>=10){
            holdTimer=0;
            health.Healing(10);
        }
    }

    IEnumerator Pattern()
    {
        timer=0;
        while (timer < 4)
        {
            help.text="Breath In";
            if (Input.GetButton("Jump"))
            {
                holdTimer += Time.deltaTime;
                particles.SetActive(true);
            }
            else
            {
                particles.SetActive(false);
            }
            yield return null; // Yield to next frame
        }
        while (timer < 5.1666f)
        {
            help.text="Hold";
            if (!Input.GetButton("Jump"))
            {
                holdTimer += Time.deltaTime;
                particles.SetActive(true);
            }
            else
            {
                particles.SetActive(false);
            }
            yield return null; // Yield to next frame
        }
        while (timer < 11.1666f)
        {
            help.text="Breath Out";
            if (Input.GetButton("Jump"))
            {
                holdTimer += Time.deltaTime;
                particles.SetActive(true);
            }
            else
            {
                particles.SetActive(false);
            }
            yield return null; // Yield to next frame
        }
        while (timer < 12.2666f)
        {
            help.text="Hold";
            if (!Input.GetButton("Jump"))
            {
                holdTimer += Time.deltaTime;
                particles.SetActive(true);
            }
            else
            {
                particles.SetActive(false);
            }
            yield return null; // Yield to next frame
        }
        StartCoroutine(Pattern());
    }
}
