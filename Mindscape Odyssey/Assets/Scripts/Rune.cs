using UnityEngine;
using System.Collections;
using TMPro;


public class Rune : MonoBehaviour
{
    public SpriteRenderer glowSprite;
    public UnityEngine.Rendering.Universal.Light2D runeLight;
    public Transform breathingIcon;
    public Transform iconStartPosition;
    public Transform iconAboveRunePosition;
    public CameraMovement cameraScript;
    public Transform boss;
    public GameObject Portal;
    public ParticleSystem bossParticles;

    private static int activatedRunes = 0; // Count of activated runes
    private bool isActivated = false;
    private bool playerInside = false;
    private Coroutine breathingCoroutine;
    private Vector3 originalBossScale;

    public static bool oneActive = false;

    public Transform galene;
    public GaleneMovement galeneMovement;

    public AudioSource bossDeath;
    public AudioSource runeActivation;

    public CombinedHRV hrv;

    void Start()
    {
        originalBossScale = boss.localScale;
        runeLight.gameObject.SetActive(false); // Ensure light is initially off
    }

void Update()
{
    if (playerInside && !isActivated && Input.GetKeyDown(KeyCode.B))
    {
        StartBreathing();
    }

    if (playerInside && !isActivated)
    {
        float glowValue = Mathf.PingPong(Time.time * 2, 1); // Ping-pong effect
        glowSprite.color = Color.Lerp(Color.red, Color.yellow, glowValue);

       

        // Move it to the target position relative to the rune
        breathingIcon.position = Vector3.Lerp(breathingIcon.position, iconAboveRunePosition.position, 0.2f);
    }
    else if (!oneActive)
    {
        
        
        breathingIcon.position = Vector3.Lerp(breathingIcon.position, iconStartPosition.position, 0.2f);
    }
}






    void StartBreathing()
    {
        if (breathingCoroutine == null)
        {
            breathingCoroutine = StartCoroutine(BreathingRoutine());
        }
    }

    IEnumerator BreathingRoutine()
    {
        yield return new WaitForSeconds(45f); // Simulate breathing session

        ActivateRune();
        breathingCoroutine = null;
    }

    void ActivateRune()
    {
        runeActivation.Play();
        isActivated = true;
        GetComponent<Collider2D>().enabled = false;
        glowSprite.color = Color.green; // Change to green permanently
        runeLight.gameObject.SetActive(true); // Activate light

        activatedRunes++;
        Debug.Log("Activated Runes: " + activatedRunes);

        
            StartCoroutine(BossShrinkRoutine());
        
       
    }

 IEnumerator BossShrinkRoutine()
{
    // Move the boss upwards each time before shrinking
    Vector3 targetPosition = boss.position + new Vector3(0, 1.5f, 0);
    float moveTime = 2f; // Time taken for the upward movement
    float elapsedMove = 0f;

    Vector3 initialPosition = boss.position;
    while (elapsedMove < moveTime)
    {
        elapsedMove += Time.deltaTime;
        boss.position = Vector3.Lerp(initialPosition, targetPosition, elapsedMove / moveTime);
        yield return null;
    }

    // Create a temporary target above the boss for camera focus
    Vector3 bossTargetPosition = boss.position + new Vector3(0, 5f, 0);
    Transform tempTarget = new GameObject("BossTarget").transform;
    tempTarget.position = bossTargetPosition;
    galeneMovement.canMove=false;
    cameraScript.target = tempTarget;

    // Compute the target scale, preserving the sign of the X scale
    Vector3 targetScale = originalBossScale * Mathf.Pow(0.5f, activatedRunes);
    targetScale.x = Mathf.Abs(targetScale.x) * Mathf.Sign(boss.localScale.x); // Keep X scale negative if needed

    Debug.Log($"Boss shrinking to: {targetScale}");
    float shrinkTime = 4f; // Time taken for shrinking
    float elapsedShrink = 0f;

    
    Vector3 initialScale = boss.localScale;
    while (elapsedShrink < shrinkTime)
    {
        elapsedShrink += Time.deltaTime;
        boss.localScale = Vector3.Lerp(initialScale, targetScale, elapsedShrink / shrinkTime);
        yield return null;
    }

    boss.localScale = targetScale;

    if (activatedRunes == 3) // Special behavior for the third rune
    {
        bossDeath.Play();
        yield return new WaitForSeconds(1f); // Wait before disappearing
        bossParticles.Play(); // Trigger particles
        
        boss.gameObject.SetActive(false); // Make boss disappear
        Portal.SetActive(true);
        hrv.chaseSequence=false;
    }

     yield return new WaitForSeconds(1f);

    cameraScript.target = galene; // Return to normal camera operation
    galeneMovement.canMove=true;
    Destroy(tempTarget.gameObject); // Cleanup
    
}



    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            oneActive = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            oneActive = false;
        }
    }
}
