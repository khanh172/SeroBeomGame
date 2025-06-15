using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom/Alternate Tile")]
public class AlternateTile : RuleTile<AlternateTile.Neighbor>
{
    public class Neighbor : RuleTile.TilingRule.Neighbor { }

    public Sprite sprite1;
    public Sprite sprite2;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = ((position.x + position.y) % 2 == 0) ? sprite1 : sprite2;
        tileData.colliderType = Tile.ColliderType.Sprite;
    }
}
