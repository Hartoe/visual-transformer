using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildMenu : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] GameObject buildMenuLayout;
    [SerializeField] GameObject buildMenuItemPrefab;

    GameObject[] buildMenuItems;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GridBuildingSystem.Instance.SetBuildActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GridBuildingSystem.Instance.SetBuildActive(true);
    }

    void Start()
    {
        // Get the grid building system building list
        List<BuildingTypeSO> buildingList = GridBuildingSystem.Instance.GetBuildingList();

        buildMenuItems = new GameObject[buildingList.Count];
        for (int i = 0; i < buildingList.Count; i++)
        {
            GameObject newBuildItem = Instantiate(buildMenuItemPrefab);
            newBuildItem.GetComponent<BuildMenuItem>()._reference = buildingList[i];
            newBuildItem.transform.SetParent(buildMenuLayout.transform);
            newBuildItem.transform.localScale = new Vector3(1,1,1);

            buildMenuItems[i] = newBuildItem;
        }
    }
}
