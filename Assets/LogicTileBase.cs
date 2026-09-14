using UnityEngine;

public class LogicTileBase : MonoBehaviour
{
	public virtual void TileUpdateVisuals(GridManager gridManager) { }
	public virtual void TileUpdateLogic(GridManager gridManager) { }
}
