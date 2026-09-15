using UnityEngine;
using UnityEngine.EventSystems;

public class GoldObjectScript : MonoBehaviour, IPointerClickHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		FindAnyObjectByType<GridManager>().AddGold(2);
		Destroy(gameObject);
	}
}
