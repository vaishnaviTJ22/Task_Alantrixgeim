using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [SerializeField] private RectTransform boardArea;
    [SerializeField] private Card cardPrefab;

    private List<Card> cards = new List<Card>();
    private LevelConfig currentLevelConfig;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateBoard(int rows, int cols, LevelConfig levelConfig = null)
    {
        ClearBoard();
        currentLevelConfig = levelConfig;

        int totalCards = rows * cols;
        List<int> ids = GenerateCardIDs(totalCards);

        float cardSize = Mathf.Min(
            boardArea.rect.width / cols,
            boardArea.rect.height / rows
        ) * 0.85f;

        float spacing = cardSize * 0.15f;

        float totalWidth = (cols * cardSize) + ((cols - 1) * spacing);
        float totalHeight = (rows * cardSize) + ((rows - 1) * spacing);

        float startX = -totalWidth / 2f + cardSize / 2f;
        float startY = totalHeight / 2f - cardSize / 2f;

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Card card = Instantiate(cardPrefab, boardArea);
                RectTransform rt = card.GetComponent<RectTransform>();

                rt.sizeDelta = new Vector2(cardSize, cardSize);

                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                float posX = startX + (c * (cardSize + spacing));
                float posY = startY - (r * (cardSize + spacing));

                rt.anchoredPosition = new Vector2(posX, posY);

                card.cardID = ids[index];

                if (levelConfig != null)
                {
                    int spriteIndex = ids[index] % levelConfig.cardFrontSprites.Length;
                    card.SetCardSprites(
                        levelConfig.cardBackSprite,
                        levelConfig.cardFrontSprites[spriteIndex]
                    );
                    card.SetFlipSpeed(levelConfig.cardFlipSpeed);
                }

                cards.Add(card);
                index++;
            }
        }
    }

    List<int> GenerateCardIDs(int total)
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < total / 2; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        for (int i = 0; i < ids.Count; i++)
        {
            int rnd = Random.Range(0, ids.Count);
            (ids[i], ids[rnd]) = (ids[rnd], ids[i]);
        }
        return ids;
    }

    void ClearBoard()
    {
        foreach (var card in cards)
        {
            if (card != null)
            {
                card.ResetCard();
            }
        }

        foreach (Transform child in boardArea)
            Destroy(child.gameObject);

        cards.Clear();
    }

    public bool AllMatched()
    {
        foreach (var card in cards)
            if (card.state != CardState.Matched)
                return false;
        return true;
    }

    public List<Card> GetCards() => cards;

    public void ResetAllCards()
    {
        foreach (var card in cards)
        {
            if (card != null)
            {
                card.ResetCard();
            }
        }
    }
}
