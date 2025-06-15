using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using TMPro;

public class SnakeController : MonoBehaviour
{
    [Header("Grid & Tilemap")]
    public GridLayout gridLayout;
    public Tilemap tilemapObstacle;
    public Tilemap tilemapGround;

    [Header("UI")]
    public TextMeshProUGUI replayPrompt;

    [Header("Segment Prefab")]
    public Transform segmentPrefab;

    [Header("Sprites - Head")]
    public Sprite headUp, headDown, headLeft, headRight;
    [Header("Sprites - Tail")]
    public Sprite tailUp, tailDown, tailLeft, tailRight;
    [Header("Sprites - Body Straight")]
    public Sprite bodyHorizontal, bodyVertical;
    [Header("Sprites - Body Corner")]
    public Sprite cornerUpLeft, cornerUpRight, cornerDownLeft, cornerDownRight;

    [Header("Movement Settings")]
    public bool useTweenForMove = true;
    public float moveDuration = 0.2f;

    [Header("Push Settings")]
    public float pushStepInterval = 0.2f;
    public bool useTweenForPush = true;

    [HideInInspector] public List<Vector2Int> positions;
    private List<Transform> segmentTransforms;
    private Vector2Int direction;
    private Vector2Int visualDirection;

    private bool isPushing = false;
    private Vector2Int pushDir;
    private Coroutine pushCoroutine;
    public bool IsPushing => isPushing;

    private bool isFallingOut = false;

    [HideInInspector] public LevelManager levelManager;

    void Start()
    {
        if (levelManager == null)
        {
            InitializeSnakeWithDefaults();
        }

    }
    void InitializeSnakeWithDefaults()
    {
        positions = new List<Vector2Int>();
        segmentTransforms = new List<Transform>();
        direction = visualDirection = Vector2Int.right;
        Vector2Int pos = Vector2Int.zero;
        positions.Add(pos);
        for (int i = 1; i < 3; i++)
        {
            pos -= direction;
            positions.Add(pos);
        }
        CreateSegmentTransforms();
        UpdateSegmentsVisualInstant();
    }

    void Update()
    {

        if (isPushing) return;
        Vector2Int newDir = direction;
        if (Input.GetKeyDown(KeyCode.UpArrow)) newDir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newDir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newDir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newDir = Vector2Int.right;
        else return;

        TryChangeDirectionAndMove(newDir);

        if (Input.GetKeyDown(KeyCode.Z) && levelManager != null && !levelManager.IsGameOver())
        {
            levelManager?.Undo();
        }
    }
    public void OnUpButtonPressed()
    {
        if (isPushing) return;
        TryChangeDirectionAndMove(Vector2Int.up);
    }
    public void OnDownButtonPressed()
    {
        if (isPushing) return;
        TryChangeDirectionAndMove(Vector2Int.down);
    }
    public void OnLeftButtonPressed()
    {
        if (isPushing) return;
        TryChangeDirectionAndMove(Vector2Int.left);
    }
    public void OnRightButtonPressed()
    {
        if (isPushing) return;
        TryChangeDirectionAndMove(Vector2Int.right);
    }

    private void TryChangeDirectionAndMove(Vector2Int newDir)
    {
        if (positions != null && positions.Count > 1 && newDir + direction == Vector2Int.zero)
            return;

        if (newDir != direction)
        {
            direction = newDir;
            visualDirection = newDir;
            MoveNormal();
        }
        else
        {
            MoveNormal();
        }
    }

    public Vector2Int GetCurrentDirection()
    {
        return direction;
    }

