using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform logoRect;
    public RectTransform playButtonRect;

    [Header("Animation Settings")]
    public float dropDistance = 200f; 
    public float logoDropDuration = 1f;
    public float buttonDropDuration = 1f;
    public float logoDropDelay = 0.2f;
    public float buttonDropDelay = 0.5f;
    public Ease dropEase = Ease.OutBounce; 

    void Start()
    {
        
        if (logoRect != null)
        {
           
            Vector2 targetLogoPos = logoRect.anchoredPosition;
             
            logoRect.anchoredPosition = new Vector2(targetLogoPos.x, targetLogoPos.y + dropDistance);
            
            logoRect.DOAnchorPos(targetLogoPos, logoDropDuration)
                .SetDelay(logoDropDelay)
                .SetEase(dropEase);
        }
        if (playButtonRect != null)
        {
            Vector2 targetButtonPos = playButtonRect.anchoredPosition;
            
            playButtonRect.anchoredPosition = new Vector2(targetButtonPos.x, targetButtonPos.y + dropDistance);
            
            playButtonRect.DOAnchorPos(targetButtonPos, buttonDropDuration)
                .SetDelay(buttonDropDelay)
                .SetEase(dropEase);
        }
    }

  
    public void PlayShowAnimation()
    {
        Start(); 
    }
}
