using System.Collections.Generic;
using UnityEngine;

public class PathfindingGuy : EntityBase
{
	class AStarCell
	{
		public AStarCell parent;
		public int x;
		public int y;
		public float g;
	}

	AStarCell AStar(int currentX, int currentY, GridManager grid)
	{
		// TODO: This is horrendously slow

		List<AStarCell> openCells = new();
		List<AStarCell> closedCells = new();

		openCells.Add(new AStarCell { x = currentX, y = currentY, g = 0 });

		AStarCell finalCell = null;
		while (openCells.Count > 0 && finalCell == null)
		{
			AStarCell newCell = new();
			float bestG = float.PositiveInfinity;
			foreach (AStarCell cell in openCells)
			{
				if (cell.g < bestG)
				{
					bestG = cell.g;
					newCell = cell;
				}
			}
			openCells.Remove(newCell);
			closedCells.Add(newCell);

			if (newCell.x == grid.m_GoalX && newCell.y == grid.m_GoalY)
			{
				return newCell;
			}

			List<AStarCell> nextCells = new();
			for (int x = -1; x <= 1; ++x)
				for (int y = -1; y <= 1; ++y)
				{
					if (x == 0 && y == 0)
						continue;

					// prevent diagonal movement
					if (x != 0 && y != 0)
						continue;

					int cellX = newCell.x + x;
					int cellY = newCell.y + y;

					bool isFree = grid.IsMovable(cellX, cellY);
					if (!isFree) continue;

					if (closedCells.Exists(c => c.x == cellX && c.y == cellY))
						continue;

					AStarCell cell = new() { parent = newCell, x = cellX, y = cellY, g = newCell.g + 1 };
					nextCells.Add(cell);
				}

			foreach (AStarCell successor in nextCells)
			{
				AStarCell openCell = openCells.Find(c => c.x == successor.x && c.y == successor.y);
				if (openCell == null)
				{
					openCells.Add(successor);
				}
				else if (successor.g < openCell.g)
				{
					openCells.Remove(openCell);
					openCells.Add(successor);
				}
			}
		}

		return null;
	}

	public override Move GetNextMove(GridManager grid)
	{
		Vector3Int currentPosCell = grid.m_LogicTilemap.WorldToCell(transform.position);
		int currentX = currentPosCell.x;
		int currentY = currentPosCell.y;

		AStarCell finalCell = AStar(currentX, currentY, grid);

		AStarCell nextCell = finalCell;
		while (nextCell != null)
		{
			if (nextCell.parent != null && nextCell.parent.x == currentX && nextCell.parent.y == currentY)
			{
				break;
			}

			nextCell = nextCell.parent;
		}

		if (nextCell == null)
		{
			return new Move();
		}

		int moveX = nextCell.x - currentX;
		int moveY = nextCell.y - currentY;

		return new Move { x = moveX, y = moveY };
	}
}
