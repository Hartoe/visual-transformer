using UnityEngine;
using UnityEngine.EventSystems;

public class DestroyOnClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject button;
    [SerializeField] AFactory factory;

    public void OnPointerClick(PointerEventData eventData)
    {
        button.SetActive(true);
        gameObject.SetActive(false);
        factory.Fix();
    }
}
