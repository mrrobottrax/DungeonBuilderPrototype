using UnityEngine;

public class WallhuggerBehaviour : GuyBehaviour
{
	public int m_FacingDir = 0; // 0 = up, counter-clockwise
	public bool m_HasWallToLeft;

	public override Move GetNextMove(GridManager grid)
	{
		Vector2Int cellPosition = grid.WorldToCell(transform.position);
		Vector2Int forward = new();
		Vector2Int left = new();

		switch (m_FacingDir)
		{
			case 0:
				forward.y += 1;
				left.x -= 1;
				break;
			case 1:
				forward.x -= 1;
				left.y -= 1;
				break;
			case 2:
				forward.y -= 1;
				left.x += 1;
				break;
			case 3:
				forward.x += 1;
				left.y += 1;
				break;
			default:
				forward.y += 1;
				left.x -= 1;
				break;
		}

		Vector2Int right = -left;

		Vector2Int forwardCellPosition = cellPosition + forward;
		Vector2Int leftCellPosition = cellPosition + left;
		Vector2Int rightCellPosition = cellPosition + right;

		bool hadWallToLeft = m_HasWallToLeft;
		m_HasWallToLeft = false;

		if (!grid.GetWorldGrid(leftCellPosition.x, leftCellPosition.y))
			m_HasWallToLeft = true;

		// if we have a wall to the left and free space, move forwards
		if (!grid.GetWorldGrid(leftCellPosition.x, leftCellPosition.y) &&
			grid.GetWorldGrid(forwardCellPosition.x, forwardCellPosition.y))
		{
			return new Move
			{
				x = forward.x,
				y = forward.y
			};
		}

		// if we have had a wall to the left last frame, and none there this frame, turn and move left
		if (hadWallToLeft && !m_HasWallToLeft)
		{
			m_FacingDir = (m_FacingDir + 1) % 4;
			return new Move
			{
				x = left.x,
				y = left.y
			};
		}

		// if we have a wall in front, turn and move right
		if (!grid.GetWorldGrid(forwardCellPosition.x, forwardCellPosition.y))
		{
			m_FacingDir = (m_FacingDir + 3) % 4;
			return new Move
			{
				x = right.x,
				y = right.y
			};
		}

		// move forwards
		return new Move
		{
			x = forward.x,
			y = forward.y
		};
	}
}
