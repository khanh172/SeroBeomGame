using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Map Prefabs (Level1, Level2, ...)")]
    public GameObject[] mapPrefabs;

    [Header("Snake Controller")]
    public SnakeController snakeController;

    [Header("Item Prefabs")]
    public Transform bananaPrefab;
    public Transform medicinePrefab;
    public Transform holePrefab; 

    [Header("Push Animation Settings")]
    [Tooltip("Thời gian tween khi đẩy item (banana hoặc medicine)")]
    public float itemPushDuration = 0.2f;

    private GameObject currentMapInstance;
    private LevelData currentLevelData;
    private Tilemap tilemapGround, tilemapObstacle;
    private GridLayout gridLayout;
    private int currentLevel = 0;

    private Dictionary<Vector2Int, ItemController> currentBananas = new Dictionary<Vector2Int, ItemController>();
    private Dictionary<Vector2Int, ItemController> currentMedicines = new Dictionary<Vector2Int, ItemController>();
    private HoleController currentHole;

    private Stack<GameState> stateStack = new Stack<GameState>();

    [Header("UI")]
    public TMP_Text gameOverText; 
    private bool isGameOver = false;

    private bool isReplayPromptActive = false;
    
    public bool IsGameOver() => isGameOver;

    void Start()
    {
        if (snakeController == null)
        {
            Debug.LogError("[LevelManager] Thiếu SnakeController.");
            return;
        }
        if (mapPrefabs == null || mapPrefabs.Length == 0)
        {
            Debug.LogError("[LevelManager] mapPrefabs chưa gán hoặc rỗng.");
            return;
        }
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        snakeController.levelManager = this;
        snakeController.enabled = true;

        LoadLevel(0);
    }
    void Update()
    {
        if (!isGameOver && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        if (isReplayPromptActive || isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
                OnReplayButtonPressed();
        }
    }
    private void LoadLevel(int levelIndex)
    {
        isGameOver = false;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
            gameOverText.rectTransform.DOKill();
        }

        Debug.Log($"[LevelManager] LoadLevel called with index = {levelIndex}");
        if (mapPrefabs == null || levelIndex < 0 || levelIndex >= mapPrefabs.Length)
        {
            Debug.LogError($"[LevelManager] LoadLevel: index sai {levelIndex}");
            return;
        }
        stateStack.Clear();

        if (currentMapInstance != null)
        {
            ClearAllItems();
            if (currentHole != null)
            {
                Destroy(currentHole.gameObject);
                currentHole = null;
            }
            Destroy(currentMapInstance);
        }

        currentLevel = levelIndex;
        currentMapInstance = Instantiate(mapPrefabs[levelIndex], Vector3.zero, Quaternion.identity, transform);
        currentLevelData = currentMapInstance.GetComponent<LevelData>();
        if (currentLevelData == null)
        {
            Debug.LogWarning($"[LevelManager] Prefab level tại index {levelIndex} thiếu component LevelData.");
            return;
        }

        gridLayout = currentMapInstance.GetComponent<GridLayout>()
                     ?? currentMapInstance.GetComponentInChildren<GridLayout>();
        if (gridLayout == null)
        {
            Debug.LogWarning($"[LevelManager] Không tìm thấy GridLayout trong prefab {mapPrefabs[levelIndex].name}.");
        }

        tilemapGround = currentLevelData.groundTilemap;
        tilemapObstacle = currentLevelData.obstacleTilemap;
        if (tilemapGround == null)
        {
            Debug.LogWarning($"[LevelManager] groundTilemap null trong LevelData của {mapPrefabs[levelIndex].name}.");
        }
        snakeController.gridLayout = gridLayout;
        snakeController.tilemapGround = tilemapGround;
        snakeController.tilemapObstacle = tilemapObstacle;

        var startPositions = new List<Vector2Int>(currentLevelData.snakeStartPositions);
        if (tilemapGround != null)
        {
            foreach (var p in startPositions)
            {
                bool ok = tilemapGround.HasTile((Vector3Int)p);
            }
        }
        snakeController.ResetSnake(startPositions, currentLevelData.snakeStartDirection);

        currentBananas.Clear();
        currentMedicines.Clear();

        foreach (var b in currentLevelData.bananaPositions)
        {
            if (tilemapGround != null && !tilemapGround.HasTile((Vector3Int)b))
            {
                Debug.LogWarning($"[LevelManager] BananaPosition {b} không nằm trên groundTilemap.");
                continue;
            }
            PlaceBananaAt(b);
        }
        foreach (var m in currentLevelData.medicinePositions)
        {
            if (tilemapGround != null && !tilemapGround.HasTile((Vector3Int)m))
            {
                Debug.LogWarning($"[LevelManager] MedicinePosition {m} không nằm trên groundTilemap.");
                continue;
            }
            PlaceMedicineAt(m);
        }
        Vector2Int holeCell = currentLevelData.holePosition;
        if (tilemapGround != null && !tilemapGround.HasTile((Vector3Int)holeCell))
        {
            Debug.LogWarning($"[LevelManager] HolePosition {holeCell} không nằm trên groundTilemap.");
        }
        else
        {
            PlaceHoleAt(holeCell, currentLevelData.holeClosedSprite, currentLevelData.holeOpenSprite);
        }

        SaveState();
    }

    private void PlaceBananaAt(Vector2Int cell)
    {
        if (bananaPrefab == null || tilemapGround == null) return;
        if (snakeController.positions != null && snakeController.positions.Contains(cell)) return;
        Vector3 world = tilemapGround.GetCellCenterWorld((Vector3Int)cell);
        Transform inst = Instantiate(bananaPrefab, world, Quaternion.identity, currentMapInstance.transform);
        ItemController ic = inst.GetComponent<ItemController>();
        if (ic != null)
        {
            ic.itemType = ItemType.Banana;
            ic.cellPosition = cell;
        }
        currentBananas[cell] = ic;
    }

    private void PlaceMedicineAt(Vector2Int cell)
    {
        if (medicinePrefab == null || tilemapGround == null) return;
        if (snakeController.positions != null && snakeController.positions.Contains(cell)) return;
        Vector3 world = tilemapGround.GetCellCenterWorld((Vector3Int)cell);
        Transform inst = Instantiate(medicinePrefab, world, Quaternion.identity, currentMapInstance.transform);
        ItemController ic = inst.GetComponent<ItemController>();
        if (ic != null)
        {
            ic.itemType = ItemType.Medicine;
            ic.cellPosition = cell;
        }
        currentMedicines[cell] = ic;
    }
    private void PlaceHoleAt(Vector2Int cell, Sprite closedSprite, Sprite openSprite)
    {
        if (holePrefab == null || tilemapGround == null) return;
        Vector3 world = tilemapGround.GetCellCenterWorld((Vector3Int)cell);
        Transform inst = Instantiate(holePrefab, world, Quaternion.identity, currentMapInstance.transform);
        HoleController hc = inst.GetComponent<HoleController>();
        if (hc != null)
        {
            hc.cellPosition = cell;
            hc.closedSprite = closedSprite;
            hc.openSprite = openSprite;
        }
        currentHole = hc;
    }

    public bool IsBananaAt(Vector2Int cell) => currentBananas.ContainsKey(cell);
    public bool IsMedicineAt(Vector2Int cell) => currentMedicines.ContainsKey(cell);
    public bool IsHoleAt(Vector2Int cell)
    {
        return currentHole != null && currentHole.cellPosition == cell && currentHole.IsOpen();
    }

    public void HandleItemEaten(Vector2Int headPos, Vector2Int dir)
    {
        if (currentBananas.TryGetValue(headPos, out ItemController bc))
        {
            currentBananas.Remove(headPos);
            Destroy(bc.gameObject);
            if (currentBananas.Count == 0 && currentHole != null)
                currentHole.Open();
        }
        else if (currentMedicines.TryGetValue(headPos, out ItemController mc))
        {
            currentMedicines.Remove(headPos);
            Destroy(mc.gameObject);
            snakeController.StartInfinitePush(-dir);
        }
        else
        {
            Debug.LogWarning($"HandleItemEaten: không tìm thấy item tại {headPos}");
        }
    }

    public void HandleEnterHole(Vector2Int headPos)
    {
        if (currentHole != null && currentHole.cellPosition == headPos && currentHole.IsOpen())
        {
            int next = currentLevel + 1;
            if (next < mapPrefabs.Length)
                LoadLevel(next);
            else
                Debug.Log("[LevelManager] Không còn level tiếp theo.");
        }
    }

    public void SaveState()
    {
        GameState gs = new GameState
        {
            snakePositions = new List<Vector2Int>(snakeController.positions),
            snakeDirection = snakeController.GetCurrentDirection(),
            bananaPositions = new List<Vector2Int>(currentBananas.Keys),
            medicinePositions = new List<Vector2Int>(currentMedicines.Keys),
            holeOpen = (currentHole != null && currentHole.IsOpen())
        };
        stateStack.Push(gs);
    }

    public void Undo()
    {
        if (isGameOver)
            return;

        if (snakeController != null && snakeController.IsPushing)
        {
            return;
        }
        if (stateStack.Count <= 1)
        {
            return;
        }
        stateStack.Pop();
        GameState gs = stateStack.Peek();
        snakeController.StopAllPushImmediate();
        snakeController.ResetSnake(gs.snakePositions, gs.snakeDirection);
        ClearAllItems();
        foreach (var b in gs.bananaPositions)
            PlaceBananaAt(b);
        foreach (var m in gs.medicinePositions)
            PlaceMedicineAt(m);
        if (currentHole != null)
        {
            if (gs.holeOpen) currentHole.Open();
            else currentHole.SetClosedState();
        }
    }

    private void ClearAllItems()
    {
        foreach (var kv in currentBananas)
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        currentBananas.Clear();
        foreach (var kv in currentMedicines)
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        currentMedicines.Clear();
    }
    public void PushItem(Vector2Int fromCell, Vector2Int toCell, Vector2Int dir)
    {
        if (currentBananas.TryGetValue(fromCell, out ItemController bc))
        {
            currentBananas.Remove(fromCell);
            bool inGround = (tilemapGround != null && tilemapGround.HasTile((Vector3Int)toCell));
            if (!inGround)
            {
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxItemFall != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxItemFall);

                Vector3 worldPos = bc.transform.position;
                float fallDistance = 4f;
                float fallDuration = 0.5f;
                bc.transform.DOMove(worldPos + Vector3.down * fallDistance, fallDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (bc != null && bc.gameObject != null)
                            Destroy(bc.gameObject);
                        HandleBananaFellOutGameOver();
                    });
            }
            else
            {
                currentBananas[toCell] = bc;
                bc.cellPosition = toCell;
                Vector3 targetWorld = tilemapGround.GetCellCenterWorld((Vector3Int)toCell);
                bc.transform.DOMove(targetWorld, itemPushDuration).SetEase(Ease.Linear);
            }
        }
        else if (currentMedicines.TryGetValue(fromCell, out ItemController mc))
        {
            currentMedicines.Remove(fromCell);
            bool inGround = (tilemapGround != null && tilemapGround.HasTile((Vector3Int)toCell));
            if (!inGround)
            {
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxItemFall != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxItemFall);

                Vector3 worldPos = mc.transform.position;
                float fallDistance = 4f;
                float fallDuration = 0.5f;
                mc.transform.DOMove(worldPos + Vector3.down * fallDistance, fallDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (mc != null && mc.gameObject != null)
                            Destroy(mc.gameObject);
                        
                    });
            }
            else
            {
                currentMedicines[toCell] = mc;
                mc.cellPosition = toCell;
                Vector3 targetWorld = tilemapGround.GetCellCenterWorld((Vector3Int)toCell);
                mc.transform.DOMove(targetWorld, itemPushDuration).SetEase(Ease.Linear);
            }
        }
        else
        {
            Debug.LogWarning($"PushItem: không tìm thấy item tại {fromCell}");
        }
    }
    public void OnReplayButtonPressed()
    {
        isReplayPromptActive = false;
        LoadLevel(currentLevel);
    }
    public void OnHomeButtonPressed()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void OnSnakeFellOut()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (snakeController != null)
            snakeController.enabled = false;

        if (gameOverText != null)
        {
            gameOverText.text = "Press R to Replay";
            gameOverText.gameObject.SetActive(true);

            gameOverText.rectTransform.DOKill();

            Vector3 originalPos = gameOverText.rectTransform.localPosition;

            float offsetY = 10f;
            float duration = 1f;
            gameOverText.rectTransform.localPosition = originalPos;
            gameOverText.rectTransform
                .DOLocalMoveY(originalPos.y + offsetY, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        Debug.Log("[LevelManager] Snake fell out => Game Over!");
    }

    private void HandleBananaFellOutGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (snakeController != null)
            snakeController.enabled = false;

        if (gameOverText != null)
        {
            gameOverText.text = "Press R to Play Again";
            gameOverText.gameObject.SetActive(true);

            // — Dừng mọi tween cũ
            gameOverText.rectTransform.DOKill();

            // — Lặp tween yoyo lên xuống
            Vector3 orig = gameOverText.rectTransform.localPosition;
            float offsetY = 10f;
            float duration = 1f;
            gameOverText.rectTransform
                .DOLocalMoveY(orig.y + offsetY, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    public void ShowReplayPrompt()
    {
        if (gameOverText == null) return;
        isReplayPromptActive = true;
        gameOverText.text = "Press R to Replay";
        gameOverText.gameObject.SetActive(true);

        gameOverText.rectTransform.DOKill();
        Vector3 orig = gameOverText.rectTransform.localPosition;
        gameOverText.rectTransform
            .DOLocalMoveY(orig.y + 10f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}


public class GameState
{
    public List<Vector2Int> snakePositions;
    public Vector2Int snakeDirection;
    public List<Vector2Int> bananaPositions;
    public List<Vector2Int> medicinePositions;
    public bool holeOpen;
}
