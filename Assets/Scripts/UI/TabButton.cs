using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerClickHandler
{
    public bool Selected {get {return isSelected;}}
    public UnityEvent<bool> OnTabSelected = new UnityEvent<bool>();
    [SerializeField] bool isSelected = false;
    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;

    private Image panel;

    // Start is called before the first frame update
    void Start()
    {
        panel = GetComponent<Image>();
    }

    public void SwitchTab()
    {
        isSelected = !isSelected;
        if (isSelected) panel.color = selectedColor;
        else panel.color = unselectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SwitchTab();
            OnTabSelected.Invoke(isSelected);
        }
    }
}
