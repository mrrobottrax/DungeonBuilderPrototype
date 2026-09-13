using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TileButton : MonoBehaviour, IBeginDragHandler, IDragHandler
{
	public TileBase m_Tile;
	public GameObject m_DraggedObject;

	public void OnBeginDrag(PointerEventData eventData)
	{
		Debug.Log("Begin Drag");
		GameObject newObject = Instantiate(m_DraggedObject);
		newObject.GetComponent<DraggedTile>().m_Tile = m_Tile;
		eventData.pointerDrag = newObject;
		ExecuteEvents.Execute(newObject, eventData, ExecuteEvents.beginDragHandler);
	}

	public void OnDrag(PointerEventData eventData)
	{ }
}
