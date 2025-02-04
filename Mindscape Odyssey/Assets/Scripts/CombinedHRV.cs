using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Rendering.Universal;

public class CombinedHRV : MonoBehaviour
{
    private float checkInterval = 10.0f;  // Interval for fetching data
    public TextMeshProUGUI HRValueDisplay;
    public int HRValue;
    public int HRnumber;
    public GaleneMovement playerMovement;

    public Light2D globalLight;
    public Light2D cameraLight;
    public TextMeshProUGUI anxiousWarningText;

    public AudioSource calmAudioSource;
    public AudioSource anxiousAudioSource;
    public AudioSource breathingAudioSource;
    public AudioSource chaseAudioSource;
    private float fadeDuration = 2.0f;
    private float calmAudioTime = 0f;
    private float anxiousAudioTime = 0f;
    private float whisperSoundTime = 0f;
    private float chaseAudioTime = 0f;

    public AudioSource heartBeatSoundEffect; // Plays only in the anxious state
    public AudioSource whisperSoundEffect; // Plays periodically regardless of state

    private string url = "http://localhost:5000/analysis_results";
    private bool manualMode = false;

    public bool chaseSequence = false;

    public DeepBreathing deepBr;

    void Start()
    {
        chaseSequence=false;
        anxiousWarningText.gameObject.SetActive(false); // Hide the warning text initially
        StartCoroutine(FetchData());
        StartCoroutine(ChangeText());
        StartCoroutine(PlayWhisperSound());
    }

    void Update()
    {
        if (manualMode && Input.GetKeyDown(KeyCode.H))
        {
            HRValue = (HRValue == 0) ? 1 : 0;
        }

        AdjustBehavior();
    }

