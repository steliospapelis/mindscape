using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepBreathing : MonoBehaviour
{
    private bool isBreathingModeActive = false;
    
    public GaleneMovement galene;
    public Animator anim;

    public GameObject chart;

    private float originalMonsterSpeed=0;

    private Coroutine monsterStopCoroutine;

    

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleBreathingMode();
        }
    }

     private void StopMonsters()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("ForestMonster");
        bool isFirstMonster = true; 

        foreach (GameObject monster in monsters)
        {
            ForestMonster monsterScript = monster.GetComponent<ForestMonster>();
            if (monsterScript != null)
            {
                
                if (isFirstMonster && originalMonsterSpeed==0)
                {
                    originalMonsterSpeed = monsterScript.moveSpeed;
                    isFirstMonster = false; 
                }

                
                monsterScript.moveSpeed = 0.01f;
            }
        }
    }

     private void StartMonsters()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("ForestMonster");


        foreach (GameObject monster in monsters)
        {
            ForestMonster monsterScript = monster.GetComponent<ForestMonster>();
            if (monsterScript != null)
            {
                
                
                monsterScript.moveSpeed = originalMonsterSpeed;
            }
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
            monsterStopCoroutine = StartCoroutine(KeepStoppingMonsters());
            Debug.Log("Entering Deep Breathing Mode");
            
            
        }
        else
        {
            galene.canMove = true;
            anim.SetBool("Breathing",false);
            chart.SetActive(false);
            StopCoroutine(monsterStopCoroutine);
            StartMonsters();
            Debug.Log("Exiting Deep Breathing Mode");
            
        }
    }

     IEnumerator KeepStoppingMonsters()
    {
        while (isBreathingModeActive) // This loop runs until the coroutine is stopped
        {
            StopMonsters();
            yield return new WaitForSeconds(0.1f); // Wait for 1 second before calling it again
        }
    }
}

