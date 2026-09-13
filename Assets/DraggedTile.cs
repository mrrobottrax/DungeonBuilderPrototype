using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class DraggedTile : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public TileBase m_Tile;

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);

		Tilemap dungeonTilemap = FindAnyObjectByType<GridManager>().m_DungeonTilemap;
		Vector3Int cellPoint = dungeonTilemap.WorldToCell(worldPoint);
		worldPoint = dungeonTilemap.CellToWorld(cellPoint);
		worldPoint.y += dungeonTilemap.cellSize.y / 2;

		transform.position = new Vector3(worldPoint.x, worldPoint.y, transform.position.z);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);

		Tilemap dungeonTilemap = FindAnyObjectByType<GridManager>().m_DungeonTilemap;
		Vector3Int cellPoint = dungeonTilemap.WorldToCell(worldPoint);
		cellPoint.z = 0;

		dungeonTilemap.SetTile(cellPoint, m_Tile);

		Destroy(gameObject);
	}
}
