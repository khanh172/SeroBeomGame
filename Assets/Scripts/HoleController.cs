using UnityEngine;

public class HoleController : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
    private SpriteRenderer sr;
    private bool isOpen = false;
    public Vector2Int cellPosition;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) Debug.LogError("HoleController cần SpriteRenderer.");
        if (closedSprite != null && sr != null) sr.sprite = closedSprite;
        isOpen = false;
    }

    public void Open()
    {
        if (openSprite != null && sr != null)
        {
            sr.sprite = openSprite;
        }
        isOpen = true;
    }

    public void SetClosedState()
    {
        if (closedSprite != null && sr != null)
        {
            sr.sprite = closedSprite;
        }
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
