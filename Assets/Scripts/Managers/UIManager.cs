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
    [SerializeField] private GameObject gameWonPenel;
    [SerializeField] private GameObject gameFailedPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

   
    private Coroutine comboCoroutine;

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
        }

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

       
    }

    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (comboPanel != null) comboPanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
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

            if (comboCoroutine != null)
            {
                StopCoroutine(comboCoroutine);
            }
            comboCoroutine = StartCoroutine(HideComboPanelAfterDelay());

        }
        else
        {
            if (comboPanel != null) comboPanel.SetActive(false);
        }
    }

    private System.Collections.IEnumerator HideComboPanelAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (comboPanel != null) comboPanel.SetActive(false);
    }

    public void UpdateTimer(float remainingTime)
    {
        if (timerText != null)
        {
            timerText.text = GameManager.Instance.GetFormattedTime(true);
        }
    }

    public void SetLevelInfo( int levelNumber)
    {
        if (levelNumberText != null)
        {
            levelNumberText.text = $"Level {levelNumber}";
        }
    }

  

    private void ShowLevelComplete(float completionTime)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameWonPenel.SetActive(true);
            AudioManager.Instance?.PlayGameOverWon();
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
            AudioManager.Instance?.PlayGameOverLoss();
            gameOverPanel.SetActive(true);
            gameFailedPanel.SetActive(true);
           
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
        SceneManager.LoadScene("MainMenu");
    }

  
  
}
