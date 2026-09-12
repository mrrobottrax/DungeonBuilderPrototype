using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	public Tilemap m_DungeonTilemap;
	public Tilemap m_EntityTilemap;

	private GuyBehaviour[,] m_EntityGrid;
	int m_MinX;
	int m_MinY;

	public void Start()
	{
		if (m_EntityTilemap.TryGetComponent(out TilemapRenderer renderer))
		{
			renderer.enabled = false;
		}

		// get grid bounds
		m_DungeonTilemap.CompressBounds();
		BoundsInt bounds = m_DungeonTilemap.cellBounds;

		int width = bounds.max.x - bounds.min.x;
		int height = bounds.max.y - bounds.min.y;

		m_MinX = bounds.min.x;
		m_MinY = bounds.min.y;

		// get entities and store them in grid
		m_EntityGrid = new GuyBehaviour[width, height];

		GuyBehaviour[] entities = FindObjectsByType<GuyBehaviour>();

		foreach (GuyBehaviour entity in entities)
		{
			Vector3Int position = m_DungeonTilemap.WorldToCell(entity.transform.position);
			if (GetEntityGrid(position.x, position.y))
			{
				Debug.Log($"Duplicate entities at position ({position}). Previous: {GetEntityGrid(position.x, position.y)}. Destroying new: {entity}.");
				Destroy(entity.gameObject);
			}

			SetEntityGrid(position.x, position.y, entity);
		}
	}

	public void FixedUpdate()
	{
		MoveAllEntities();
	}

	public void MoveAllEntities()
	{
		GuyBehaviour[] entities = FindObjectsByType<GuyBehaviour>();

		// update internal representation
		foreach (GuyBehaviour entity in entities)
		{
			GuyBehaviour.Move move = entity.GetNextMove(this);

			Vector3Int cellPosition = m_DungeonTilemap.WorldToCell(entity.transform.position);

			int gridX = cellPosition.x;
			int gridY = cellPosition.y;

			int newGridX = gridX + move.X;
			int newGridY = gridY + move.Y;

			bool canMove = true;

			GuyBehaviour entityOnDestSquare = GetEntityGrid(newGridX, newGridY);
			canMove &= !entityOnDestSquare;

			TileBase tileOnDestSquare = GetWorldGrid(newGridX, newGridY);
			canMove &= tileOnDestSquare;

			if (!canMove)
			{
				Debug.Log($"{entity.gameObject} failed to move {move.X}, {move.Y}.", entity.gameObject);
				continue;
			}

			SetEntityGrid(gridX, gridY, null);
			SetEntityGrid(newGridX, newGridY, entity);
		}

		// update sprites to match internal representation
		for (int y = 0; y < m_EntityGrid.GetLength(1); ++y)
			for (int x = 0; x < m_EntityGrid.GetLength(0); ++x)
			{
				GuyBehaviour entity = m_EntityGrid[x, y];
				if (!entity) continue;

				int gridX = x + m_MinX;
				int gridY = y + m_MinY;

				Vector3Int cell = new()
				{
					x = gridX,
					y = gridY
				};
				Vector3 world = m_DungeonTilemap.CellToWorld(cell);
				world.y += m_DungeonTilemap.cellSize.y / 2;
				entity.transform.position = world;
			}
	}

	public TileBase GetWorldGrid(int x, int y)
	{
		return m_DungeonTilemap.GetTile(new Vector3Int(x, y, 0));
	}

	public GuyBehaviour GetEntityGrid(int x, int y)
	{
		x -= m_MinX;
		y -= m_MinY;

		if (x < 0)
			return null;

		if (y < 0)
			return null;

		if (x >= m_EntityGrid.GetLength(0))
			return null;

		if (y >= m_EntityGrid.GetLength(1))
			return null;

		return m_EntityGrid[x, y];
	}

	public void SetEntityGrid(int x, int y, GuyBehaviour entity)
	{
		x -= m_MinX;
		y -= m_MinY;

		if (x < 0)
			return;

		if (y < 0)
			return;

		if (x >= m_EntityGrid.GetLength(0))
			return;

		if (y >= m_EntityGrid.GetLength(1))
			return;

		m_EntityGrid[x, y] = entity;
	}
}
