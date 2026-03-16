using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 10f;
    [SerializeField] private RoadData RoadData1;
    [SerializeField] private RoadData RoadData2;
    [SerializeField] private RoadData RoadData3;
    [SerializeField] private RoadData RoadData4;
    [SerializeField] private RoadPreview roadPreviewPrefab;
    [SerializeField] private Road roadPrefab;
    [SerializeField] private BuildingGrid grid;
    [SerializeField] private BusPreview busPreviewPrefab;
    [SerializeField] private Bus busPrefab;
    private IPreview preview;
    private Road lastHovered;
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

            // Reset last hovered road if turning destroy mode off
            if (!destroy && lastHovered != null)
            {
                lastHovered.ChangeState(Road.RoadState.BUILT);
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
            preview = CreateRoadPreview(RoadData1, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            preview = CreateRoadPreview(RoadData2, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            preview = CreateRoadPreview(RoadData3, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            preview = CreateRoadPreview(RoadData4, mousePos);
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
        // Raycast to find road under mouse
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
        {
            Road hovered = hit.collider.GetComponentInParent<Road>();

            if (hovered != lastHovered)
            {
                // Reset previous hover
                if (lastHovered != null)
                    lastHovered.ChangeState(Road.RoadState.BUILT);

                // Set new hover
                if (hovered != null)
                    hovered.ChangeState(Road.RoadState.DESTROYHOVER);

                lastHovered = hovered;
            }

            // Left click destroys it
            if (hovered != null && Input.GetMouseButtonDown(0))
            {
                if (grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
                grid.RemRoad(hovered.transform.position); // remove from grid
                Destroy(hovered.gameObject);
                lastHovered = null; // clear hover since its gone
            }
        }
        else
        {
            // No road under cursor — reset lastHovered
            if (lastHovered != null)
            {
                lastHovered.ChangeState(Road.RoadState.BUILT);
                lastHovered = null;
            }
        }
    }

    
    private void PlaceRoad(Vector3 roadPosition)
    {
        if (preview is not RoadPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(roadPosition);
        Road road = Instantiate(roadPrefab, snappedPos, Quaternion.identity);
        road.Setup(((RoadPreview)preview).Data, ((RoadPreview)preview).RoadModel.Rotation);
        grid.SetRoad(road, snappedPos);
        Destroy(((RoadPreview)preview).gameObject);
        preview = null;
    }

    private void RemoveRoad(Vector3 mouseWorldPosition)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(mouseWorldPosition);
        grid.RemRoad(snappedPos);
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
        //Debug.Log("Coordinates: " + snappedX + ", " + snappedZ);
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
            RemoveRoad(mouseWorldPosition);
            return;
        }
        if (destroy)
        {
            // Raycast to find building under mouse
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
            {
                Road b = hit.collider.GetComponentInParent<Road>();
                if (b != null && !b.IsCityRoad)
                {
                    b.ChangeState(Road.RoadState.DESTROYHOVER);
                    if (Input.GetMouseButtonDown(0))
                    {

                        grid.RemRoad(b.transform.position);
                        Destroy(b.gameObject);
                    }
                }
            }
            return; // exit so preview doesn't move
        }
        if (preview is RoadPreview)
        {
            ((RoadPreview)preview).transform.position = mouseWorldPosition;
            Vector3 buildPosition = ((RoadPreview)preview).RoadModel.GetAllBuildingPositions().First(); //road is only 1 tile
            bool canBuild = grid.CanBuild(buildPosition);
            if (canBuild && !destroy)
            {
                ((RoadPreview)preview).transform.position = GetSnappedCenterPosition(buildPosition);
                ((RoadPreview)preview).ChangeState(RoadPreview.RoadPreviewState.POSITIVE);
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceRoad(buildPosition);
                }
            }
            else
            {
                ((RoadPreview)preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
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
                ((BusPreview)preview).ChangeState(RoadPreview.RoadPreviewState.POSITIVE);
                if ( Input.GetMouseButtonDown(0))
                {
                    PlaceBus(busPosition);
                }
            }
            else
            {
                ((BusPreview)preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                preview.Rotate(90);
            }
        }

    }

    private RoadPreview CreateRoadPreview(RoadData data, Vector3 position)
    {
        RoadPreview roadPreview = Instantiate(roadPreviewPrefab, position, Quaternion.identity);
        roadPreview.Setup(data);
        return roadPreview;
    }

    private BusPreview CreateBusPreview(BusData data, Vector3 position)
    {
        BusPreview busPreview = Instantiate(busPreviewPrefab, position, Quaternion.identity);
        busPreview.Setup(data);
        return busPreview;
    }
    #endregion
}

