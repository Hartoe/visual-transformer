using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildMenuItem : MonoBehaviour, IPointerClickHandler
{
    public BuildingTypeSO _reference;

    [Header("Components")]
    [SerializeField] RawImage buildImage;
    [SerializeField] TextMeshProUGUI buildTitle;

    public void OnPointerClick(PointerEventData eventData)
    {
        GridBuildingSystem.Instance.SetBuildingType(_reference);
    }

    // Start is called before the first frame update
    void Start()
    {
        buildTitle.text = _reference.nameString;
        buildImage.texture = _reference.image;
    }
}
