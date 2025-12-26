using UnityEngine;
using UnityEngine.Events;

public class ScoringManager : MonoBehaviour
{
    public static ScoringManager Instance;

    [Header("Score Display")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int, int> OnComboChanged;
    public UnityEvent<int, int> OnTargetScoreSet;

    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int ComboMultiplier { get; private set; } = 1;
    public int TargetScore { get; private set; }

    [Header("Default Scoring")]
    public int defaultMatchScore = 100;
    public int defaultMismatchPenalty = 10;

    private int currentMatchScore;
    private int currentMismatchPenalty;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentMatchScore = defaultMatchScore;
        currentMismatchPenalty = defaultMismatchPenalty;
    }

    public void AddScore(bool matched)
    {
        if (matched)
        {
            Combo++;
            ComboMultiplier = Combo;

            int scoreToAdd = currentMatchScore * ComboMultiplier;
            Score += scoreToAdd;

            OnComboChanged?.Invoke(Combo, ComboMultiplier);
            Debug.Log($"Match! +{scoreToAdd} (Combo x{ComboMultiplier})");
        }
        else
        {
            if (Combo > 0)
            {
                Debug.Log($"Combo broken! Was at x{Combo}");
            }
            Combo = 0;
            ComboMultiplier = 1;
            Score = Mathf.Max(0, Score - currentMismatchPenalty);

            OnComboChanged?.Invoke(Combo, ComboMultiplier);
            Debug.Log($"Mismatch! -{currentMismatchPenalty}");
        }

        OnScoreChanged?.Invoke(Score);
    }

    public void SetLevelTargetScore(int target)
    {
        TargetScore = target;
        OnTargetScoreSet?.Invoke(Score, TargetScore);
        Debug.Log($"Target Score set to: {target}");
    }

    public void SetLevelScoring(int matchScore, int mismatchPenalty)
    {
        currentMatchScore = matchScore;
        currentMismatchPenalty = mismatchPenalty;
    }

    public void SetScore(int score)
    {
        Score = score;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        Combo = 0;
        ComboMultiplier = 1;
        OnScoreChanged?.Invoke(Score);
        OnComboChanged?.Invoke(Combo, ComboMultiplier);
        Debug.Log("Score Reset");
    }

    public bool HasReachedTarget()
    {
        return Score >= TargetScore;
    }

    public float GetScoreProgress()
    {
        if (TargetScore <= 0) return 0f;
        return Mathf.Clamp01((float)Score / TargetScore);
    }

    public void AddTimeBonus(float timeElapsed, int maxBonus = 500)
    {
        int bonus = Mathf.RoundToInt(maxBonus / Mathf.Max(1f, timeElapsed / 60f));
        Score += bonus;
        OnScoreChanged?.Invoke(Score);
        Debug.Log($"Time Bonus! +{bonus}");
    }

    public void AddPerfectMatchBonus(int bonus = 1000)
    {
        Score += bonus;
        OnScoreChanged?.Invoke(Score);
        Debug.Log($"Perfect Match Bonus! +{bonus}");
    }
}
