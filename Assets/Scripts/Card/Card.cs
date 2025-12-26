using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CardState
{
    FaceDown,
    Flipping,
    FaceUp,
    Matched
}

public class Card : MonoBehaviour
{
    public int cardID;
    public CardState state = CardState.FaceDown;

    [Header("Visual References")]
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;

   
    [Header("Animation Settings")]
    public float flipSpeed = 0.3f;
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float matchScaleAmount = 1.2f;
    public float matchScaleDuration = 0.3f;

    private bool isAnimating;
    private bool isBeingProcessed = false;
    private Button cardButton;
    private Material frontMaterialInstance;
    private Coroutine currentFlipCoroutine;

    private void Awake()
    {
        ValidateReferences();
        cardButton = GetComponent<Button>();

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnClick);
        }

      
    }

    private void ValidateReferences()
    {
        if (front == null)
        {
            front = transform.Find("Front")?.gameObject;
        }

        if (back == null)
        {
            back = transform.Find("Back")?.gameObject;
        }

        if (frontImage == null && front != null)
        {
            frontImage = front.GetComponent<Image>();
        }

        if (backImage == null && back != null)
        {
            backImage = back.GetComponent<Image>();
        }
    }

    public void OnClick()
    {
        Debug.Log($"Card clicked! State: {state}, Animating: {isAnimating}, Processing: {isBeingProcessed}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        if (!GameManager.Instance.CanFlipCard(this))
        {
            Debug.Log("GameManager says card cannot be flipped");
            return;
        }

        if (state != CardState.FaceDown || isAnimating || isBeingProcessed)
        {
            Debug.Log($"Card blocked - State: {state}, Animating: {isAnimating}, Processing: {isBeingProcessed}");
            return;
        }

        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
        }

        currentFlipCoroutine = StartCoroutine(FlipToFront());
    }

    IEnumerator FlipToFront()
    {
        isAnimating = true;
        state = CardState.Flipping;

        if (cardButton != null)
        {
            cardButton.interactable = false;
        }

        AudioManager.Instance?.PlayFlip();

        float elapsed = 0f;
        float halfFlipTime = flipSpeed / 2f;

        while (elapsed < halfFlipTime)
        {
            elapsed += Time.deltaTime;
            float t = flipCurve.Evaluate(elapsed / halfFlipTime);
            float angle = Mathf.Lerp(0, 90, t);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        if (back != null) back.SetActive(false);
        if (front != null) front.SetActive(true);

        elapsed = 0f;
        while (elapsed < halfFlipTime)
        {
            elapsed += Time.deltaTime;
            float t = flipCurve.Evaluate(elapsed / halfFlipTime);
            float angle = Mathf.Lerp(90, 180, t);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 180, 0);
        state = CardState.FaceUp;
        isAnimating = false;
        currentFlipCoroutine = null;

        GameManager.Instance?.OnCardRevealed(this);
    }

    public void FlipBack()
    {
        if (state == CardState.Matched)
        {
            return;
        }

        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
        }

        currentFlipCoroutine = StartCoroutine(FlipToBack());
    }

    IEnumerator FlipToBack()
    {
        isAnimating = true;
        state = CardState.Flipping;

        float elapsed = 0f;
        float halfFlipTime = flipSpeed / 2f;

        while (elapsed < halfFlipTime)
        {
            elapsed += Time.deltaTime;
            float t = flipCurve.Evaluate(elapsed / halfFlipTime);
            float angle = Mathf.Lerp(180, 90, t);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        if (front != null) front.SetActive(false);
        if (back != null) back.SetActive(true);

        elapsed = 0f;
        while (elapsed < halfFlipTime)
        {
            elapsed += Time.deltaTime;
            float t = flipCurve.Evaluate(elapsed / halfFlipTime);
            float angle = Mathf.Lerp(90, 0, t);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, 0);
        state = CardState.FaceDown;
        isAnimating = false;
        currentFlipCoroutine = null;

        if (!isBeingProcessed)
        {
            EnableInteraction();
        }
    }

    public void FlipInstant(bool showFront)
    {
        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
            currentFlipCoroutine = null;
        }

        isAnimating = false;
        state = showFront ? CardState.FaceUp : CardState.FaceDown;
        if (front != null) front.SetActive(showFront);
        if (back != null) back.SetActive(!showFront);
        transform.rotation = Quaternion.Euler(0, showFront ? 180 : 0, 0);
    }

    public void SetProcessing(bool processing)
    {
        isBeingProcessed = processing;

        if (!processing && state == CardState.FaceDown)
        {
            Debug.Log("processing");
            EnableInteraction();
        }
    }

    public void SetMatched()
    {
        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
            currentFlipCoroutine = null;
        }

        state = CardState.Matched;
        isAnimating = false;
        isBeingProcessed = false;
        DisableInteraction();
        StartCoroutine(MatchAnimation());
    }

    IEnumerator MatchAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * matchScaleAmount;

        float elapsed = 0f;
        float halfDuration = matchScaleDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

   

    public void SetCardSprites(Sprite backSprite, Sprite frontSprite)
    {
        if (backImage != null && backSprite != null)
        {
            backImage.sprite = backSprite;
        }

        if (frontImage != null && frontSprite != null)
        {
            frontImage.sprite = frontSprite;
        }
    }

    public void SetFlipSpeed(float speed)
    {
        flipSpeed = Mathf.Max(0.1f, speed);
    }

    public void ResetCard()
    {
        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
            currentFlipCoroutine = null;
        }

        state = CardState.FaceDown;
        isAnimating = false;
        isBeingProcessed = false;

        if (front != null) front.SetActive(false);
        if (back != null) back.SetActive(true);

        if (frontImage != null)
        {
            Color color = frontImage.color;
            color.a = 1f;
            frontImage.color = color;
            frontImage.material = null;
        }

        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = Vector3.one;

        DisableInteraction();
    }

    public void DisableInteraction()
    {
        if (cardButton != null)
        {
            cardButton.interactable = false;
        }
    }

    public void EnableInteraction()
    {
        if (state == CardState.FaceDown && !isAnimating && !isBeingProcessed)
        {
            if (cardButton != null)
            {
                cardButton.interactable = true;
                Debug.Log($"Card {cardID} enabled");
            }
        }
        else
        {
            Debug.Log($"Card {cardID} NOT enabled - State: {state}, Animating: {isAnimating}, Processing: {isBeingProcessed}");
        }
    }
}
