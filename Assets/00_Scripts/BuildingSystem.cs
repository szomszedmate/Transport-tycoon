using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 10f;
    [SerializeField] private BuildingData BuildingData1;
    [SerializeField] private BuildingData BuildingData2;
    [SerializeField] private BuildingData BuildingData3;
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;
    private BuildingPreview preview;

    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        if (preview != null)
        {
            HandlePreview(mousePos);
        } else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                preview = CreatePreview(BuildingData1, mousePos);
            } else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                preview = CreatePreview(BuildingData2, mousePos);
            } else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                preview = CreatePreview(BuildingData3, mousePos);
            }
        }
    }

    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPositions();
        bool canBuild = grid.CanBuild(buildPosition);
        if ( canBuild)
        {
            preview.transform.position = GetSnappedCenterPosition(buildPosition);
            preview.ChangeState(BuildingPreview.BuildingPrevioewState.POSITIVE);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(buildPosition);
            }
        }
        else
        {
            preview.ChangeState(BuildingPreview.BuildingPrevioewState.NEGATIVE);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90);
        }
    }

    private void PlaceBuilding(List<Vector3> buildingPositions)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);
        building.Setup(preview.Data, preview.BuildingModel.Rotation);
        grid.SetBuilding(building, buildingPositions);
        Destroy(preview.gameObject);
        preview = null;
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPositions)
    {
        //List<int> xs = allBuildingPositions.Select(p => Mathf.FloorToInt(p.x)).ToList();
        //List<int> zs = allBuildingPositions.Select(p => Mathf.FloorToInt(p.z)).ToList();
        //float centerX = (xs.Min() + xs.Max()) / 2f + CellSize / 2f;
        //float centerZ = (zs.Min() + zs.Max()) / 2f + CellSize / 2f;
        //return new Vector3(centerX, 0, centerZ);

        // Compute raw center of building
        float rawX = allBuildingPositions.Average(p => p.x);
        float rawZ = allBuildingPositions.Average(p => p.z);

        // Snap to nearest grid cell center
        float snappedX = Mathf.Floor(rawX / CellSize) * CellSize + CellSize / 2f;
        float snappedZ = Mathf.Floor(rawZ / CellSize) * CellSize + CellSize / 2f;

        return new Vector3(snappedX, 0, snappedZ);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
    {
        BuildingPreview buildingPreview = Instantiate(previewPrefab, position, Quaternion.identity);
        buildingPreview.Setup(data);
        return buildingPreview;
    }
}

