using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Configuration")]
    public List<LevelConfig> levels;

    private int currentLevelIndex = 0;

    public LevelConfig CurrentLevel => levels[currentLevelIndex];
    public int CurrentLevelNumber => currentLevelIndex + 1;
    public int TotalLevels => levels.Count;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        currentLevelIndex = levelIndex;
        LevelConfig level = levels[currentLevelIndex];

        if (!level.IsValidConfiguration())
        {
            return;
        }

        if (BoardManager.Instance != null)
        {
            var existingCards = BoardManager.Instance.GetCards();
            foreach (var card in existingCards)
            {
                if (card != null)
                {
                    card.ResetCard();
                }
            }
        }

        BoardManager.Instance.GenerateBoard(level.rows, level.columns, level);
        ScoringManager.Instance.ResetScore();
        ScoringManager.Instance.SetLevelScoring(level.matchBonus, level.mismatchPenalty);
        if (level.usePreview)
        {
            GameManager.Instance.StartLevelWithPreview(
                level.previewDuration,
                level.useTimeLimit,
                level.timeLimitSeconds
            );
        }
        else
        {
            GameManager.Instance.StartTimer(level.timeLimitSeconds, level.useTimeLimit);
        }

        UIManager.Instance.SetLevelInfo(level.levelNumber);

        Debug.Log($"Loaded Level {level.levelNumber}: {level.levelName}");
        Debug.Log($"Preview: {(level.usePreview ? $"{level.previewDuration}s" : "Disabled")}");
        Debug.Log($"Time Limit: {(level.useTimeLimit ? level.GetFormattedTimeLimit() : "Unlimited")}");
    }

    public void LoadNextLevel()
    {
        if (currentLevelIndex < levels.Count - 1)
        {
            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("All levels completed!");
        }
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public bool HasNextLevel()
    {
        return currentLevelIndex < levels.Count - 1;
    }
}
