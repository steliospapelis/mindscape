using UnityEngine;
using System.Collections;

public class PatternDetection1 : MonoBehaviour
{
    float holdTimer;
    float timer;

    public HealthManager health;

    public GameObject particles;

    void OnEnable()
    {
        holdTimer = 0;
        StartCoroutine(Pattern());
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
