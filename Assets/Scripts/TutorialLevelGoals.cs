using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class TutorialLevelGoals : MonoBehaviour
{
    private BuildingManager buildingManager;
    private int buildingCount;

    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GridBuildingSystem.Instance.BuildingManager;
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraSystem.DoCameraMovement)
        {
            // Key input for subgoals
            if (Input.GetKeyDown(KeyCode.W))
                AFactory.Invoke(this, "W", Matrix.Identity(1));
            if (Input.GetKeyDown(KeyCode.A))
                AFactory.Invoke(this, "A", Matrix.Identity(1));
            if (Input.GetKeyDown(KeyCode.S))
                AFactory.Invoke(this, "S", Matrix.Identity(1));
            if (Input.GetKeyDown(KeyCode.D))
                AFactory.Invoke(this, "D", Matrix.Identity(1));
        }

        // Building event for subgoals
        if (buildingCount < buildingManager.factories.Count)
            AFactory.Invoke(this, buildingManager.factories[buildingManager.factories.Count - 1].GetBuildingTypeSO().nameString, Matrix.Identity(1));
        buildingCount = buildingManager.factories.Count;
    }
}
