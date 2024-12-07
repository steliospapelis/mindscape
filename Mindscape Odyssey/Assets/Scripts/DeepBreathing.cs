using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering.Universal;

public class DeepBreathing : MonoBehaviour
{
    public bool hasHealed = true;
    public RectTransform circleTransform; // The breathing circle
    public Light2D galeneLight; // 2D Light behind Galene

    public ParticleSystem endParticles1; // Particle system for end of breathing
    public ParticleSystem endParticles2;
    public Vector2 minScale = new Vector2(0.5f, 0.5f); // Smallest size of the circle
    public Vector2 maxScale = new Vector2(1.5f, 1.5f); // Largest size of the circle

    public float frequency = 6f; // Default breaths per minute
    private float inhaleDuration, holdDuration, exhaleDuration;
    public float totalTime = 45f; // Total duration of the exercise
    private float elapsedTime = 0f;

    private float lightStartIntensity = 0.4f;
    private float lightEndIntensity = 0.9f; // Adjust as needed
    private float lightStartRadius = 5f;
    private float lightEndRadius = 20f;

    public GameObject Circle;

    private float timer = 0f;
    private enum BreathingPhase { Inhale, Hold, Exhale, PostExhaleHold }
    private BreathingPhase currentPhase = BreathingPhase.PostExhaleHold;
    public bool isBreathing = false; // Track if the breathing exercise is active

    public Light2D[] targetLights; // Array to reference the six lights
    private Dictionary<Light2D, float> originalIntensities = new Dictionary<Light2D, float>();

    public GaleneMovement galene;
    public Animator anim;
    public TextMeshProUGUI Text;

    public HealthManager health;

    public GameObject particles;

    private int HealthAmount;

    private Dictionary<TilemapMover, float> tileMapMoverSpeeds = new Dictionary<TilemapMover, float>();
    private Dictionary<NewMonster, float> newMonsterSpeeds = new Dictionary<NewMonster, float>();

    private Dictionary<FlyingEnemy, float> newPatrolSpeeds = new Dictionary<FlyingEnemy, float>();

    private Dictionary<FlyingEnemy, float> newChargeSpeeds = new Dictionary<FlyingEnemy, float>();

    public Renderer sharedMaterialRenderer; // New Renderer for controlling material property

    private float lightIncreaseDelay = 10f;

    public TextMeshProUGUI[] textsToDeactivate;

    private GameObject boss;
    private Boss bossScript;

    float bossSpeed;

    public CombinedHRV hrv;

    

    void Start()
    {
        
        hasHealed = true;
        Text.text = "Hold";
        Circle.SetActive(true);
        particles.SetActive(false);
        galeneLight.intensity = lightStartIntensity;
        CalculateBreathingDurations();
        ResetVisualization();
        UpdateMaterialProgress(0f); // Ensure material starts at progress 0
    }

    void Update()
    {
        // Start the breathing exercise when "B" is pressed
        if (Input.GetKeyDown(KeyCode.B) && !isBreathing)
        {
            
            StartBreathing(frequency);
        }

        if (isBreathing)
        {
            elapsedTime += Time.deltaTime;

            // Update breathing circle animation
            UpdateBreathingCircle();

            // Rotate the circle
            RotateCircle();

            // Update light and screen fade
            float progress = elapsedTime / totalTime;
            UpdateLight(progress);
            UpdateMaterialProgress(progress); // Update material progress

            // End the exercise after totalTime
            if (elapsedTime >= totalTime)
            {
                EndExercise();
            }
        }
    }

    private void UpdateBreathingCircle()
    {
        timer += Time.deltaTime;

        switch (currentPhase)
        {
            case BreathingPhase.Inhale:
                circleTransform.localScale = Vector2.Lerp(minScale, maxScale, timer / inhaleDuration);
                if (timer >= inhaleDuration)
                {
                    timer = 0f;
                    currentPhase = BreathingPhase.Hold;
                    Text.text = "Hold";
                    
                }
                break;

            case BreathingPhase.Hold:
                circleTransform.localScale = maxScale; // Hold at max size
                if (timer >= holdDuration)
                {
                    timer = 0f;
                    currentPhase = BreathingPhase.Exhale;
                    Text.text = "Breath Out";
                }
                break;

            case BreathingPhase.Exhale:
                circleTransform.localScale = Vector2.Lerp(maxScale, minScale, timer / exhaleDuration);
                if (timer >= exhaleDuration)
                {
                    timer = 0f;
                    currentPhase = BreathingPhase.PostExhaleHold;
                    Text.text = "Hold";
                }
                break;

            case BreathingPhase.PostExhaleHold:
                circleTransform.localScale = minScale; // Hold at min size
                if (timer >= holdDuration)
                {
                    timer = 0f;
                    currentPhase = BreathingPhase.Inhale; // Restart cycle
                    Text.text = "Breath In";
                }
                break;
        }
    }

    private void RotateCircle()
    {
        float rotationAngle = (360f / totalTime - 0.1f) * Time.deltaTime; // Rotate to complete a full circle in totalTime
        circleTransform.Rotate(0, 0, rotationAngle);
    }

