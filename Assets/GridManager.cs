using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
	public Tilemap m_LogicTilemap;
	public Tilemap m_VisualTilemap;

	public int m_GoalX;
	public int m_GoalY;

	public TileBase m_DraggingTile;
	public Vector2Int m_DragPosition;
	public bool m_ShouldEndDrag;

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
		}
		m_ShouldEndDrag = false;
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
			if (move.x == 0 && move.y == 0) continue;

			Vector3Int cellPosition = m_LogicTilemap.WorldToCell(entity.transform.position);

			int gridX = cellPosition.x;
			int gridY = cellPosition.y;

			int newGridX = gridX + move.x;
			int newGridY = gridY + move.y;

			bool canMove = true;

			//GuyBehaviour entityOnDestSquare = GetEntityGrid(newGridX, newGridY);
			//canMove &= !entityOnDestSquare;

			//TileBase tileOnDestSquare = GetWorldGrid(newGridX, newGridY);
			//canMove &= tileOnDestSquare;

			//if (!canMove)
			//{
			//	Debug.Log($"{entity.gameObject} failed to move {move.x}, {move.y}.", entity.gameObject);
			//	continue;
			//}

			//SetEntityGrid(gridX, gridY, null);
			//SetEntityGrid(newGridX, newGridY, entity);
		}

		// update sprites to match internal representation
		//for (int y = 0; y < m_EntityGrid.GetLength(1); ++y)
		//	for (int x = 0; x < m_EntityGrid.GetLength(0); ++x)
		//	{
		//		GuyBehaviour entity = m_EntityGrid[x, y];
		//		if (!entity) continue;

		//		int gridX = x + m_MinX;
		//		int gridY = y + m_MinY;

		//		Vector3Int cell = new()
		//		{
		//			x = gridX,
		//			y = gridY
		//		};
		//		Vector3 world = m_DungeonTilemap.CellToWorld(cell);
		//		world.y += m_DungeonTilemap.cellSize.y / 2;
		//		entity.transform.position = world;
		//	}
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

	public TileBase GetVisualOnlyTile(TileBase originalTile)
	{
		if (originalTile is Tile stdTile)
		{
			Tile visualTile = ScriptableObject.CreateInstance<Tile>();
			visualTile.sprite = stdTile.sprite;
			visualTile.color = stdTile.color;
			visualTile.colliderType = Tile.ColliderType.None;
			return visualTile;
		}

		return originalTile;
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
}
