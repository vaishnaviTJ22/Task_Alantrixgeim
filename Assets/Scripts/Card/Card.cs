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
    private bool canClick = false;
    private Button cardButton;
    private Material frontMaterialInstance;

    private void Awake()
    {
        ValidateReferences();
        cardButton = GetComponent<Button>();

      
        DisableInteraction();
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
        if (state != CardState.FaceDown || isAnimating || !canClick)
        {
            return;
        }

        StartCoroutine(FlipToFront());
    }

    IEnumerator FlipToFront()
    {
        isAnimating = true;
        state = CardState.Flipping;

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

        back.SetActive(false);
        front.SetActive(true);

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

        GameManager.Instance?.OnCardRevealed(this);
    }

    public void FlipBack()
    {
        if (state == CardState.Matched) return;
        StartCoroutine(FlipToBack());
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

        front.SetActive(false);
        back.SetActive(true);

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
    }

    public void FlipInstant(bool showFront)
    {
        state = showFront ? CardState.FaceUp : CardState.FaceDown;
        front.SetActive(showFront);
        back.SetActive(!showFront);
        transform.rotation = Quaternion.Euler(0, showFront ? 180 : 0, 0);
    }

    public void SetMatched()
    {
        state = CardState.Matched;
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
        state = CardState.FaceDown;
        isAnimating = false;
        canClick = false;

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
        canClick = false;
        if (cardButton != null)
        {
            cardButton.interactable = false;
        }
    }

    public void EnableInteraction()
    {
        if (state != CardState.Matched)
        {
            canClick = true;
            if (cardButton != null)
            {
                cardButton.interactable = true;
            }
        }
    }
}
