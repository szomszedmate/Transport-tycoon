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

    [SerializeField] private BusStopPreview busStopPreviewPrefab;
    [SerializeField] private BusStop busStopPrefab;
    public bool RoutePlanning { get; private set; } = false;
    private Road hovered;
    public IPreview Preview { get; private set; }
    private Vector3 lastSnappedPos;
    public BuildingGrid Grid { get => grid; private set => grid = value; } // for debug

    private Road lastHovered;
    private Bus lastBusSelected;
    private Bus busSelected;
    private Road lastRoadSelected;
    private Road roadSelected;
    public bool destroy = false;
    private bool doneRotating = false; // for pre rotating roads
    private bool canAfford;

    public event EventHandler prevdest;
    public event EventHandler destroymodeturn;
    public event EventHandler<int> selectprev;
    public event EventHandler<BuyRequestEventArgs> BuyRequest; 
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
        DestroyPreview();
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
            case 6:
                Preview = CreateBusStopPreview(BusStopData1, mousePos);
                break;
            default:
                break;
        }
        float price = Preview.Data.Cost;    // checking costs for preview material
        var args = new BuyRequestEventArgs { Cost = price, Deduct = false };
        BuyRequest?.Invoke(this, args);
        canAfford = args.IsApproved;
    }

    #region Buses
    [SerializeField]
    private BusData BusData1;

    private void PlaceBus(Vector3 busPosition)
    {
        if (Preview is not BusPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);

        var agent = bus.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        bus.Setup(((BusPreview)Preview).Data, ((BusPreview)Preview).BusModel.Rotation);

        Grid.SetVehicle(bus, snappedPos);
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
            busSelected.ChangeState(Bus.BusState.CONFIRMED);
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
        if (busSelected.RouteConfirmed) return; // TODO display message: route needs reset
        roadSelected = hit.collider.GetComponentInParent<Road>();
        if (roadSelected is null) return;

        if (lastRoadSelected is null) // Just add the first road
        {
            if (roadSelected.Road_HasBusStop()) // first road must have bus stop
            {
                if (!busSelected.AddToRoute(roadSelected)) return;
                roadSelected.ChangeState(Road.RoadState.SELECTED);
                lastRoadSelected = roadSelected;
            } else
            {
                return;
            }
        }
        Direction? direction = Grid.GetRelativeDirection(lastRoadSelected, roadSelected);
        if (direction == null) return;
        if (lastRoadSelected.IsConnectedTo(roadSelected, (Direction)direction))
        {
            busSelected.AddToRoute(roadSelected);
            roadSelected.ChangeState(Road.RoadState.SELECTED);

            lastRoadSelected = roadSelected;
        }
    }

    public void RemFromRoute(RaycastHit hit)
    {
        if (busSelected.RouteConfirmed) return; // TODO display message: route needs reset
        roadSelected = hit.collider.GetComponentInParent<Road>();

        if (roadSelected == null || busSelected.Route.Count == 0) return;
        if (roadSelected != busSelected.Route.Last()) return; // only remove the last road

        roadSelected.ChangeState(Road.RoadState.BUILT);
        busSelected.RemFromRoute(roadSelected);

        if (busSelected.Route.Count > 0)
        {
            lastRoadSelected = busSelected.Route.Last();
        }
        else
        {
            lastRoadSelected = null;
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
        if (!busSelected.HasRoute().Item2.Road_HasBusStop()) return; // route has to end with city road

        busSelected.ConfirmRoute();
    }
    #endregion

    #region Bus stops

    [SerializeField]
    private BusStopData BusStopData1;

    private void PlaceBusStop(Vector3 busPosition)
    {
        if (Preview is not BusStopPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        BusStop busStop = Instantiate(busStopPrefab, snappedPos, Quaternion.identity);

        var agent = busStop.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        busStop.SetUp(((BusStopPreview)Preview).Data, ((BusStopPreview)Preview).BusStopModel.Rotation);

        Grid.SetBusStop(busStop, snappedPos);
        Destroy(((BusStopPreview)Preview).gameObject);
        Preview = null;

    }

    #endregion


    #region Roads



    public void HandleDestroyMode()
    {
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
        if (Grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
        Grid.RemRoad(hovered.transform.position); // remove from grid
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
        Grid.SetRoad(road, snappedPos);
        // Destroy(((RoadPreview)preview).gameObject);
        //preview = null;
    }

    private void RemoveRoad(Vector3 mouseWorldPosition)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(mouseWorldPosition);
        Grid.RemRoad(snappedPos);
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

    private int RotateToMatch(RoadPreview roadPreview, Road otherRoad)
    {
        Direction? direction = grid.GetRelativePreviewDirection(roadPreview, otherRoad);
        if (direction == null) return -1;
        for (int rotation = 0; rotation < 4; rotation++)
        {
            if (grid.IsPreviewConnectedTo((RoadPreview)Preview, otherRoad, (Direction)direction))
            {
                return rotation;
            }
            roadPreview.Rotate(90);
        }
        return -1;
    }

    private void CheckRoadsForMatch(RoadPreview preview)
    {
        (int x, int y) = grid.WorldToGridPosition(preview.transform.position);
        for (int i = -1; i <= 1;  i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Mathf.Abs(i) == Mathf.Abs(j)) continue; // skip diagonals

                int newX = x + i;
                int newY = y + j;

                if (newX < 0 || newX >= grid.Width || newY < 0 || newY >= grid.Height) continue;
                if (!(grid.Grid[newX, newY].IsRoad())) continue; // continue if no road

                int rotation = RotateToMatch(preview, grid.Grid[newX, newY].Road);
                if (rotation != -1)
                {
                    
                    doneRotating = true;
                    break;
                }
            }
        }
        doneRotating = true;
    }

    public void RotatePreview(int rotation)
    {
        Preview.Rotate(rotation);
        doneRotating = true;
    }

    public void HandlePreview(Vector3 mouseWorldPosition, bool shouldIPlace)
    {
        if (!canAfford)
        {
            Preview.ChangeState(PreviewState.NEGATIVE);
        }
        if (Preview is RoadPreview roadPreview) // roads
        {
            roadPreview.transform.position = mouseWorldPosition;

            Vector3 buildPosition = roadPreview.RoadModel.GetAllBuildingPositions().First(); //road is only 1 tile
            Vector3 currentSnappedPos = GetSnappedCenterPosition(buildPosition); // for pre rotation

            if (currentSnappedPos != lastSnappedPos)
            {
                doneRotating = false;
                lastSnappedPos = currentSnappedPos;
            }

            roadPreview.transform.position = mouseWorldPosition;

            bool canBuild = Grid.CanBuild(buildPosition);
            if (canBuild && !destroy)
            {
                roadPreview.transform.position = currentSnappedPos;
                if (!doneRotating)
                {
                    CheckRoadsForMatch((RoadPreview)Preview);
                }

                if (canAfford)
                {
					roadPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)   // placing the road
                    {
                            PlaceRoad(buildPosition);

						    float price = Preview.Data.Cost;    // update affordability
						    var args = new BuyRequestEventArgs { Cost = price, Deduct = true };
						    BuyRequest?.Invoke(this, args);

						    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = false }; // check for next item
						    BuyRequest?.Invoke(this, checkNext);
						    canAfford = checkNext.IsApproved;
					}
				}
            }
            else
            {
                roadPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (Preview is BusPreview busPreview) // buses
        {
            busPreview.transform.position = mouseWorldPosition;
            Vector3 busPosition = busPreview.BusModel.GetBusPosition();
            bool canBuild = Grid.CanBuildBus(busPosition);
            if (canBuild && !destroy)
            {
                busPreview.transform.position = GetSnappedCenterPosition(busPosition);

                if (canAfford)
                {
                    busPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        float price = busPreview.Data.Cost;    // checking costs
						var args = new BuyRequestEventArgs { Cost = price, Deduct = true };
						BuyRequest?.Invoke(this, args);
                        PlaceBus(busPosition);
                    }
                }
            }
            else
            {
                busPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (Preview is BusStopPreview busStopPreview) // bus stops
        {
            busStopPreview.transform.position = mouseWorldPosition;
            Vector3 busStopPosition = busStopPreview.BusStopModel.GetBusStopPosition();
            //Debug.Log(Grid.WorldToGridPosition(busStopPosition));
            bool canBuild = Grid.CanBuildBusStop(busStopPosition);
            if (canBuild && !destroy)
            {
                busStopPreview.transform.position = GetSnappedCenterPosition(busStopPosition);
                busStopPreview.ChangeState(PreviewState.POSITIVE);
                if (shouldIPlace)
                {
                    PlaceBusStop(busStopPosition);
                }
            }
            else
            {
                busStopPreview.ChangeState(PreviewState.NEGATIVE);
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

    private BusStopPreview CreateBusStopPreview(BusStopData data, Vector3 position)
    {
        BusStopPreview busStopPreview = Instantiate(busStopPreviewPrefab, position, Quaternion.identity);
        busStopPreview.Setup(data);
        return busStopPreview;
    }
    #endregion
}

