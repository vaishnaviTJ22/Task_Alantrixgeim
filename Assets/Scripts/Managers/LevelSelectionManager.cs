using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonsContainer;
    
    [Header("Scene Config")]
    [SerializeField] private string gameplaySceneName = "GameScene";

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager instance not found!");
            return;
        }

        // Load save data to check unlocks
        SaveData saveData = SaveSystem.LoadFullData();
        int highestUnlocked = 1;

        if (saveData != null)
        {
            highestUnlocked = Mathf.Max(1, saveData.highestUnlockedLevel);
        }

        // Clear existing buttons
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate buttons for each level
        for (int i = 0; i < LevelManager.Instance.TotalLevels; i++)
        {
            int levelNum = i + 1;
            GameObject btnObj = Instantiate(levelButtonPrefab, buttonsContainer);
            LevelButtonUI btnUI = btnObj.GetComponent<LevelButtonUI>();

            if (btnUI != null)
            {
                bool isUnlocked = levelNum <= highestUnlocked;
                int score = 0;

                // Find score for this level
                if (saveData != null && saveData.levelScores != null)
                {
                    var progress = saveData.levelScores.Find(x => x.levelNumber == levelNum);
                    if (progress != null)
                    {
                        score = progress.bestScore;
                    }
                }

                btnUI.Setup(levelNum, score, isUnlocked, OnLevelClicked);
            }
            else
            {
                Debug.LogError("LevelButtonPrefab does not have LevelButtonUI component!");
            }
        }
    }

    private void OnLevelClicked(int levelIndex)
    {
        // Save the selected level index to save data so Main Game loads it
        // We use SaveSystem to "inject" the current level choice.
        
        SaveData data = SaveSystem.LoadFullData();
        if (data == null)
        {
            data = new SaveData
            {
                highestUnlockedLevel = 1,
                levelScores = new System.Collections.Generic.List<LevelProgress>()
            };
        }

        data.currentLevel = levelIndex + 1; // 1-based level number usually, but SaveSystem.Load uses data.currentLevel
        // Note: SaveSystem.Load() does: LevelManager.Instance.LoadLevel(data.currentLevel - 1);
        // So if we save '1', it loads index '0'. Correct.

        System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", JsonUtility.ToJson(data, true));

        // Load Scene
        SceneManager.LoadScene(gameplaySceneName);
    }
}
