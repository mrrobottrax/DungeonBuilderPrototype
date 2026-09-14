using UnityEngine;

public class DoorScript : LogicTileBase
{
	public GameObject m_GameObjectToSpawn;
	public int m_SpawnEveryXTicks = 30;

	public int m_TickCounter = 0;

	public override void TileUpdateLogic(GridManager gridManager)
	{
		if (m_TickCounter >= m_SpawnEveryXTicks)
		{
			m_TickCounter = 0;
			Instantiate(m_GameObjectToSpawn).transform.position = transform.position;
		}

		++m_TickCounter;
	}
}
