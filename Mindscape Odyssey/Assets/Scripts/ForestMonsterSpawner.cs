using UnityEngine;

public class ForestMonsterSpawner : MonoBehaviour
{
    private bool triggered = false;
    public GameObject monster;
    public Transform spawner;
    
    public Vector3[] spawnPoints;


    public int numEnemies;

    
   

    void OnTriggerEnter2D()
    {

        if (triggered==false){
       

       
        for (int i = 0; i < numEnemies; i++)
        {
            SpawnEnemy(i);
        }
      }
      triggered = true;
    }

    void SpawnEnemy(int index)
    {
        
        Vector3 spawnPos = spawnPoints[index];

        
        Instantiate(monster, spawnPos, Quaternion.identity);
    }
}
