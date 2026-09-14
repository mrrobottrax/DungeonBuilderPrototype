using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TileButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public TileBase m_Tile;

	public void OnBeginDrag(PointerEventData eventData)
	{
		GridManager gridManager = FindAnyObjectByType<GridManager>();
		gridManager.BeginTileDrag(m_Tile);
	}

	public void OnDrag(PointerEventData eventData)
	{
		GridManager gridManager = FindAnyObjectByType<GridManager>();
		gridManager.OnTileDrag(eventData.position);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		GridManager gridManager = FindAnyObjectByType<GridManager>();
		gridManager.EndTileDrag();
	}
}
