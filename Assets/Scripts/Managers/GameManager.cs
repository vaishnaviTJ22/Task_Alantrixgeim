using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    [Header("Timer Events")]
    public UnityEvent<float> OnTimerUpdate;
    public UnityEvent OnTimerExpired;
    public UnityEvent<float> OnLevelComplete;
    public UnityEvent OnPreviewStart;
    public UnityEvent OnPreviewEnd;

    private List<Card> revealedCards = new List<Card>();
    private HashSet<Card> cardsBeingProcessed = new HashSet<Card>();
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
        var savedData = SaveSystem.LoadFullData();

        if (savedData != null)
        {
            SaveSystem.Load();
        }
        else
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(0);
            }
            else
            {
                BoardManager.Instance.GenerateBoard(4, 4);
                //  StartCoroutine(StartPreviewPhase(defaultPreviewDuration, true));
            }
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

        isInPreview = false;
        
        EnableAllCardsInteraction();

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

    public bool CanFlipCard(Card card)
    {
        if (isGameOver || isInPreview) return false;
        if (cardsBeingProcessed.Contains(card)) return false;
        if (card.state != CardState.FaceDown) return false;
        return true;
    }

    public void OnCardRevealed(Card card)
    {
        if (isGameOver || isInPreview) return;

        if (cardsBeingProcessed.Contains(card))
        {
            return;
        }

        revealedCards.Add(card);

        if (revealedCards.Count >= 2)
        {
            Card a = revealedCards[revealedCards.Count - 2];
            Card b = revealedCards[revealedCards.Count - 1];

            revealedCards.RemoveAt(revealedCards.Count - 1);
            revealedCards.RemoveAt(revealedCards.Count - 1);

            StartCoroutine(ProcessCardPair(a, b));
        }
    }

    IEnumerator ProcessCardPair(Card a, Card b)
    {
        if (a == null || b == null || a == b)
        {
            yield break;
        }

        cardsBeingProcessed.Add(a);
        cardsBeingProcessed.Add(b);

        a.SetProcessing(true);
        b.SetProcessing(true);

        yield return new WaitForSeconds(0.3f);

        if (a.cardID == b.cardID)
        {
            a.SetMatched();
            b.SetMatched();
            ScoringManager.Instance.AddScore(true);
            AudioManager.Instance?.PlayMatch();

            cardsBeingProcessed.Remove(a);
            cardsBeingProcessed.Remove(b);

            CheckLevelComplete();
        }
        else
        {
            ScoringManager.Instance.AddScore(false);
            AudioManager.Instance?.PlayMismatch();

            float hideDelay = LevelManager.Instance.CurrentLevel.mismatchHideDelay;

            yield return new WaitForSeconds(hideDelay);

            if (a != null && a.state == CardState.FaceUp)
            {
                a.FlipBack();
            }

            if (b != null && b.state == CardState.FaceUp)
            {
                b.FlipBack();
            }

            yield return new WaitForSeconds(0.3f);

            cardsBeingProcessed.Remove(a);
            cardsBeingProcessed.Remove(b);

            a.SetProcessing(false);
            b.SetProcessing(false);
        }
    }

    void CheckLevelComplete()
    {
        if (BoardManager.Instance.AllMatched())
        {
            StartCoroutine(HandleLevelCompleteSequence());
        }
    }

    IEnumerator HandleLevelCompleteSequence()
    {
        isGameOver = true;
        StopTimer();

        yield return new WaitForSeconds(2.0f);

        OnLevelComplete?.Invoke(elapsedTime);

        if (LevelManager.Instance != null)
        {
            LevelConfig level = LevelManager.Instance.CurrentLevel;
            ScoringManager.Instance.AddTimeBonus(elapsedTime, level.timeBonusMultiplier * 100);

            int levelNumber = LevelManager.Instance.CurrentLevelNumber;
            int score = ScoringManager.Instance.Score;
            // Fix: Pass levelNumber directly (1-based) as expected by SaveSystem and LevelSelectionManager
            SaveSystem.SaveLevelProgress(levelNumber, score, true);

            if (LevelManager.Instance.HasNextLevel())
            {
                StartCoroutine(AutoLoadNextLevel());
            }
        }
        else
        {
            ScoringManager.Instance.AddTimeBonus(elapsedTime);
        }

       
        SaveSystem.Save();

        Debug.Log($"Level Complete! Time: {GetFormattedTime(false)}");
    }

    IEnumerator AutoLoadNextLevel()
    {
        yield return new WaitForSeconds(3f);

        if (!isGameOver) yield break;

        LoadNextLevel();
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
        List<Card> cards = BoardManager.Instance.GetCards();
        foreach (var card in cards)
        {
            if (card != null)
            {
                card.DisableInteraction();
            }
        }
    }

    private void EnableAllCardsInteraction()
    {
        if (isGameOver || isInPreview) return;

        List<Card> cards = BoardManager.Instance.GetCards();
        foreach (var card in cards)
        {
            if (card != null && card.state == CardState.FaceDown)
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
           // StartCoroutine(StartPreviewPhase(defaultPreviewDuration, true));
        }
    }

    private void ResetGameState()
    {
        revealedCards.Clear();
        cardsBeingProcessed.Clear();
        isGameOver = false;
        isInPreview = false;
        ResetTimer();
        ScoringManager.Instance.ResetScore();
    }
}
