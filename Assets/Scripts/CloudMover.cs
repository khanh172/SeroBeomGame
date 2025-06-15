using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CloudMover : MonoBehaviour
{
    [Header("Tham số điều khiển")]
    [Tooltip("Nếu true: di chuyển từ trái sang phải; false: di chuyển từ phải sang trái")]
    public bool moveLeftToRight = true;

    [Tooltip("Khoảng thời gian để di chuyển từ startX đến endX (giây). Nếu <= 0 sẽ random giữa minDuration và maxDuration.")]
    public float duration = 0f;

    [Tooltip("Nếu duration <= 0, random thời gian di chuyển sẽ từ minDuration đến maxDuration.")]
    public float minDuration = 10f;
    public float maxDuration = 20f;

    [Tooltip("Delay khởi đầu trước khi cloud bắt đầu di chuyển (giây). Nếu <= 0 thì không delay.")]
    public float initialDelay = 0f;

    [Header("Tùy chọn random Y (anchoredPosition.y)")]
    public bool randomizeY = false;
    public float minY = -100f; 
    public float maxY = 100f;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private Tween moveTween;
    float marginFactor = 0.01f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("[CloudMover] Không tìm thấy Canvas parent cho " + name);
        }
    }

    void OnEnable()
    {
        StartMove();
    }

    void OnDisable()
    {

        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            moveTween = null;
        }
    }

    public void StartMove()
    {

        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            moveTween = null;
        }

        if (canvasRectTransform == null) return;

        float canvasWidth = canvasRectTransform.rect.width;

        float cloudWidth = rectTransform.rect.width;
        float startX, endX;
        if (moveLeftToRight)
        {
            startX = -canvasWidth * 0.5f - cloudWidth * marginFactor;
            endX = +canvasWidth * 0.5f + cloudWidth * marginFactor;
        }
        else
        {
            startX = +canvasWidth * 0.5f + cloudWidth * marginFactor;
            endX = -canvasWidth * 0.5f - cloudWidth * marginFactor;
        }

        Vector2 anchoredPos = rectTransform.anchoredPosition;
        if (randomizeY)
        {
            float randY = Random.Range(minY, maxY);
            anchoredPos.y = randY;
        }
        anchoredPos.x = startX;
        rectTransform.anchoredPosition = anchoredPos;

        float moveDuration = duration;
        if (moveDuration <= 0f)
        {
            moveDuration = Random.Range(minDuration, maxDuration);
        }

        if (initialDelay > 0f)
        {
            moveTween = rectTransform.DOAnchorPosX(endX, moveDuration)
                .SetDelay(initialDelay)
                .SetEase(Ease.Linear)
                .OnComplete(OnTweenComplete);
        }
        else
        {
            moveTween = rectTransform.DOAnchorPosX(endX, moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(OnTweenComplete);
        }
    }

    private void OnTweenComplete()
    {
        StartMove();
    }
}
