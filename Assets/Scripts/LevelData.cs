using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class LevelData : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;      
    public Tilemap obstacleTilemap;    

    [Header("Snake Start")]
    public List<Vector2Int> snakeStartPositions = new List<Vector2Int>();
    public Vector2Int snakeStartDirection = Vector2Int.right;

    [Header("Banana Positions")]
    public List<Vector2Int> bananaPositions = new List<Vector2Int>();

    [Header("Medicine Positions")]
    public List<Vector2Int> medicinePositions = new List<Vector2Int>();

    [Header("Hole")]
    public Vector2Int holePosition;
    public Sprite holeClosedSprite;
    public Sprite holeOpenSprite;

}
