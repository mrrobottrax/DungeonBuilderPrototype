using UnityEngine;
using UnityEngine.Tilemaps;

// A tile that can have the current sprite switched by using animation frame
[CreateAssetMenu(fileName = "SwitchableTile", menuName = "Scriptable Objects/SwitchableTile")]
public class SwitchableTile : TileBase
{
	public GameObject m_GameObjectToSpawn;
	public Sprite[] m_Sprites;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		tileData.transform = Matrix4x4.identity;
		tileData.color = Color.white;
		tileData.gameObject = m_GameObjectToSpawn;
		tileData.colliderType = Tile.ColliderType.None;
		tileData.flags = TileFlags.LockTransform | TileFlags.LockColor | TileFlags.InstantiateGameObjectRuntimeOnly;

		if (m_Sprites != null && m_Sprites.Length > 0)
		{
			tileData.sprite = m_Sprites[0];
		}
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		if (m_Sprites != null && m_Sprites.Length > 0)
		{
			tileAnimationData.animatedSprites = m_Sprites;
			tileAnimationData.animationSpeed = 0;
			tileAnimationData.animationStartTime = 0;
			tileAnimationData.flags = TileAnimationFlags.PauseAnimation;
			return true;
		}

		return false;
	}
}