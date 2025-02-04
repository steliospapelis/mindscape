using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLoggingTutorial : MonoBehaviour
{
    private List<string> logData = new List<string>();
    private string logFilePath;
    
    private HealthManager Health;
    
    public int checkpoint =0;

    private Camera mainCamera;
    private GameObject[] forestMonsters;

    private void Start()
    {
        string logsDirectory;
        
        #if UNITY_EDITOR
        logsDirectory = Path.Combine(Application.dataPath, "logs");
        #else
        logsDirectory = Path.Combine(Application.persistentDataPath, "logs");
        #endif
        
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }
        
        logFilePath = Path.Combine(logsDirectory, "game_log_tutorial" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

        
        Health = FindObjectOfType<HealthManager>();

        mainCamera = Camera.main;
        

        logData.Add("Timestamp;W;A;D;Space;HP;Death;EnemiesVisible;Checkpoint;X;Y");
    }

    private void Update()
    {
        LogFrame();
        if (Input.GetKeyDown(KeyCode.L)){
            SaveLogToFile();
        }
    }

    private void LogFrame()
    {
        float timestamp = Time.time;
        int w = Input.GetKey(KeyCode.W) ? 1 : 0;
        int a = Input.GetKey(KeyCode.A) ? 1 : 0;
        int d = Input.GetKey(KeyCode.D) ? 1 : 0;
        int space = Input.GetKey(KeyCode.Space) ? 1 : 0;
        
        
        float hp = Health != null ? Health.health : 0f;
        int death = 0; 
        int enemiesVisible = GetVisibleEnemyCount();
        float x = transform.position.x;
        float y = transform.position.y;

        string logEntry = $"{timestamp:F2};{w};{a};{d};{space};{hp};{death};{enemiesVisible};{checkpoint};{x:F2};{y:F2}";
        logData.Add(logEntry);
    }

    public void LogDeath()
    {
        if (logData.Count > 1)
        {
            string[] lastEntry = logData[logData.Count - 1].Split(';');
            lastEntry[6] = "1"; // Set "Death" column to 1
            logData[logData.Count - 1] = string.Join(";", lastEntry);
        }
    }

    public int GetVisibleEnemyCount()
    {

        
        forestMonsters = GameObject.FindGameObjectsWithTag("ForestMonster");
        int count = 0;
       
        foreach (GameObject enemy in forestMonsters)
        {
            if (enemy != null && IsVisible(enemy.transform.position))
            {
                count++;
            }
        }
        return count;
    }

    private bool IsVisible(Vector3 position)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }


    public void SaveLogToFile()
    {
        File.WriteAllLines(logFilePath, logData);
        Debug.Log($"Log saved to: {logFilePath}");
    }
}

