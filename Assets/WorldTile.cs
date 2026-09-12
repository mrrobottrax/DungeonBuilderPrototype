using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldTile", menuName = "Scriptable Objects/WorldTile")]
public class WorldTile : Tile
{
	public bool m_IsWalkable = true;
	public TileType m_Type;
}

public enum TileType { Ground, Wall }
