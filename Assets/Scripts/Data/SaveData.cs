using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int currentLevel;
    public int score;
    public int highestUnlockedLevel;
    public List<int> matchedCardIDs;
    public List<LevelProgress> levelScores;
}

[Serializable]
public class LevelProgress
{
    public int levelNumber;
    public int bestScore;
    public bool completed;
}
