using UnityEngine;
using UnityEngine.EventSystems;

public class PanelExit : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Destroy(transform.parent.gameObject);
    }
}
