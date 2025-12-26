using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Mismatch Settings")]
    public float mismatchFlipBackDelay = 1.0f;

    [Header("Preview Settings")]
    public float defaultPreviewDuration = 2f;

    [Header("Timer Events")]
    public UnityEvent<float> OnTimerUpdate;
    public UnityEvent OnTimerExpired;
    public UnityEvent<float> OnLevelComplete;
    public UnityEvent OnPreviewStart;
    public UnityEvent OnPreviewEnd;

    private List<Card> revealedCards = new List<Card>();
    private bool isProcessingMatch = false;
    private bool isGameOver = false;
    private bool isInPreview = false;

    private float elapsedTime = 0f;
    private float timeLimit = 0f;
    private bool useTimeLimit = false;
    private bool isTimerRunning = false;

    public float ElapsedTime => elapsedTime;
    public float RemainingTime => Mathf.Max(0, timeLimit - elapsedTime);
    public bool IsTimerRunning => isTimerRunning;
    public bool IsInPreview => isInPreview;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(0);
        }
        else
        {
            BoardManager.Instance.GenerateBoard(4, 4);
            StartCoroutine(StartPreviewPhase(defaultPreviewDuration, true));
        }

      
    }

    private void Update()
    {
        if (isTimerRunning && !isGameOver && !isInPreview)
        {
            elapsedTime += Time.deltaTime;

            OnTimerUpdate?.Invoke(RemainingTime);

            if (useTimeLimit && RemainingTime <= 0)
            {
                TimerExpired();
            }
        }
    }

    public void StartLevelWithPreview(float previewDuration, bool enableTimeLimit, float timeLimitSeconds)
    {
        timeLimit = timeLimitSeconds;
        useTimeLimit = enableTimeLimit;
        StartCoroutine(StartPreviewPhase(previewDuration, false));
    }

    IEnumerator StartPreviewPhase(float duration, bool startTimerAfter)
    {
        isInPreview = true;
        isTimerRunning = false;

        OnPreviewStart?.Invoke();
        Debug.Log("Preview phase started - Memorize the cards!");

        DisableAllCardsInteraction();

        yield return new WaitForSeconds(0.2f);

        List<Card> allCards = BoardManager.Instance.GetCards();
        foreach (var card in allCards)
        {
            card.FlipInstant(true);
        }

        yield return new WaitForSeconds(duration);

        foreach (var card in allCards)
        {
            card.FlipBack();
        }

        yield return new WaitForSeconds(0.5f);

        EnableAllCardsInteraction();

        isInPreview = false;
        OnPreviewEnd?.Invoke();

        if (startTimerAfter)
        {
            StartTimer(timeLimit, useTimeLimit);
        }
        else
        {
            isTimerRunning = true;
        }

        Debug.Log("Preview phase ended - Game started!");
    }

    public void StartTimer(float timeLimitSeconds = 0, bool enableTimeLimit = false)
    {
        elapsedTime = 0f;
        timeLimit = timeLimitSeconds;
        useTimeLimit = enableTimeLimit;
        isTimerRunning = true;

        Debug.Log($"Timer started: {(useTimeLimit ? $"{timeLimitSeconds}s limit" : "Unlimited")}");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = false;
        OnTimerUpdate?.Invoke(RemainingTime);
    }

    public string GetFormattedTime(bool showRemaining = true)
    {
        float time = showRemaining ? RemainingTime : elapsedTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public void OnCardRevealed(Card card)
    {
        if (isProcessingMatch || isGameOver || isInPreview) return;

        revealedCards.Add(card);

        if (revealedCards.Count >= 2)
        {
            isProcessingMatch = true;
            StartCoroutine(ProcessCardMatch());
        }
    }

    IEnumerator ProcessCardMatch()
    {
        yield return new WaitForSeconds(0.3f);

        Card a = revealedCards[^2];
        Card b = revealedCards[^1];

        if (a == b)
        {
            isProcessingMatch = false;
            yield break;
        }

        if (a.cardID == b.cardID)
        {
            a.SetMatched();
            b.SetMatched();
            ScoringManager.Instance.AddScore(true);

            revealedCards.Clear();
            CheckLevelComplete();
        }
        else
        {
            ScoringManager.Instance.AddScore(false);

            float hideDelay = LevelManager.Instance != null
                ? LevelManager.Instance.CurrentLevel.mismatchHideDelay
                : mismatchFlipBackDelay;

            yield return new WaitForSeconds(hideDelay);

            a.FlipBack();
            b.FlipBack();
            revealedCards.Clear();
        }

        isProcessingMatch = false;
    }

    void CheckLevelComplete()
    {
        if (BoardManager.Instance.AllMatched())
        {
            isGameOver = true;
            StopTimer();

            OnLevelComplete?.Invoke(elapsedTime);

            if (LevelManager.Instance != null)
            {
                LevelConfig level = LevelManager.Instance.CurrentLevel;
                ScoringManager.Instance.AddTimeBonus(elapsedTime, level.timeBonusMultiplier * 100);
            }
            else
            {
                ScoringManager.Instance.AddTimeBonus(elapsedTime);
            }


            if (LevelManager.Instance != null)
            {
                int levelNumber = LevelManager.Instance.CurrentLevelNumber;
                int score = ScoringManager.Instance.Score;
            }

           

            Debug.Log($"Level Complete! Time: {GetFormattedTime(false)}");
        }
    }

    private void TimerExpired()
    {
        isGameOver = true;
        StopTimer();

        OnTimerExpired?.Invoke();

        Debug.Log("Time's Up! Game Over!");

        DisableAllCardsInteraction();

    }

    private void DisableAllCardsInteraction()
    {
        foreach (var card in BoardManager.Instance.GetCards())
        {
            if (card != null)
            {
                card.DisableInteraction();
            }
        }
    }

    private void EnableAllCardsInteraction()
    {
        foreach (var card in BoardManager.Instance.GetCards())
        {
            if (card != null)
            {
                card.EnableInteraction();
            }
        }
    }

    public void LoadNextLevel()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.HasNextLevel())
        {
            ResetGameState();
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void RestartLevel()
    {
        ResetGameState();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
        else
        {
            BoardManager.Instance.GenerateBoard(4, 4);
            StartCoroutine(StartPreviewPhase(defaultPreviewDuration, true));
        }
    }

    private void ResetGameState()
    {
        revealedCards.Clear();
        isProcessingMatch = false;
        isGameOver = false;
        isInPreview = false;
        ResetTimer();
        ScoringManager.Instance.ResetScore();
    }
}
