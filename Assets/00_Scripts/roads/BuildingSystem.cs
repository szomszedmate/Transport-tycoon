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
    public IPreview Preview { get; private set; }
    private Road lastHovered;
    private bool destroy = false;


    private void Update()
    {

        
    }

    #region Buses
    [SerializeField]
    private BusData BusData1;

    private void PlaceBus(Vector3 busPosition)
    {
        if (Preview is not BusPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);
        bus.Setup(((BusPreview)Preview).Data, ((BusPreview)Preview).BusModel.Rotation);
        grid.SetVehicle(bus, snappedPos);
        Destroy(((BusPreview)Preview).gameObject);
        Preview = null;
        
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
        if (Preview is not RoadPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(roadPosition);
        Road road = Instantiate(roadPrefab, snappedPos, Quaternion.identity);
        road.Setup(((RoadPreview)Preview).Data, ((RoadPreview)Preview).RoadModel.Rotation);
        grid.SetRoad(road, snappedPos);
        Destroy(((RoadPreview)Preview).gameObject);
        Preview = null;
    }

    private void RemoveRoad(Vector3 mouseWorldPosition)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(mouseWorldPosition);
        grid.RemRoad(snappedPos);
    }
    #endregion

    #region Grid

    public Vector3 GetSnappedCenterPosition(Vector3 buildingPosition)
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

    public Vector3 GetMouseWorldPosition()
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
    public void HandlePreview(Vector3 mouseWorldPosition)
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
        if (Preview is RoadPreview)
        {
            ((RoadPreview)Preview).transform.position = mouseWorldPosition;
            Vector3 buildPosition = ((RoadPreview)Preview).RoadModel.GetAllBuildingPositions().First(); //road is only 1 tile
            bool canBuild = grid.CanBuild(buildPosition);
            if (canBuild && !destroy)
            {
                ((RoadPreview)Preview).transform.position = GetSnappedCenterPosition(buildPosition);
                ((RoadPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.POSITIVE);
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceRoad(buildPosition);
                }
            }
            else
            {
                ((RoadPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Preview.Rotate(90);
            }
        }
        else if (Preview is BusPreview)
        {
            ((BusPreview)Preview).transform.position = mouseWorldPosition;
            Vector3 busPosition = ((BusPreview)Preview).BusModel.GetBusPosition();
            bool canBuild = grid.CanBuildBus(busPosition);
            if (canBuild && !destroy)
            {
                ((BusPreview)Preview).transform.position = GetSnappedCenterPosition(busPosition);
                ((BusPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.POSITIVE);
                if ( Input.GetMouseButtonDown(0))
                {
                    PlaceBus(busPosition);
                }
            }
            else
            {
                ((BusPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Preview.Rotate(90);
            }
        }

    }

    public void destroyPreview()
    {
        Destroy(((MonoBehaviour)Preview).gameObject);
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

