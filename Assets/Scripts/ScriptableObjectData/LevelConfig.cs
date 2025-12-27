using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Card Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;

    [Header("Grid Settings")]
    public int rows;
    public int columns;

    [Header("Preview Settings")]
    public bool usePreview = true;
    public float previewDuration = 2f;

    [Header("Time Settings")]
    public bool useTimeLimit = true;
    public float timeLimitSeconds = 180f;

    [Tooltip("Time bonus multiplier for completing quickly")]
    public int timeBonusMultiplier = 10;

    [Header("Difficulty Settings")]
    public float cardFlipSpeed = 0.3f;
    public float mismatchHideDelay = 0.6f;

    [Header("Scoring")]
    public int matchBonus = 100;
    public int mismatchPenalty = 10;
    public int targetScore = 1000;

    [Header("Card Theme")]
    public Sprite cardBackSprite;
    public Sprite[] cardFrontSprites;

    public int TotalCards => rows * columns;

    public bool IsValidConfiguration()
    {
        if (TotalCards % 2 != 0)
        {
            Debug.LogError($"Level {levelNumber}: Total cards must be even!");
            return false;
        }

        if (cardFrontSprites == null || cardFrontSprites.Length == 0)
        {
            Debug.LogError($"Level {levelNumber}: Need at least 1 card sprite!");
            return false;
        }

        return true;
    }

    public string GetFormattedTimeLimit()
    {
        int minutes = Mathf.FloorToInt(timeLimitSeconds / 60);
        int seconds = Mathf.FloorToInt(timeLimitSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
