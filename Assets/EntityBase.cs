using UnityEngine;

public class EntityBase : MonoBehaviour
{
	public enum EFacingDir
	{
		NORTH,
		EAST,
		SOUTH,
		WEST,
	}

	public struct Move
	{
		public int x;
		public int y;
	}

	public EFacingDir m_FacingDir;

	public virtual Move GetNextMove(GridManager grid)
	{
		bool shouldMove = UnityEngine.Random.value < 0.5f;
		bool shouldMoveX = UnityEngine.Random.value < 0.5f;
		bool shouldMovePositive = UnityEngine.Random.value < 0.5f;

		if (!shouldMove)
		{
			return new Move();
		}

		return new Move
		{
			x = (shouldMoveX ? 1 : 0) * (shouldMovePositive ? 1 : -1),
			y = (shouldMoveX ? 0 : 1) * (shouldMovePositive ? 1 : -1),
		};
	}

	public virtual void TakeDamage()
	{
		Destroy(gameObject);
	}
}
