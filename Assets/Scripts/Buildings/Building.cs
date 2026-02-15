using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public static Building Create(Vector3 worldPosition, Vector2Int origin, BuildingTypeSO.Dir dir, BuildingTypeSO buildingTypeSO)
    {
        Transform buildingTransform = Instantiate(buildingTypeSO.prefab,
                                                  worldPosition,
                                                  Quaternion.Euler(0, buildingTypeSO.GetRotationAngle(dir), 0));

        Building building = buildingTransform.GetComponent<Building>();
        building.buildingTypeSO = buildingTypeSO;
        building.origin = origin;
        building.dir = dir;

        return building;
    }

    protected BuildingTypeSO buildingTypeSO;
    protected Vector2Int origin;
    protected BuildingTypeSO.Dir dir;
    protected GameObject infoPanelInstance;
    protected GameObject infoPanelParent;

    void Start()
    {
        //infoPanelParent = 
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return buildingTypeSO.GetGridPositionList(origin, dir);
    }

    public BuildingTypeSO GetBuildingTypeSO() => buildingTypeSO;
    public BuildingTypeSO.Dir GetDir() => dir;

    public void ShowInfoPanel()
    {
        infoPanelParent = GameObject.Find("Info Panel Screen");
        foreach (Transform child in infoPanelParent.transform)
        {
            Destroy(child.gameObject);
        }
        infoPanelInstance = Instantiate(buildingTypeSO.infoPanel, infoPanelParent.transform);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