    private void UpdateLight(float progress)
{
    // Adjust progress to account for the delay
    if (elapsedTime >= lightIncreaseDelay)
    {
        float adjustedProgress = Mathf.Clamp01((elapsedTime - lightIncreaseDelay) / (totalTime - lightIncreaseDelay));
        galeneLight.intensity = Mathf.Lerp(lightStartIntensity, lightEndIntensity, adjustedProgress);
        galeneLight.pointLightOuterRadius = Mathf.Lerp(lightStartRadius, lightEndRadius, adjustedProgress);
    }
    else
    {
        // Keep light at starting intensity and radius until delay has passed
        galeneLight.intensity = lightStartIntensity;
        galeneLight.pointLightOuterRadius = lightStartRadius;
    }
}

    private void UpdateMaterialProgress(float progress)
    {
        if (sharedMaterialRenderer != null)
        {
            sharedMaterialRenderer.sharedMaterial.SetFloat("_Progress", Mathf.Clamp01(progress));
        }
    }

   private void EndExercise()
{
    galeneLight.intensity = 0f;
    Circle.SetActive(false);
    
    StartCoroutine(HealingSequence()); // Start the healing sequence coroutine
    Text.text = "";
    endParticles1.Play();
    endParticles2.Play();
    
    

    foreach (var light in targetLights)
    {
        if (light != null)
        {
            light.intensity = originalIntensities[light];
        }
    }

    foreach (var text in textsToDeactivate)
    {
        if (text != null)
        {
            text.gameObject.SetActive(true); 
        }
    }


    foreach (var mover in tileMapMoverSpeeds.Keys)
    {
        if (mover != null)
        {
            mover.speed = tileMapMoverSpeeds[mover];
        }
    }
    tileMapMoverSpeeds.Clear();

    

    isBreathing = false;
    
    anim.SetBool("Breathing", false);
    UpdateMaterialProgress(0f); // Reset material progress
}

private IEnumerator HealingSequence()
{
    float healingDelay = 1.8f; // Total duration for healing
    yield return new WaitForSeconds(healingDelay); // Wait for the next healing tick
    galene.canMove = true;
    health.Healing(HealthAmount);
    hasHealed=true;
    if(boss!=null){
    bossScript.speed=bossSpeed;
    }

    foreach (var monster in newMonsterSpeeds.Keys)
    {
        if (monster != null)
        {
            monster.speed = newMonsterSpeeds[monster];
        }
    }
    newMonsterSpeeds.Clear();

    foreach (var flying in newPatrolSpeeds.Keys)
    {
        if (flying != null)
        {
            flying.patrolSpeed = newPatrolSpeeds[flying];
            flying.chargeSpeed = newChargeSpeeds[flying];
        }
    }
    newPatrolSpeeds.Clear();
    newChargeSpeeds.Clear();
    
     
}



    private void CalculateBreathingDurations()
    {
        float totalBreaths = frequency * (totalTime / 60f);
        float timePerBreath = totalTime / totalBreaths;

        inhaleDuration = timePerBreath * 0.4f; // 40% inhale
        holdDuration = timePerBreath * 0.1f;   // 10% hold
        exhaleDuration = timePerBreath * 0.5f; // 50% exhale

        HealthAmount = 100;
    }

    private void ResetVisualization()
    {
        elapsedTime = 0f;
        timer = 0f;
        currentPhase = BreathingPhase.PostExhaleHold;

        galeneLight.intensity = lightStartIntensity;
        galeneLight.pointLightOuterRadius = lightStartRadius;
    }

    public void StartBreathing(float newFrequency)
    {
        
        foreach (var text in textsToDeactivate)
    {
        if (text != null)
        {
            text.gameObject.SetActive(false); 
        }
    }

        foreach (var light in targetLights)
        {
            if (light != null)
            {
                originalIntensities[light] = light.intensity;
                light.intensity = 0.0f;
            }
        }

        foreach (var mover in FindObjectsOfType<TilemapMover>())
        {
            tileMapMoverSpeeds[mover] = mover.speed;
            mover.speed = 0f;
        }



        boss = GameObject.FindWithTag("Boss");
        if(boss!=null){
        bossScript = boss.GetComponent<Boss>();
        bossSpeed = bossScript.speed;
        bossScript.speed=0;
        }


        foreach (var monster in FindObjectsOfType<NewMonster>())
        {
            newMonsterSpeeds[monster] = monster.speed;
            monster.speed = 0f;
        }

        foreach (var flying in FindObjectsOfType<FlyingEnemy>())
        {
            newPatrolSpeeds[flying] = flying.patrolSpeed;
            newChargeSpeeds[flying] = flying.chargeSpeed;
            flying.patrolSpeed = 0f;
            flying.chargeSpeed = 0f;
        }

        StartCoroutine(hrv.NotifyServer(true, false, false)); // Example usage

        hasHealed = false;
        Circle.SetActive(true);
        frequency = newFrequency;
        CalculateBreathingDurations();
        ResetVisualization();
        isBreathing = true;
        galene.canMove = false;
        anim.SetBool("Breathing", true);
    }
}
