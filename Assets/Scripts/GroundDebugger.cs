using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class GroundDebugger : MonoBehaviour
{
    [Header("Tilemap Ground to Debug")]
    public Tilemap tilemapGround;

    [Header("Debug Settings")]
    [Tooltip("Nếu true, in ra cellBounds và count ô ground trong Start() hoặc khi đổi giá trị trong Inspector")]
    public bool debugPrintOnStart = true;
    [Tooltip("Nếu true, mỗi khi click chuột trái sẽ in ra ô (cell) dưới con trỏ và world center của ô đó")]
    public bool debugPrintOnClick = true;
    [Tooltip("Màu Gizmos để vẽ wire cube ô ground")]
    public Color gizmoColor = Color.green;
    [Tooltip("Nếu true, sẽ vẽ wire cube cho mỗi ô ground trong OnDrawGizmosSelected")]
    public bool drawGizmos = true;
    [Tooltip("Nếu true, sẽ in ra danh sách một số ô ground (cẩn thận nếu ground nhiều ô sẽ log rất nhiều)")]
    public bool debugPrintAllTiles = false;

    private BoundsInt lastBounds; 

    void Start()
    {
        if (Application.isPlaying && debugPrintOnStart)
        {
            PrintGroundInfo();
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying && debugPrintOnStart)
        {
            PrintGroundInfo();
        }
    }

    void PrintGroundInfo()
    {
        if (tilemapGround == null)
        {
            Debug.LogWarning("GroundDebugger: tilemapGround chưa gán!");
            return;
        }
        BoundsInt b = tilemapGround.cellBounds;
        int xMin = b.xMin, xMax = b.xMax - 1;
        int yMin = b.yMin, yMax = b.yMax - 1;
        Debug.Log($"[GroundDebugger] Ground cellBounds: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}");

        int count = 0;
        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int y = b.yMin; y < b.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemapGround.HasTile(cell))
                {
                    count++;
                    if (debugPrintAllTiles)
                    {
                        Debug.Log($"Ground tile at cell: ({x}, {y})");
                    }
                }
            }
        }
        Debug.Log($"[GroundDebugger] Total ground tiles (HasTile): {count}");
    }

    void Update()
    {
        if (Application.isPlaying && debugPrintOnClick && Input.GetMouseButtonDown(0))
        {
            PrintCellUnderMouse();
        }
    }

    private void PrintCellUnderMouse()
    {
        if (tilemapGround == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = tilemapGround.WorldToCell(mouseWorldPos);
        bool has = tilemapGround.HasTile(cellPos);
        Vector3 center = tilemapGround.GetCellCenterWorld(cellPos);
        Debug.Log($"[GroundDebugger] Clicked world pos: {mouseWorldPos}, cell under mouse: ({cellPos.x}, {cellPos.y}), HasTile={has}, cell center world: {center}");
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || tilemapGround == null) return;

        Gizmos.color = gizmoColor;
        BoundsInt b = tilemapGround.cellBounds;
        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int y = b.yMin; y < b.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemapGround.HasTile(cell))
                {
                    Vector3 center = tilemapGround.GetCellCenterWorld(cell);
                    Vector3Int one = new Vector3Int(1, 0, 0);
                    Vector3 centerNextX = tilemapGround.GetCellCenterWorld(cell + one);
                    float cellWidth = Mathf.Abs(centerNextX.x - center.x);
                    Vector3Int oneY = new Vector3Int(0, 1, 0);
                    Vector3 centerNextY = tilemapGround.GetCellCenterWorld(cell + oneY);
                    float cellHeight = Mathf.Abs(centerNextY.y - center.y);
                    Gizmos.DrawWireCube(center, new Vector3(cellWidth, cellHeight, 0.1f));
                }
            }
        }
    }
}
