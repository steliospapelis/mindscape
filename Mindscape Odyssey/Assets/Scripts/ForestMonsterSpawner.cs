using UnityEngine;

public class ForestMonsterSpawner : MonoBehaviour
{
    private bool triggered = false;
    public GameObject monster;
    public Transform spawner;
    public float spawnRadius = 5f; 

    public HRV HRV;

    
   

    void OnTriggerEnter2D()
    {

        if (triggered==false){
        int HRValue = HRV.HRValue;

        
        int numEnemies = 0;
        if (HRValue > 670)
        {
            numEnemies = 1;
        }
        else if (HRValue >= 630 && HRValue <= 670)
        {
            numEnemies = 2;
        }
        else if (HRValue < 630)
        {
            numEnemies = 3;
        }

       
        for (int i = 0; i < numEnemies; i++)
        {
            SpawnEnemy();
        }
      }
      triggered = true;
    }

    void SpawnEnemy()
    {
        
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        
        Vector3 spawnPos = spawner.position + new Vector3(randomPos.x, 0f, 0f);

        
        Instantiate(monster, spawnPos, Quaternion.identity);
    }
}
