using UnityEngine;

[CreateAssetMenu(fileName = "GridTileData", menuName = "Scriptable Objects/GridTileData")]
public class GridTileData : ScriptableObject
{
	public bool m_IsWalkable = true;
	public TileType m_Type;
}

public enum TileType { Ground, Wall }
