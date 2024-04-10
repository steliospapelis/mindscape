using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepBreathing : MonoBehaviour
{
    private bool isBreathingModeActive = false;
    
    public GaleneMovement galene;
    public Animator anim;

    public GameObject chart;

    

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleBreathingMode();
        }
    }

    void ToggleBreathingMode()
    {
        
        isBreathingModeActive = !isBreathingModeActive;

    

        
        if (isBreathingModeActive)
        {
            galene.canMove = false;
            anim.SetBool("Breathing",true);
            chart.SetActive(true);
            Debug.Log("Entering Deep Breathing Mode");
            
        }
        else
        {
            galene.canMove = true;
            anim.SetBool("Breathing",false);
            chart.SetActive(false);
            Debug.Log("Exiting Deep Breathing Mode");
            
        }
    }
}