    IEnumerator FetchData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error fetching data: " + webRequest.error);
                manualMode = true;
            }
            else
            {
                manualMode = false;
                var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                HRValue = result.binary_output;
                HRnumber = result.current_hrv;
                StartCoroutine(FetchDataLoop());
            }
        }
    }

    IEnumerator FetchDataLoop()
    {
        while (!manualMode)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError("Error fetching data: " + webRequest.error);
                    manualMode = true;
                }
                else
                {
                    var result = JsonUtility.FromJson<ValueResponse>(webRequest.downloadHandler.text);
                    HRValue = result.binary_output;
                    HRnumber = result.current_hrv;
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    IEnumerator ChangeText()
    {
        while (true)
        {
            if (HRValue == 0)
            {
                HRValueDisplay.text = "Calm " + HRnumber.ToString();
                HRValueDisplay.color = Color.green;
                anxiousWarningText.gameObject.SetActive(false);

                ColorUtility.TryParseHtmlString("#18203C", out Color calmColor);
                globalLight.color = calmColor;
                ColorUtility.TryParseHtmlString("#8F9EB2", out Color cameraColor);
                cameraLight.color = cameraColor;

                StartCoroutine(SwitchToCalmState());
            }
            else
            {
                HRValueDisplay.text = "Anxious " + HRnumber.ToString();
                HRValueDisplay.color = Color.red;
                anxiousWarningText.gameObject.SetActive(true);

                ColorUtility.TryParseHtmlString("#B50F10", out Color anxiousColor);
                globalLight.color = anxiousColor;
                cameraLight.color = anxiousColor;

                StartCoroutine(SwitchToAnxiousState());
            }

            yield return new WaitForSeconds(1f);
        }
    }

   IEnumerator SwitchToCalmState()
{   
    if(deepBr.isBreathing){
        
        StartCoroutine(SwitchToBreathingState());
    }
    else if(chaseSequence==true){
        StartCoroutine(SwitchToChasingState());
        
    }
    else{
    // Save current playback position of the anxious audio
    if (anxiousAudioSource.isPlaying)
    {
        anxiousAudioTime = anxiousAudioSource.time;
        yield return StartCoroutine(FadeOut(anxiousAudioSource));
    }

    if (chaseAudioSource.isPlaying)
    {
        yield return StartCoroutine(FadeOut(chaseAudioSource));
    }

     if (breathingAudioSource.isPlaying)
    {
        
        yield return StartCoroutine(FadeOut(breathingAudioSource));
    }

    // Resume calm audio from the saved position
    if (!calmAudioSource.isPlaying)
    {
        calmAudioSource.time = calmAudioTime; // Resume from the saved position
        yield return StartCoroutine(FadeIn(calmAudioSource));
    }

    if (heartBeatSoundEffect.isPlaying)
    {
        heartBeatSoundEffect.Stop();
    }

    }
}

IEnumerator SwitchToAnxiousState()
{
    if(deepBr.isBreathing){
        StartCoroutine(SwitchToBreathingState());
    }
    else if(chaseSequence==true){
        StartCoroutine(SwitchToChasingState());
        
    }
    else{
    // Save current playback position of the calm audio
    if (calmAudioSource.isPlaying)
    {
        calmAudioTime = calmAudioSource.time;
        yield return StartCoroutine(FadeOut(calmAudioSource));
    }
     if (chaseAudioSource.isPlaying)
    {
        yield return StartCoroutine(FadeOut(chaseAudioSource));
    }

     if (breathingAudioSource.isPlaying)
    {
        
        yield return StartCoroutine(FadeOut(breathingAudioSource));
    }

    // Resume anxious audio from the saved position
    if (!anxiousAudioSource.isPlaying)
    {
        anxiousAudioSource.time = anxiousAudioTime; // Resume from the saved position
        yield return StartCoroutine(FadeIn(anxiousAudioSource));
    }

    if (!heartBeatSoundEffect.isPlaying)
    {
        heartBeatSoundEffect.Play();
    }
    }
}

IEnumerator SwitchToBreathingState()
{
    // Save current playback position of the calm audio
    if (calmAudioSource.isPlaying)
    {
        calmAudioTime = calmAudioSource.time;
        
    
    }

    // Resume anxious audio from the saved position
    if (anxiousAudioSource.isPlaying)
    {
        anxiousAudioTime = anxiousAudioSource.time; 

    }

    if (chaseAudioSource.isPlaying)
    {
        chaseAudioTime = chaseAudioSource.time; 

    }

    if (!breathingAudioSource.isPlaying)
    {
        
        yield return StartCoroutine(FadeIn(breathingAudioSource));
    }

    yield return null;

}

IEnumerator SwitchToChasingState()
{
    

    if (!chaseAudioSource.isPlaying)
    {
        chaseAudioSource.time = chaseAudioTime;
        yield return StartCoroutine(FadeIn(chaseAudioSource));
    }

    if (breathingAudioSource.isPlaying)
    {
        
        yield return StartCoroutine(FadeOut(breathingAudioSource));
    }

     if (calmAudioSource.isPlaying)
    {
        calmAudioTime = calmAudioSource.time;
        yield return StartCoroutine(FadeOut(calmAudioSource));
    }

     if (anxiousAudioSource.isPlaying)
    {
        anxiousAudioTime = anxiousAudioSource.time;
        yield return StartCoroutine(FadeOut(anxiousAudioSource));
    }

    yield return null;

}

    IEnumerator FadeOut(AudioSource audioSource)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
    }

    IEnumerator FadeIn(AudioSource audioSource)
    {
        audioSource.Play();
        float startVolume = 0f;
        audioSource.volume = startVolume;

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = 1f;
    }

    IEnumerator PlayWhisperSound()
{
    while (true)
    {
        // Set the playback position to resume from where it stopped
        whisperSoundEffect.time = whisperSoundTime;

        if (! whisperSoundEffect.isPlaying && !deepBr.isBreathing)
        {
             yield return StartCoroutine(FadeIn(whisperSoundEffect));
        }

        // Wait for 5 seconds while the sound plays
        yield return new WaitForSeconds(5f);

        // Save the current playback position before stopping the sound
        whisperSoundTime =   whisperSoundEffect.time;

       
        yield return StartCoroutine(FadeOut(whisperSoundEffect));
        

        // Wait for 15 seconds (remaining 20-second cycle time)
        yield return new WaitForSeconds(25f);
    }
}

    private void AdjustBehavior()
    {
        FlyingEnemy[] flyingEnemies = FindObjectsOfType<FlyingEnemy>();
        NewMonster[] newMonsters = FindObjectsOfType<NewMonster>();

        if (HRValue == 1)
        {
            foreach (FlyingEnemy enemy in flyingEnemies)
            {
                enemy.Damage = 25;
                enemy.chargeSpeed = 9f;
                enemy.pauseTime = 1f;
            }

            foreach (NewMonster monster in newMonsters)
            {
                monster.Damage = 15;
                if (Mathf.Abs(monster.speed) <= 3.05f)
                {
                    monster.speed *= 1.2f;
                }
            }

            playerMovement.moveSpeed = 5f;
        }
        else
        {
            foreach (FlyingEnemy enemy in flyingEnemies)
            {
                enemy.Damage = 20;
                enemy.chargeSpeed = 8f;
                enemy.pauseTime = 1.2f;
            }

            foreach (NewMonster monster in newMonsters)
            {
                monster.Damage = 10;
                if (Mathf.Abs(monster.speed) > 3.6f)
                {
                    monster.speed /= 1.2f;
                }
            }

            playerMovement.moveSpeed = 5.8f;
        }
    }

    [System.Serializable]
    public class ValueResponse
    {
        public int binary_output;
        public int current_hrv;
    }
}
