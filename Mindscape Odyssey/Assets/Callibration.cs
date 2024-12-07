using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;  // For scene loading

public class CalibrationScreen : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI wordText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI exampleText;
    public Button nextButton;
    public Button readyButton;
    public RectTransform fixationCross;

    private int trialCount = 0;
    private int totalTrialsPerBlock = 20;
    private float reactionStartTime;
    private bool isWaitingForInput = false;
    private bool trialActive = false;
    private string[] colors = { "Κόκκινο", "Πράσινο", "Μπλε" };
    private Color[] colorValues = { Color.red, Color.green, Color.blue };
    private KeyCode[] responseKeys = { KeyCode.X, KeyCode.C, KeyCode.V };
    private List<bool> trialCongruency = new List<bool>();
    private bool isPracticePhase = false;

    private List<float> reactionTimes = new List<float>();  
    private int correctAnswers = 0;


    void Start()
    {
        feedbackText.gameObject.SetActive(false);
        wordText.gameObject.SetActive(false);
        exampleText.gameObject.SetActive(false);
        fixationCross.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        readyButton.onClick.AddListener(ReadyForTesting);
        readyButton.gameObject.SetActive(true);
        instructionText.text = "This callibration consists of three phases :\n\n Relaxation \n Practise \n Test \n\n Firstly, you will have to relax for 5 minutes. Stay calm and try not to move. \n\n Press Ready to begin relaxation.";
    }

    private IEnumerator RelaxationPhase()
    {
        readyButton.gameObject.SetActive(false);
        float countdownTime = 15f;  // Change to 300f for full duration
        while (countdownTime > 0f)
        {
            int minutes = Mathf.FloorToInt(countdownTime / 60);
            int seconds = Mathf.FloorToInt(countdownTime % 60);
            instructionText.text = $"Relax for {minutes:00}:{seconds:00} minutes.";
            yield return new WaitForSeconds(1f);
            countdownTime -= 1f;
        }

        instructionText.text = "Relaxation Phase complete. \n\nClick 'Next' to receive instructions for the Practice Phase.";
        nextButton.gameObject.SetActive(true);
    }

    private void OnNextButtonClicked()
    {
        if (instructionText.text.Contains("Practice Phase"))
        {
            exampleText.gameObject.SetActive(true);
            instructionText.text = 
    "During this phase, you will complete 30 trials. In each trial, a word will appear on the screen. " +
    "However, the color of the word itself may not match the meaning of the word.\n\n" +
    "Your task is to quickly press the button corresponding to the color of the text, not the word's meaning. For example :\n\n\n\n\n\n" +
    "Focus carefully and respond as quickly and accurately as possible.\n" +
    "Click 'Ready' to start the Practice Phase.";

            readyButton.gameObject.SetActive(true);
        }
        else if (instructionText.text.Contains("Test Phase"))
        {
            instructionText.text = "In the Test Phase, you will complete 3 blocks of 40 trials each. \n\nAfter each block, you will take a 30-second break. \n\n\nClick 'Ready' to begin the Test Phase. ";
            readyButton.gameObject.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("Starting Menu");  // Replace with your actual main menu scene name
        }
    }

    private void ReadyForTesting(){

    if (instructionText.text.Contains("Practice Phase"))
        {
            StartPracticePhase();
        }
    else if(instructionText.text.Contains("Test Phase"))
    {
        StartTestBlock();
    }
    else{
        StartCoroutine(RelaxationPhase());readyButton.gameObject.SetActive(false);
    }




    }

    private void StartPracticePhase()
    {
        readyButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(false);
        GenerateTrialCongruency(6);
        isPracticePhase = true;
        reactionTimes.Clear();  
        correctAnswers = 0;
        exampleText.gameObject.SetActive(false);

        StartCoroutine(PracticePhase());
    }

    private IEnumerator PracticePhase()
    {
        for (trialCount = 0; trialCount < 6; trialCount++)
        {
            yield return StartCoroutine(RunTrial(trialCongruency[trialCount]));
            feedbackText.gameObject.SetActive(false);
            wordText.gameObject.SetActive(false);
        }

        instructionText.gameObject.SetActive(true);
        isPracticePhase = false;
        float averageTime = CalculateAverageTime();
        float accuracy = (correctAnswers / 6f) * 100;
        instructionText.text = $"Practice complete.\n\nAverage Reaction Time: {averageTime:F2} s\n\nAccuracy: {accuracy:F2}%\n\n\n Click 'Next' to receive instructions for the Test Phase.";
        nextButton.gameObject.SetActive(true);
    }

    private void StartTestBlock()
    {
        readyButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(false);
        reactionTimes.Clear();  
        correctAnswers = 0;
        StartCoroutine(TestPhase());
    }

    private IEnumerator TestPhase()
{
    for (int block = 1; block <= 3; block++)
    {
        
        GenerateTrialCongruency(8);
        for (trialCount = 0; trialCount < 8; trialCount++)
        {
            instructionText.gameObject.SetActive(false);
            yield return StartCoroutine(RunTrial(trialCongruency[trialCount]));
            feedbackText.gameObject.SetActive(false);
            wordText.gameObject.SetActive(false);
        }

        if (block < 3)
        {
            instructionText.gameObject.SetActive(true);
            for (int secondsLeft = 30; secondsLeft > 0; secondsLeft--)
            {
                instructionText.text = $"Take a break : {secondsLeft}-seconds left!";
                yield return new WaitForSeconds(1f);
            }
        }
    }

    instructionText.gameObject.SetActive(true);
    float averageTime = CalculateAverageTime();
    float accuracy = (correctAnswers / 24f) * 100;
    instructionText.text = $"Test complete.\n\nAverage Reaction Time: {averageTime:F2} s\n\nAccuracy: {accuracy:F2}%\n\n\n Click 'Finish' to return to the main menu.";
    buttonText.text = "Finish";
    nextButton.gameObject.SetActive(true);
}


    private void GenerateTrialCongruency(int trialCount)
    {
        trialCongruency.Clear();
        for (int i = 0; i < trialCount / 2; i++)
        {
            trialCongruency.Add(true);
            trialCongruency.Add(false);
        }
        for (int i = trialCongruency.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (trialCongruency[i], trialCongruency[j]) = (trialCongruency[j], trialCongruency[i]);
        }
    }

    private IEnumerator RunTrial(bool isCongruent)
    {
        feedbackText.gameObject.SetActive(false);
        wordText.gameObject.SetActive(false);
        fixationCross.gameObject.SetActive(true);

        fixationCross.anchoredPosition = new Vector2(Random.Range(-50f, 50f), Random.Range(-50f, 50f));
        wordText.rectTransform.anchoredPosition = fixationCross.anchoredPosition;

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        fixationCross.gameObject.SetActive(false);
        wordText.gameObject.SetActive(true);
        ShowStroopWord(isCongruent);

        reactionStartTime = Time.time;
        isWaitingForInput = true;
        trialActive = true;

        while (trialActive) yield return null;
    }

    private void ShowStroopWord(bool isCongruent)
    {
        int wordIndex = Random.Range(0, colors.Length);
        int colorIndex = isCongruent ? wordIndex : (wordIndex + Random.Range(1, colors.Length)) % colors.Length;
        wordText.text = colors[wordIndex];
        wordText.color = colorValues[colorIndex];
    }

    void Update()
    {
        if (isWaitingForInput)
        {
            for (int i = 0; i < responseKeys.Length; i++)
            {
                if (Input.GetKeyDown(responseKeys[i]))
                {
                    float reactionTime = Time.time - reactionStartTime;
                    isWaitingForInput = false;
                    bool correct = CheckCorrectAnswer(i);
                    DisplayFeedback(correct);
                    reactionTimes.Add(reactionTime);  
                    if (correct) correctAnswers++;  
                    StartCoroutine(FeedbackTime());
                }
            }
        }
    }

    private IEnumerator FeedbackTime()
    {
        float waitTime;
        if(isPracticePhase){
            waitTime=0.5f;
        }
        else{
            waitTime=0f;
        }
        yield return new WaitForSeconds(waitTime);
        trialActive = false;
    }

    private bool CheckCorrectAnswer(int keyIndex)
    {
        return wordText.color == colorValues[keyIndex];
    }

     private float CalculateAverageTime()
    {
        float sum = 0f;
        foreach (float t in reactionTimes) sum += t;
        return sum / reactionTimes.Count;
    }

    private void DisplayFeedback(bool correct)
    {
        if(isPracticePhase){
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = correct ? "Correct!" : "Wrong!";
        }
        else{
        feedbackText.gameObject.SetActive(false);
        }
    }
}
