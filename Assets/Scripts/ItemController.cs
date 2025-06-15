using UnityEngine;

public enum ItemType { Banana, Medicine }

public class ItemController : MonoBehaviour
{
    public ItemType itemType;
    [HideInInspector] public Vector2Int cellPosition;
}
