using UnityEngine;
using System.Collections;

public class PatternDetection : MonoBehaviour
{
    float holdTimer;

    public HealthManager health;

    void OnEnable()
    {
        Debug.Log("Enabled");
        holdTimer = 4;
        StartCoroutine(Pattern());
    }

    IEnumerator Pattern()
    {
        while (holdTimer > 0)
        {
            if (Input.GetButton("Jump"))
            {
                holdTimer -= Time.deltaTime;
            }
            else
            {
                holdTimer = 4;
            }
            yield return null; // Yield to next frame
        }

        holdTimer = 1;
        while (holdTimer > 0)
        {
            if (!Input.GetButton("Jump"))
            {
                holdTimer -= Time.deltaTime;
            }
            else
            {
                holdTimer = 1;
            }
            yield return null; // Yield to next frame
        }

        holdTimer = 6;
        while (holdTimer > 0)
        {
            if (Input.GetButton("Jump"))
            {
                holdTimer -= Time.deltaTime;
            }
            else
            {
                holdTimer = 6;
            }
            yield return null; // Yield to next frame
        }

        Debug.Log("Pattern Detected!");
        health.Healing(10);
        StartCoroutine(Pattern());
    }
}
