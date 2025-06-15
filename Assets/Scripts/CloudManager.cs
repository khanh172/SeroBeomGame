using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CloudManager : MonoBehaviour
{
    [Header("Danh sách các Cloud UI (RectTransform)")]
    public List<RectTransform> cloudRects;

    [Header("Thiết lập chung")]
    public bool moveLeftToRight = true;
    public float minDuration = 10f;
    public float maxDuration = 20f;
    public float minInitialDelay = 0f;
    public float maxInitialDelay = 5f;

    [Header("Randomize Y?")]
    public bool randomizeY = false;
    public float minY = -100f;
    public float maxY = 100f;
    float marginFactor = 0.1f;
    private RectTransform canvasRectTransform;

    void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    void Start()
    {
        foreach (var rt in cloudRects)
        {
            StartCloud(rt);
        }
    }

    private void StartCloud(RectTransform rt)
    {
        if (canvasRectTransform == null || rt == null) return;

        float canvasWidth = canvasRectTransform.rect.width;
        float cloudWidth = rt.rect.width;

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

        Vector2 anchoredPos = rt.anchoredPosition;
        if (randomizeY)
            anchoredPos.y = Random.Range(minY, maxY);
        anchoredPos.x = startX;
        rt.anchoredPosition = anchoredPos;

        float duration = Random.Range(minDuration, maxDuration);
        float delay = Random.Range(minInitialDelay, maxInitialDelay);

        Tween tween = rt.DOAnchorPosX(endX, duration)
                        .SetEase(Ease.Linear)
                        .SetDelay(delay)
                        .OnComplete(() => {
                           
                            StartCloud(rt);
                        });
    }
}
