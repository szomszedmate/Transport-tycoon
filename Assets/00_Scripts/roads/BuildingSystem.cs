using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 10f;
    [SerializeField] private BuildingData BuildingData1;
    [SerializeField] private BuildingData BuildingData2;
    [SerializeField] private BuildingData BuildingData3;
    [SerializeField] private BuildingData BuildingData4;
    [SerializeField] private BuildingPreview buildingPreviewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;
    [SerializeField] private BusPreview busPreviewPrefab;
    [SerializeField] private Bus busPrefab;
    private IPreview preview;
    private Building lastHovered;
    private bool destroy = false;


    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        // Right-click destroys preview
        if (Input.GetMouseButtonDown(1) && preview != null)
        {
            Destroy(((MonoBehaviour)preview).gameObject); // Works for buses and roads
            preview = null;
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            destroy = !destroy;

            // Reset last hovered building if turning destroy mode off
            if (!destroy && lastHovered != null)
            {
                lastHovered.ChangeState(Building.BuildingState.BUILT);
                lastHovered = null;
            }
        }

        if (destroy)
        {
            HandleDestroyMode();
            return;
        }

        if (preview != null)
        {
            HandlePreview(mousePos);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            preview = CreateRoadPreview(BuildingData1, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            preview = CreateRoadPreview(BuildingData2, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            preview = CreateRoadPreview(BuildingData3, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            preview = CreateRoadPreview(BuildingData4, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            preview = CreateBusPreview(BusData1, mousePos);
        }
    }

    #region Buses
    [SerializeField]
    private BusData BusData1;

    private void PlaceBus(Vector3 busPosition)
    {
        if (preview is not BusPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);
        bus.Setup(((BusPreview)preview).Data, ((BusPreview)preview).BusModel.Rotation);
        grid.SetVehicle(bus, snappedPos);
        Destroy(((BusPreview)preview).gameObject);
        preview = null;
        
    }
    #endregion

    #region Roads
    private void HandleDestroyMode()
    {
        // Raycast to find building under mouse
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
        {
            Building hovered = hit.collider.GetComponentInParent<Building>();

            if (hovered != lastHovered)
            {
                // Reset previous hover
                if (lastHovered != null)
                    lastHovered.ChangeState(Building.BuildingState.BUILT);

                // Set new hover
                if (hovered != null)
                    hovered.ChangeState(Building.BuildingState.DESTROYHOVER);

                lastHovered = hovered;
            }

            // Left click destroys it
            if (hovered != null && Input.GetMouseButtonDown(0))
            {
                if (grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
                grid.RemBuilding(hovered.transform.position); // remove from grid
                Destroy(hovered.gameObject);
                lastHovered = null; // clear hover since its gone
            }
        }
        else
        {
            // No building under cursor — reset lastHovered
            if (lastHovered != null)
            {
                lastHovered.ChangeState(Building.BuildingState.BUILT);
                lastHovered = null;
            }
        }
    }

    
    private void PlaceBuilding(Vector3 buildingPosition)
    {
        if (preview is not BuildingPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(buildingPosition);
        Building building = Instantiate(buildingPrefab, snappedPos, Quaternion.identity);
        building.Setup(((BuildingPreview)preview).Data, ((BuildingPreview)preview).BuildingModel.Rotation);
        grid.SetBuilding(building, snappedPos);
        Destroy(((BuildingPreview)preview).gameObject);
        preview = null;
    }

    private void RemoveBuilding(Vector3 mouseWorldPosition)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(mouseWorldPosition);
        grid.RemBuilding(snappedPos);
    }
    #endregion

    #region Grid

    private Vector3 GetSnappedCenterPosition(Vector3 buildingPosition)
    {
        //List<int> xs = allBuildingPositions.Select(p => Mathf.FloorToInt(p.x)).ToList();
        //List<int> zs = allBuildingPositions.Select(p => Mathf.FloorToInt(p.z)).ToList();
        //float centerX = (xs.Min() + xs.Max()) / 2f + CellSize / 2f;
        //float centerZ = (zs.Min() + zs.Max()) / 2f + CellSize / 2f;
        //return new Vector3(centerX, 0, centerZ);

        // Snap to nearest grid cell center
        float snappedX = Mathf.Floor(buildingPosition.x / CellSize) * CellSize + CellSize / 2f;
        float snappedZ = Mathf.Floor(buildingPosition.z / CellSize) * CellSize + CellSize / 2f;
        Debug.Log("Coordinates: " + snappedX + ", " + snappedZ);
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
    #endregion

    #region Preview
    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        if (destroy && Input.GetMouseButtonDown(0))
        {
            RemoveBuilding(mouseWorldPosition);
            return;
        }
        if (destroy)
        {
            // Raycast to find building under mouse
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
            {
                Building b = hit.collider.GetComponentInParent<Building>();
                if (b != null && !b.IsCityRoad)
                {
                    b.ChangeState(Building.BuildingState.DESTROYHOVER);
                    if (Input.GetMouseButtonDown(0))
                    {

                        grid.RemBuilding(b.transform.position);
                        Destroy(b.gameObject);
                    }
                }
            }
            return; // exit so preview doesn't move
        }
        if (preview is BuildingPreview)
        {
            ((BuildingPreview)preview).transform.position = mouseWorldPosition;
            Vector3 buildPosition = ((BuildingPreview)preview).BuildingModel.GetAllBuildingPositions().First(); //road is only 1 tile
            bool canBuild = grid.CanBuild(buildPosition);
            if (canBuild && !destroy)
            {
                ((BuildingPreview)preview).transform.position = GetSnappedCenterPosition(buildPosition);
                ((BuildingPreview)preview).ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceBuilding(buildPosition);
                }
            }
            else
            {
                ((BuildingPreview)preview).ChangeState(BuildingPreview.BuildingPreviewState.NEGATIVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                preview.Rotate(90);
            }
        }
        else if (preview is BusPreview)
        {
            ((BusPreview)preview).transform.position = mouseWorldPosition;
            Vector3 busPosition = ((BusPreview)preview).BusModel.GetBusPosition();
            bool canBuild = grid.CanBuildBus(busPosition);
            if (canBuild && !destroy)
            {
                ((BusPreview)preview).transform.position = GetSnappedCenterPosition(busPosition);
                ((BusPreview)preview).ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);
                if ( Input.GetMouseButtonDown(0))
                {
                    PlaceBus(busPosition);
                }
            }
            else
            {
                ((BusPreview)preview).ChangeState(BuildingPreview.BuildingPreviewState.NEGATIVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                preview.Rotate(90);
            }
        }

    }

    private BuildingPreview CreateRoadPreview(BuildingData data, Vector3 position)
    {
        BuildingPreview buildingPreview = Instantiate(buildingPreviewPrefab, position, Quaternion.identity);
        buildingPreview.Setup(data);
        return buildingPreview;
    }

    private BusPreview CreateBusPreview(BusData data, Vector3 position)
    {
        BusPreview busPreview = Instantiate(busPreviewPrefab, position, Quaternion.identity);
        busPreview.Setup(data);
        return busPreview;
    }
    #endregion
}

