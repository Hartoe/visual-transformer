using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    private Transform visual;

    // Start is called before the first frame update
    void Start()
    {
        RefreshVisuals();

        GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshVisuals();
    }

    void LateUpdate()
    {
        if (GridBuildingSystem.Instance.GetBuildActive())
        {
            Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition() + GridBuildingSystem.Instance.GetBuildingPositionOffset();
            targetPosition.y = 1f;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem.Instance.GetBuildingRotation(), Time.deltaTime * 15f);
            if (!visual.gameObject.activeSelf) visual.gameObject.SetActive(true);
        }
        else
        {
            visual.gameObject.SetActive(false);
        }
    }

    private void RefreshVisuals()
    {
        if (visual != null)
        {
            Destroy(visual.gameObject);
            visual = null;
        }

        BuildingTypeSO buildingTypeSO = GridBuildingSystem.Instance.GetBuildingTypeSO();

        if (buildingTypeSO != null)
        {
            visual = Instantiate(buildingTypeSO.visual, Vector3.zero, Quaternion.identity);
            visual.parent = transform;
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
        }
    }
}
