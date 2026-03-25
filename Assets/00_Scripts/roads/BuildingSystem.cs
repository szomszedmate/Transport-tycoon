using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
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
    public bool RoutePlanning { get; private set; } = false;
    private Road hovered;
    public IPreview Preview { get; private set; }
    private Road lastHovered;
    private Bus lastBusSelected;
    private Bus busSelected;
    private Road lastRoadSelected;
    private Road roadSelected;
    public bool destroy = false;

    public event EventHandler prevdest;
    public event EventHandler destroymodeturn;
    public event EventHandler<int> selectprev;
    private void Update()
    {
        if (destroy)
        {
            HandleDestroyMode();
            return;
        }
    }

    public void DestroyMode()
    {
        destroy = !destroy;
        destroymodeturn?.Invoke(this, EventArgs.Empty);
        if (!destroy)
        {
            DestroyModeTurnOff();
        }
        else
        {
            prevdest?.Invoke(this, EventArgs.Empty);
            DestroyPreview();
        }

    }

    public void DestroyModeTurnOff()
    {

        // Reset last hovered road if turning destroy mode off
        if (!destroy && lastHovered != null)
        {
            lastHovered.ChangeState(Road.RoadState.BUILT);
            lastHovered = null;

        }
    }
    public void DestroyPreview()
    {
        if (destroy && Preview == null)
        {
            //destroy = false;
            //destroymodeturn?.Invoke(this, EventArgs.Empty);
            //DestroyModeTurnOff();
        }
        else if (Preview != null)
        {
            // If we have a preview, just kill the preview and leave destroy mode alone
            Destroy(((MonoBehaviour)Preview).gameObject);
            Preview = null;
            prevdest?.Invoke(this, EventArgs.Empty);
        }

    }
    public void CreatePreview(int type)
    {
        Vector3 mousePos = GetMouseWorldPosition();
        selectprev?.Invoke(this, type);
        switch (type)
        {
            case 1:
                Preview = CreateRoadPreview(RoadData1, mousePos);
                break;
            case 2:
                Preview = CreateRoadPreview(RoadData2, mousePos);
                break;
            case 3:
                Preview = CreateRoadPreview(RoadData3, mousePos);
                break;
            case 4:
                Preview = CreateRoadPreview(RoadData4, mousePos);
                break;
            case 5:
                Preview = CreateBusPreview(BusData1, mousePos);
                break;

            default:
                break;
        }
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

    public void SelectBusForPlanning(RaycastHit hit)
    {
        Bus hitBus = hit.collider.GetComponentInParent<Bus>();

        // 1. CLEANUP: If we are switching away or clicking away, reset the OLD bus visuals
        if (lastBusSelected != null && hitBus != lastBusSelected)
        {
            foreach (Road road in lastBusSelected.Route)
            {
                road.ChangeState(Road.RoadState.BUILT);
            }
            lastBusSelected.ChangeState(Bus.BusState.BUILT);
            roadSelected = null;
            lastRoadSelected = null;
        }

        // 2. TOGGLE/OFF: If we hit nothing OR hit the same bus again, turn planning OFF
        if (hitBus == null)
        {
            if (lastBusSelected != null)
            {
                // Only reset the actual route data if that's your intended "Cancel" behavior
                // lastBusSelected.ResetRoute(); 
                lastBusSelected.ChangeState(Bus.BusState.BUILT);
            }

            lastBusSelected = null;
            busSelected = null;
            RoutePlanning = false;
            return; // Exit early, we are done
        }

        // 3. SELECTION: If we hit a NEW bus
        busSelected = hitBus;
        lastBusSelected = busSelected;

        busSelected.ChangeState(Bus.BusState.SELECTHOVER);
        RoutePlanning = true;
        lastRoadSelected = hitBus.HasRoute().Item2;

        // Highlight the bus's existing route so the player knows where it goes
        if (busSelected.RouteConfirmed)
        {
            foreach (Road road in busSelected.Route)
            {
                road.ChangeState(Road.RoadState.CONFIRMED);
            }
        } else
        {
            foreach (Road road in busSelected.Route)
            {
                road.ChangeState(Road.RoadState.SELECTED);
            }
        }
    }

    public void AddToRoute(RaycastHit hit)
    {
        roadSelected = hit.collider.GetComponentInParent<Road>();
        if (roadSelected is null) return;

        if (lastRoadSelected is null && roadSelected.IsCityRoad) // Just add the first road if city road
        {
            if (!busSelected.AddToRoute(roadSelected)) return;
            roadSelected.ChangeState(Road.RoadState.SELECTED);
            lastRoadSelected = roadSelected;
        }
        Direction? direction = grid.GetRelativeDirection(lastRoadSelected, roadSelected);
        if (direction == null) return;
        if (lastRoadSelected.IsConnectedTo(roadSelected, (Direction)direction))
        {
            busSelected.AddToRoute(roadSelected);
            roadSelected.ChangeState(Road.RoadState.SELECTED);

            lastRoadSelected = roadSelected;
        }
    }

    public void ResetRoute()
    {
        if (lastBusSelected != null)
        {
            lastBusSelected.ChangeState(Bus.BusState.BUILT);
        }
        foreach (Road road in lastBusSelected.Route)
        {
            road.ChangeState(Road.RoadState.BUILT);
        }

        lastBusSelected.ResetRoute();
        lastBusSelected = null;
        busSelected = null;
        roadSelected = null;
        lastRoadSelected = null;
        RoutePlanning = false;
        return; // Exit early, we are done
    }

    public void BS_ConfirmRoute()
    {
        if (busSelected == null) return;
        if (!busSelected.HasRoute().Item2.IsCityRoad) return; // route has to end with city road

        busSelected.ConfirmRoute();
    }
    #endregion

    #region Roads



    public void HandleDestroyMode()
    {
        //Debug.Log("Destroying");
        // Raycast to find road under mouse
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
        {
            hovered = hit.collider.GetComponentInParent<Road>();

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
            //if (hovered != null && Input.GetMouseButtonDown(0))
            //{
            //    if (grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
            //    grid.RemRoad(hovered.transform.position); // remove from grid
            //    Destroy(hovered.gameObject);
            //    lastHovered = null; // clear hover since its gone
            //}
        }
        else
        {
            // No road under cursor  reset lastHovered
            if (lastHovered != null)
            {
                lastHovered.ChangeState(Road.RoadState.BUILT);
                lastHovered = null;
            }
        }
    }

    public void Destroy()
    {
        if (Preview != null) return;
        if (grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
        grid.RemRoad(hovered.transform.position); // remove from grid
        Destroy(hovered.gameObject);
        lastHovered = null; // clear hover since its gone
    }

    private void PlaceRoad(Vector3 roadPosition)
    {
        if (Preview is not RoadPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(roadPosition);

        //if (grid.HasTrees(snappedPos))
        //{
        //    grid.ClearTrees(snappedPos);
        //}
        Road road = Instantiate(roadPrefab, snappedPos, Quaternion.identity);
        road.Setup(((RoadPreview)Preview).Data, ((RoadPreview)Preview).RoadModel.Rotation);
        grid.SetRoad(road, snappedPos);
        // Destroy(((RoadPreview)preview).gameObject);
        //preview = null;
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

    public Vector3 GetMouseWorldPosition()
    {
        if (Mouse.current == null) return Vector3.zero; // Check if the mouse actually exists and is on screen

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Plane groundPlane = new(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
    #endregion

    #region Preview
    public void HandlePreview(Vector3 mouseWorldPosition, bool shouldIPlace)
    {
        //if (destroy && Input.GetMouseButtonDown(0))
        //{
        //    RemoveRoad(mouseWorldPosition);
        //    return;
        //}
        //if (destroy)
        //{
        //    // Raycast to find building under mouse
        //    if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
        //    {
        //        Road b = hit.collider.GetComponentInParent<Road>();
        //        if (b != null && !b.IsCityRoad)
        //        {
        //            b.ChangeState(Road.RoadState.DESTROYHOVER);
        //            if (Input.GetMouseButtonDown(0))
        //            {

        //                grid.RemRoad(b.transform.position);
        //                Destroy(b.gameObject);
        //            }
        //        }
        //    }
        //    return; // exit so preview doesn't move
        //}
        if (Preview is RoadPreview)
        {
            ((RoadPreview)Preview).transform.position = mouseWorldPosition;
            Vector3 buildPosition = ((RoadPreview)Preview).RoadModel.GetAllBuildingPositions().First(); //road is only 1 tile
            bool canBuild = grid.CanBuild(buildPosition);
            if (canBuild && !destroy)
            {
                ((RoadPreview)Preview).transform.position = GetSnappedCenterPosition(buildPosition);
                ((RoadPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.POSITIVE);
                if (shouldIPlace)
                {
                    PlaceRoad(buildPosition);
                }
            }
            else
            {
                ((RoadPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
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
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceBus(busPosition);
                }
            }
            else
            {
                ((BusPreview)Preview).ChangeState(RoadPreview.RoadPreviewState.NEGATIVE);
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

