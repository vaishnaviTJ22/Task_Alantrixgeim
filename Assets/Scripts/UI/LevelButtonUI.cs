using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class LevelButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject lockIcon;

    private int levelIndex;

    public void Setup(int levelNum, int score, bool isUnlocked, UnityAction<int> onClick)
    {
        levelIndex = levelNum - 1;

        if (levelNumberText != null)
        {
            levelNumberText.text = levelNum.ToString();
        }

        if (scoreText != null)
        {
            if (isUnlocked && score > 0)
            {
                scoreText.gameObject.SetActive(true);
                scoreText.text = score.ToString();
            }
            else
            {
                scoreText.gameObject.SetActive(false);
            }
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        if (button != null)
        {
            button.interactable = isUnlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(levelIndex));
        }
    }
}
