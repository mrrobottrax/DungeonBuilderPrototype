using UnityEngine;

public class GuyBehaviour : MonoBehaviour
{
	public struct Move
	{
		public int X;
		public int Y;
	}

	void Start()
	{
		Debug.Log("I'm a guy!");
	}

	public Move GetNextMove(GridManager grid)
	{
		bool shouldMoveX = UnityEngine.Random.value < 0.5f;
		bool shouldMovePositive = UnityEngine.Random.value < 0.5f;

		return new Move
		{
			X = (shouldMoveX ? 1 : 0) * (shouldMovePositive ? 1 : -1),
			Y = (shouldMoveX ? 0 : 1) * (shouldMovePositive ? 1 : -1),
		}; ;
	}
}
