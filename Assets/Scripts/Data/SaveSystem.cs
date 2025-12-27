using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class SaveSystem
{
    private static string path => Application.persistentDataPath + "/save.json";

    public static void Save()
    {
        SaveData data = LoadFullData();
        
        if (data == null)
        {
            data = new SaveData
            {
                highestUnlockedLevel = 1,
                levelScores = new List<LevelProgress>()
            };
        }

        data.currentLevel = LevelManager.Instance.CurrentLevelNumber;
        data.score = ScoringManager.Instance.Score;
        
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    public static void Load()
    {
        if (!File.Exists(path)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

        if (data.currentLevel > 0)
        {
            LevelManager.Instance.LoadLevel(data.currentLevel - 1);
        }

        ScoringManager.Instance.SetScore(data.score);
    }

    public static void SaveLevelProgress(int levelNumber, int score, bool completed)
    {
        SaveData data = LoadFullData() ?? new SaveData
        {
            levelScores = new List<LevelProgress>()
        };

        LevelProgress progress = data.levelScores.FirstOrDefault(lp => lp.levelNumber == levelNumber);

        if (progress == null)
        {
            progress = new LevelProgress { levelNumber = levelNumber };
            data.levelScores.Add(progress);
        }

        progress.bestScore = Mathf.Max(progress.bestScore, score);
        progress.completed = completed;

        if (completed)
        {
            data.highestUnlockedLevel = Mathf.Max(data.highestUnlockedLevel, levelNumber + 1);
        }

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    public static SaveData LoadFullData()
    {
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }
}
