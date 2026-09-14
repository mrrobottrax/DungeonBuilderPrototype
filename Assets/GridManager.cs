using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	public Tilemap m_LogicTilemap;
	public Tilemap m_VisualTilemap;

	public int m_GoalX;
	public int m_GoalY;

	public TileBase m_DraggingTile;
	public Vector2Int m_DragPosition;
	public bool m_ShouldEndDrag;

	private EntityBase[,] m_EntityCache;
	private int m_EntityCacheMinX;
	private int m_EntityCacheMinY;

	public void Start()
	{
		// create logic and visual tilemaps
		Debug.Assert(m_LogicTilemap);
		Debug.Assert(!m_VisualTilemap);

		m_LogicTilemap.CompressBounds();

		// create visuals tilemap
		m_VisualTilemap = new GameObject("Visuals_Tilemap").AddComponent<Tilemap>();
		m_VisualTilemap.transform.SetParent(m_LogicTilemap.transform.parent);

		// copy tilemap fields
		m_VisualTilemap.color = m_LogicTilemap.color;
		m_VisualTilemap.tileAnchor = m_LogicTilemap.tileAnchor;
		m_VisualTilemap.orientation = m_LogicTilemap.orientation;

		// copy tilerenderer fields
		TilemapRenderer logicRenderer = m_LogicTilemap.GetComponent<TilemapRenderer>();
		TilemapRenderer visualsRenderer = m_VisualTilemap.gameObject.AddComponent<TilemapRenderer>();

		visualsRenderer.sortOrder = logicRenderer.sortOrder;
		visualsRenderer.mode = logicRenderer.mode;
		visualsRenderer.detectChunkCullingBounds = logicRenderer.detectChunkCullingBounds;

		visualsRenderer.sortingLayerName = logicRenderer.sortingLayerName;
		visualsRenderer.sortingLayerID = logicRenderer.sortingLayerID;
		visualsRenderer.sortingOrder = logicRenderer.sortingOrder;
		visualsRenderer.sharedMaterials = logicRenderer.sharedMaterials;
		visualsRenderer.renderingLayerMask = logicRenderer.renderingLayerMask;

		logicRenderer.enabled = false;

		// find gold tile
		GoldTileScript goldLocationObject = FindAnyObjectByType<GoldTileScript>();
		Vector3Int goldLocationCell = m_LogicTilemap.WorldToCell(goldLocationObject.transform.position);

		m_GoalX = goldLocationCell.x;
		m_GoalY = goldLocationCell.y;
	}

	public void Update()
	{
		UpdateVisualTilemap();

		// show preview
		if (m_DraggingTile)
		{
			Vector3Int tilePosition = new(m_DragPosition.x, m_DragPosition.y, 0);

			bool isValidDropSpot = m_LogicTilemap.GetTile(tilePosition) != null;

			if (isValidDropSpot)
			{
				m_VisualTilemap.SetTile(tilePosition, GetVisualOnlyTile(m_DraggingTile));
				if (m_ShouldEndDrag)
					m_LogicTilemap.SetTile(tilePosition, m_DraggingTile);
			}
			else
			{
				m_VisualTilemap.SetTile(tilePosition, GetVisualOnlyTile(m_DraggingTile));
				m_VisualTilemap.RemoveTileFlags(tilePosition, TileFlags.LockColor);
				m_VisualTilemap.SetColor(tilePosition, new Color(1, 0, 0, 1));
			}

			if (m_ShouldEndDrag)
				m_DraggingTile = null;
		}
		m_ShouldEndDrag = false;

		// update animations
		LogicTileBase[] logicTiles = FindObjectsByType<LogicTileBase>();
		foreach (LogicTileBase logicTile in logicTiles)
		{
			logicTile.TileUpdateVisuals(this);
		}
	}

	public void FixedUpdate()
	{
		UpdateEntityCache();
		MoveAllEntities();
		UpdateAllTiles();
	}

	public void MoveAllEntities()
	{
		EntityBase[] entities = FindObjectsByType<EntityBase>();
		foreach (EntityBase entity in entities)
		{
			EntityBase.Move move = entity.GetNextMove(this);
			if (move.x == 0 && move.y == 0) continue;

			Vector3Int cellPosition = m_LogicTilemap.WorldToCell(entity.transform.position);

			int gridX = cellPosition.x;
			int gridY = cellPosition.y;

			int newGridX = gridX + move.x;
			int newGridY = gridY + move.y;

			bool canMove = IsMovable(newGridX, newGridY);

			if (!canMove)
			{
				Debug.Log($"{entity.gameObject} failed to move {move.x}, {move.y}.", entity.gameObject);
				continue;
			}

			Vector3 world = m_LogicTilemap.CellToWorld(new Vector3Int(newGridX, newGridY, 0));
			world.y += m_LogicTilemap.cellSize.y / 2;
			entity.transform.position = world;

			UpdateEntityCache();
		}
	}

	public void UpdateAllTiles()
	{
		LogicTileBase[] logicTiles = FindObjectsByType<LogicTileBase>();
		foreach (LogicTileBase logicTile in logicTiles)
		{
			logicTile.TileUpdateLogic(this);
		}
	}

	public bool IsMovable(int gridX, int gridY)
	{
		bool isOOB = m_LogicTilemap.GetTile(new Vector3Int(gridX, gridY, 0)) == null;
		if (isOOB) return false;

		bool isBlocked = GetEntityAt(gridX, gridY);
		if (isBlocked) return false;

		return true;
	}

	public Vector2Int WorldToCell(Vector3 position)
	{
		Vector3Int positionCell = m_LogicTilemap.WorldToCell(position);
		return new Vector2Int(positionCell.x, positionCell.y);
	}

	public TileBase GetWorldGrid(int x, int y)
	{
		return m_LogicTilemap.GetTile(new Vector3Int(x, y, 0));
	}

	static Dictionary<TileBase, TileBase> s_VisualTileCache = new();
	public TileBase GetVisualOnlyTile(TileBase originalTile)
	{
		// this creates a new tile asset without the gameobject field set
		if (originalTile == null) return null;

		if (s_VisualTileCache.TryGetValue(originalTile, out TileBase newTile))
			return newTile;

		newTile = originalTile;

		if (originalTile is Tile stdTile)
		{
			Tile visualTile = ScriptableObject.CreateInstance<Tile>();
			visualTile.sprite = stdTile.sprite;
			visualTile.color = stdTile.color;
			visualTile.colliderType = Tile.ColliderType.None;
			newTile = visualTile;
		}
		else if (originalTile is SwitchableTile switchTile)
		{
			SwitchableTile visualTile = ScriptableObject.CreateInstance<SwitchableTile>();
			visualTile.m_Sprites = switchTile.m_Sprites;
			newTile = visualTile;
		}

		if (newTile == originalTile)
			Debug.LogError($"Unsupported tile of type {originalTile.GetType()}");
		else
			s_VisualTileCache.Add(originalTile, newTile);

		return newTile;
	}

	public void UpdateVisualTilemap()
	{
		m_LogicTilemap.CompressBounds();
		BoundsInt bounds = m_LogicTilemap.cellBounds;

		TileBase[] allTiles = m_LogicTilemap.GetTilesBlock(bounds);
		TileBase[] visualOnlyTiles = new TileBase[allTiles.Length];

		for (int i = 0; i < allTiles.Length; i++)
		{
			visualOnlyTiles[i] = GetVisualOnlyTile(allTiles[i]);
		}

		m_VisualTilemap.ClearAllTiles();
		m_VisualTilemap.SetTilesBlock(bounds, visualOnlyTiles);
	}

	public void BeginTileDrag(TileBase tile)
	{
		m_DraggingTile = tile;
	}

	public void OnTileDrag(Vector2 screenPosition)
	{
		Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
		Vector3Int cellPoint = m_VisualTilemap.WorldToCell(worldPoint);
		m_DragPosition = new Vector2Int(cellPoint.x, cellPoint.y);
	}

	public void EndTileDrag()
	{
		m_ShouldEndDrag = true;
	}

	public EntityBase GetEntityAt(int cellX, int cellY)
	{
		return m_EntityCache[cellX - m_EntityCacheMinX, cellY - m_EntityCacheMinY];
	}

	// cache entities in grid array for fast lookup
	public void UpdateEntityCache()
	{
		m_LogicTilemap.CompressBounds();
		BoundsInt bounds = m_LogicTilemap.cellBounds;

		if (m_EntityCache == null || m_EntityCache.GetLength(0) != bounds.size.x || m_EntityCache.GetLength(1) != bounds.size.y)
		{
			m_EntityCache = new EntityBase[bounds.size.x, bounds.size.y];
		}

		Array.Clear(m_EntityCache, 0, m_EntityCache.Length);

		EntityBase[] entities = FindObjectsByType<EntityBase>();
		foreach (EntityBase entity in entities)
		{
			Vector3 entityWorldPos = entity.transform.position;
			Vector3Int entityCellPos = m_LogicTilemap.WorldToCell(entityWorldPos);
			m_EntityCacheMinX = bounds.min.x;
			m_EntityCacheMinY = bounds.min.y;
			entityCellPos.x -= m_EntityCacheMinX;
			entityCellPos.y -= m_EntityCacheMinY;
			m_EntityCache[entityCellPos.x, entityCellPos.y] = entity;
		}
	}
}