    public void MoveNormal()
    {
        if (positions == null || positions.Count == 0) return;

        Vector2Int newHead = positions[0] + direction;
        if (tilemapObstacle != null && tilemapObstacle.GetTile((Vector3Int)newHead) != null)
            return;

        levelManager?.SaveState();

        bool didMove = false;

        if (levelManager != null && levelManager.IsBananaAt(newHead))
        {
            Vector2Int behind = newHead + direction;
            bool noStone = (tilemapObstacle == null || tilemapObstacle.GetTile((Vector3Int)behind) == null);
            bool noItem = !levelManager.IsBananaAt(behind) && !levelManager.IsMedicineAt(behind) && !levelManager.IsHoleAt(behind);
            bool canPush = noStone && noItem;
            if (canPush)
            {
                levelManager.PushItem(newHead, behind, direction);
                positions.Insert(0, newHead);
                positions.RemoveAt(positions.Count - 1);
                didMove = true;
            }
            else
            {
                levelManager.HandleItemEaten(newHead, direction);
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxEatBanana != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxEatBanana);
                positions.Insert(0, newHead);
                didMove = true;
            }
        }
        else if (levelManager != null && levelManager.IsMedicineAt(newHead))
        {
            Vector2Int behind = newHead + direction;
            bool noStone = (tilemapObstacle == null || tilemapObstacle.GetTile((Vector3Int)behind) == null);
            bool noItem = !levelManager.IsBananaAt(behind) && !levelManager.IsMedicineAt(behind) && !levelManager.IsHoleAt(behind);
            bool canPush = noStone && noItem;
            if (canPush)
            {
                levelManager.PushItem(newHead, behind, direction);
                positions.Insert(0, newHead);
                positions.RemoveAt(positions.Count - 1);
                didMove = true;
            }
            else
            {
                levelManager.HandleItemEaten(newHead, direction);
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxEatMedicine != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxEatMedicine);
               
            }
        }
        else
        {
            positions.Insert(0, newHead);
            positions.RemoveAt(positions.Count - 1);
            didMove = true;
        }

        if (didMove)
        {
            
            if (AudioManager.Instance != null && AudioManager.Instance.sfxMove != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxMove);

            bool anyInside = false;
            if (tilemapGround != null)
            {
                foreach (var cell in positions)
                {
                    if (tilemapGround.HasTile((Vector3Int)cell))
                    {
                        anyInside = true;
                        break;
                    }
                }
            }
            else anyInside = true;

            if (!anyInside)
            {
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxSnakeFall != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSnakeFall);

                AnimateFallOutAndGameOver();
                return;
            }

            visualDirection = direction;
            UpdateSegmentsVisual();

            if (levelManager != null && levelManager.IsHoleAt(positions[0]))
            {
                levelManager.HandleEnterHole(positions[0]);
            }
        }
    }

    public void StopAllPushImmediate()
    {
        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine);
            pushCoroutine = null;
        }
        isPushing = false;
    }

    public void ResetSnake(List<Vector2Int> startPositions, Vector2Int startDirection)
    {
        isFallingOut = false;

        if (segmentTransforms != null)
        {
            foreach (var seg in segmentTransforms)
            {
                if (seg != null)
                    seg.DOKill();
            }
        }
        StopAllPushImmediate();

        if (segmentTransforms != null)
        {
            foreach (var seg in segmentTransforms)
            {
                if (seg != null) Destroy(seg.gameObject);
            }
            segmentTransforms.Clear();
        }

        positions = new List<Vector2Int>(startPositions);
        direction = startDirection;
        visualDirection = startDirection;
        CreateSegmentTransforms();
        UpdateSegmentsVisualInstant();

        enabled = true;
    }

    private void CreateSegmentTransforms()
    {
        if (segmentTransforms == null) segmentTransforms = new List<Transform>();
        segmentTransforms.Clear();
        if (segmentPrefab == null) return;
        foreach (var pos in positions)
        {
            Transform seg = Instantiate(segmentPrefab, Vector3.zero, Quaternion.identity, transform);
            segmentTransforms.Add(seg);

        }

    }

    public void StartPush(Vector2Int dir, float duration)
    {
        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine);
            pushCoroutine = null;
        }
        pushDir = dir;
        isPushing = true;
        pushCoroutine = StartCoroutine(PushCoroutine(duration, finite: true));
    }

    public void StartInfinitePush(Vector2Int dir)
    {
        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine);
            pushCoroutine = null;
        }
        pushDir = dir;
        isPushing = true;
        pushCoroutine = StartCoroutine(PushCoroutine(0f, finite: false));

        StartCoroutine(ShowReplayPromptAfterDelay(4f));
    }

    private IEnumerator PushCoroutine(float duration, bool finite)
    {
        float elapsed = 0f;
        while (finite ? (elapsed < duration) : isPushing)
        {
            Vector2Int headPos = positions[0];
            Vector2Int newHead = headPos + pushDir;

            bool inGround = (tilemapGround != null && tilemapGround.HasTile((Vector3Int)newHead));
            bool hasStone = inGround && tilemapObstacle != null && tilemapObstacle.GetTile((Vector3Int)newHead) != null;
            if (hasStone)
            {
                Debug.Log("Push dừng vì gặp Stone tại " + newHead);
                isPushing = false;
                break;
            }

            if (levelManager != null)
            {
                if (levelManager.IsBananaAt(newHead) || levelManager.IsMedicineAt(newHead))
                {
                    Debug.Log("Push dừng vì gặp item trên ground tại " + newHead);
                    isPushing = false;
                    break;
                }
            }

            levelManager?.SaveState();

            positions.Insert(0, newHead);
            positions.RemoveAt(positions.Count - 1);

            visualDirection = pushDir;
            UpdateSegmentsVisual();

            yield return new WaitForSeconds(pushStepInterval);

            if (finite)
            {
                elapsed += pushStepInterval;
                if (elapsed >= duration) break;
            }
        }
        isPushing = false;
        pushCoroutine = null;
    }
    private void UpdateSegmentsVisual()
    {
        if (!useTweenForMove)
        {
            UpdateSegmentsVisualInstant();
            return;
        }
        int count = positions.Count;
        while (segmentTransforms.Count < count)
        {
            if (segmentPrefab)
            {
                var seg = Instantiate(segmentPrefab, Vector3.zero, Quaternion.identity, transform);
                segmentTransforms.Add(seg);
            }
            else break;
        }
        while (segmentTransforms.Count > count)
        {
            var seg = segmentTransforms[segmentTransforms.Count - 1];
            segmentTransforms.RemoveAt(segmentTransforms.Count - 1);
            if (seg) Destroy(seg.gameObject);
        }
        Vector3[] newCenters = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Vector2Int pos = positions[i];
            Vector3Int cellPos3 = new Vector3Int(pos.x, pos.y, 0);
            if (tilemapGround != null)
                newCenters[i] = tilemapGround.GetCellCenterWorld(cellPos3);
            else if (gridLayout != null)
                newCenters[i] = gridLayout.CellToWorld(cellPos3) + (gridLayout.cellSize / 2f);
            else
                newCenters[i] = Vector3.zero;
        }
        int completedTweens = 0;
        for (int i = 0; i < count; i++)
        {
            var segT = segmentTransforms[i];
            segT.DOMove(newCenters[i], moveDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                completedTweens++;
                if (completedTweens >= count)
                {
                    ApplySprites();
                }
            });
        }
        if (count == 0) return;
        if (count == 1)
        {
            ApplySprites();
        }
    }

    private void UpdateSegmentsVisualInstant()
    {
        if (positions == null || segmentTransforms == null) return;
        while (segmentTransforms.Count < positions.Count)
        {
            if (segmentPrefab)
            {
                var seg = Instantiate(segmentPrefab, Vector3.zero, Quaternion.identity, transform);
                segmentTransforms.Add(seg);
            }
            else break;
        }
        while (segmentTransforms.Count > positions.Count)
        {
            var seg = segmentTransforms[segmentTransforms.Count - 1];
            segmentTransforms.RemoveAt(segmentTransforms.Count - 1);
            if (seg) Destroy(seg.gameObject);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            Vector3 world;
            Vector3Int cellPos3 = new Vector3Int(pos.x, pos.y, 0);
            if (tilemapGround != null)
                world = tilemapGround.GetCellCenterWorld(cellPos3);
            else if (gridLayout != null)
                world = gridLayout.CellToWorld(cellPos3) + (gridLayout.cellSize / 2f);
            else
                world = Vector3.zero;
            var segT = segmentTransforms[i];
            segT.position = world;
        }
        ApplySprites();
    }

    private void AnimateFallOutAndGameOver()
    {
        if (isFallingOut) return;
        isFallingOut = true;

        StopAllPushImmediate();

        enabled = false;
        float fallDistance = 20f;
        float fallDuration = 0.5f;

        if (AudioManager.Instance != null && AudioManager.Instance.sfxSnakeFall != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSnakeFall);

        int count = segmentTransforms != null ? segmentTransforms.Count : 0;
        if (count == 0)
        {
            OnFallComplete();
            return;
        }

        int completed = 0;
        foreach (var seg in segmentTransforms)
        {
            if (seg == null)
            {
                completed++;
                continue;
            }
            Vector3 fromPos = seg.position;
            Vector3 toPos = fromPos + Vector3.down * fallDistance;
            seg.DOMove(toPos, fallDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                completed++;
                if (completed >= count)
                {
                    OnFallComplete();
                }
            });
        }
    }
    private IEnumerator ShowReplayPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isPushing && levelManager != null)
        {
            levelManager.ShowReplayPrompt();
        }
    }
    private void OnFallComplete()
    {
        if (levelManager != null)
        {
            levelManager.OnSnakeFellOut(); 
        }
        else
        {
            Debug.Log("Snake fell out - Game Over");
        }
    }
    private void ApplySprites()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            var segT = segmentTransforms[i];
            var sr = segT.GetComponent<SpriteRenderer>();
            if (!sr) continue;
            Sprite sprite = null;
            if (i == 0)
            {
                if (visualDirection == Vector2Int.up) sprite = headUp;
                else if (visualDirection == Vector2Int.down) sprite = headDown;
                else if (visualDirection == Vector2Int.left) sprite = headLeft;
                else if (visualDirection == Vector2Int.right) sprite = headRight;
            }
            else if (i == positions.Count - 1)
            {
                Vector2Int dirTail = positions[i - 1] - positions[i];
                if (dirTail == Vector2Int.up) sprite = tailUp;
                else if (dirTail == Vector2Int.down) sprite = tailDown;
                else if (dirTail == Vector2Int.left) sprite = tailLeft;
                else if (dirTail == Vector2Int.right) sprite = tailRight;
            }
            else
            {
                Vector2Int prev = positions[i - 1], next = positions[i + 1];
                Vector2Int dirPrev = prev - positions[i], dirNext = next - positions[i];
                if (dirPrev.x != 0 && dirNext.x != 0) sprite = bodyHorizontal;
                else if (dirPrev.y != 0 && dirNext.y != 0) sprite = bodyVertical;
                else
                {
                    bool up = dirPrev == Vector2Int.up || dirNext == Vector2Int.up;
                    bool down = dirPrev == Vector2Int.down || dirNext == Vector2Int.down;
                    bool left = dirPrev == Vector2Int.left || dirNext == Vector2Int.left;
                    bool right = dirPrev == Vector2Int.right || dirNext == Vector2Int.right;
                    if (up && right) sprite = cornerUpRight;
                    else if (up && left) sprite = cornerUpLeft;
                    else if (down && right) sprite = cornerDownRight;
                    else if (down && left) sprite = cornerDownLeft;
                    else sprite = bodyHorizontal;
                }
            }
            if (sprite == null && bodyHorizontal != null) sprite = bodyHorizontal;
            sr.sprite = sprite;
            var spSize = sr.sprite.bounds.size;
            if (spSize.x > 0.0001f && spSize.y > 0.0001f)
            {
                Vector3 cellWorldSize;
                if (tilemapGround != null)
                {
                    Vector3 c0 = tilemapGround.GetCellCenterWorld(Vector3Int.zero);
                    Vector3 c1 = tilemapGround.GetCellCenterWorld(Vector3Int.one);
                    cellWorldSize = c1 - c0;
                }
                else if (gridLayout != null)
                {
                    Vector3 w0 = gridLayout.CellToWorld(Vector3Int.zero);
                    Vector3 w1 = gridLayout.CellToWorld(Vector3Int.one);
                    cellWorldSize = w1 - w0;
                }
                else cellWorldSize = Vector3.one;
                segT.localScale = new Vector3(cellWorldSize.x / spSize.x, cellWorldSize.y / spSize.y, 1f);
            }
        }
    }
    
}
