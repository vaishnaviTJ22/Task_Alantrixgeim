using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Combo UI")]
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private TextMeshProUGUI comboMultiplierText;

    [Header("Level Info")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI levelNumberText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Confirmation Panel")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Preview UI")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private TextMeshProUGUI previewText;

    private System.Action onConfirmYes;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupEventListeners();
        HideAllPanels();
    }

    private void SetupEventListeners()
    {
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            ScoringManager.Instance.OnComboChanged.AddListener(UpdateCombo);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdate.AddListener(UpdateTimer);
            GameManager.Instance.OnLevelComplete.AddListener(ShowLevelComplete);
            GameManager.Instance.OnTimerExpired.AddListener(ShowGameOver);
            GameManager.Instance.OnPreviewStart.AddListener(ShowPreview);
            GameManager.Instance.OnPreviewEnd.AddListener(HidePreview);
        }

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);
    }

    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (previewPanel != null) previewPanel.SetActive(false);
        if (comboPanel != null) comboPanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}/ ";
        }

    }

    public void UpdateCombo(int combo, int multiplier)
    {
        if (combo > 1)
        {
            if (comboPanel != null) comboPanel.SetActive(true);

           

            if (comboMultiplierText != null)
            {
                comboMultiplierText.text = $"x{multiplier}";
            }

        }
        else
        {
            if (comboPanel != null) comboPanel.SetActive(false);
        }
    }

    public void UpdateTimer(float remainingTime)
    {
        if (timerText != null)
        {
            timerText.text = GameManager.Instance.GetFormattedTime(true);
        }
    }

    public void SetLevelInfo(string levelName, int levelNumber)
    {
        if (levelNameText != null)
        {
            levelNameText.text = levelName;
        }

        if (levelNumberText != null)
        {
            levelNumberText.text = $"Level {levelNumber}";
        }
    }

    private void ShowPreview()
    {
        if (previewPanel != null)
        {
            previewPanel.SetActive(true);
            if (previewText != null)
            {
                previewText.text = "Memorize the cards!";
            }
        }
    }

    private void HidePreview()
    {
        if (previewPanel != null)
        {
            previewPanel.SetActive(false);
        }
    }

    private void ShowLevelComplete(float completionTime)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

           
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {ScoringManager.Instance.Score}";
            }

            if (finalTimeText != null)
            {
                finalTimeText.text = $"Time: {GameManager.Instance.GetFormattedTime(false)}";
            }

            if (nextLevelButton != null)
            {
                bool hasNext = LevelManager.Instance != null && LevelManager.Instance.HasNextLevel();
                nextLevelButton.gameObject.SetActive(hasNext);
            }
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {ScoringManager.Instance.Score}";
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnNextLevelClicked()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance.LoadNextLevel();
    }

    private void OnRestartClicked()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance.RestartLevel();
    }

    private void OnMenuClicked()
    {
        ShowConfirmation("Return to Menu?", () =>
        {
            SceneManager.LoadScene("MenuScene");
        });
    }

    public void ShowConfirmation(string message, System.Action onConfirm)
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);

            if (confirmationText != null)
            {
                confirmationText.text = message;
            }

            onConfirmYes = onConfirm;
        }
    }

    private void OnConfirmYes()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        onConfirmYes?.Invoke();
        onConfirmYes = null;
    }

    private void OnConfirmNo()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        onConfirmYes = null;
    }

    public void OnBackButtonClicked()
    {
        ShowConfirmation("Return to Level Selection?", () =>
        {
            SceneManager.LoadScene("LevelSelectionScene");
        });
    }
}
