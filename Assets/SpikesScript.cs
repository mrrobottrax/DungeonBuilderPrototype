using UnityEngine;

public class SpikesScript : LogicTileBase
{
	public int m_UpFrameNumber = 0;
	public int m_DownFrameNumber = 1;
	public int m_MidFrameNumber = 2;

	public enum EState
	{
		DOWN,
		MID,
		UP
	}

	public EState m_State = EState.DOWN;

	public override void TileUpdateVisuals(GridManager gridManager)
	{
		int frameNumber = 0;

		switch (m_State)
		{
			case EState.UP:
				frameNumber = m_UpFrameNumber;
				break;

			case EState.DOWN:
				frameNumber = m_DownFrameNumber;
				break;

			case EState.MID:
				frameNumber = m_MidFrameNumber;
				break;
		}

		Vector3Int cellPos = gridManager.m_VisualTilemap.WorldToCell(transform.position);
		cellPos.z = 0;
		gridManager.m_VisualTilemap.SetAnimationFrame(cellPos, frameNumber);
	}

	public override void TileUpdateLogic(GridManager gridManager)
	{
		if (m_State == EState.UP)
		{
			m_State = EState.MID;
		}
		else if (m_State == EState.MID)
		{
			m_State = EState.DOWN;
		}
		else if (m_State == EState.DOWN)
		{
			// check if has entity above
			Vector2Int cellPos = gridManager.WorldToCell(transform.position);
			EntityBase entity = gridManager.GetEntityAt(cellPos.x, cellPos.y);
			if (entity != null)
			{
				m_State = EState.UP;
				entity.TakeDamage();

				// increase cost by 1 for every dead body
				int oldAdditionalCost = gridManager.GetAdditionalCost(cellPos.x, cellPos.y);
				gridManager.SetAdditionalCost(cellPos.x, cellPos.y, oldAdditionalCost + 1);
			}
		}
	}
}
