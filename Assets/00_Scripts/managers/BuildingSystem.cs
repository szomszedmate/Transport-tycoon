using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Bus;
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

    [SerializeField] private TruckPreview truckPreviewPrefab;
    [SerializeField] private Truck truckPrefab;

    [SerializeField] private BusPreview busPreviewPrefab;
    [SerializeField] private Bus busPrefab;

    [SerializeField] private BusStopPreview busStopPreviewPrefab;
    [SerializeField] private BusStop busStopPrefab;
    [SerializeField] private BusStopData BusStopData;
    public bool RoutePlanning { get; private set; } = false;
    private Road hovered;
    public IPreview Preview { get; set; }
    private Vector3 lastSnappedPos;
    public BuildingGrid Grid { get => grid; set => grid = value; }
    public RoadPreview RoadPreviewPrefab { get => roadPreviewPrefab; set => roadPreviewPrefab = value; }
    public Road RoadPrefab { get => roadPrefab; set => roadPrefab = value; }

    private Road lastHovered;
    private VehicleBase lastVehicleSelected;
    private VehicleBase vehicleSelected;
    private Road lastRoadSelected;
    private Road roadSelected;
    public bool destroy = false;
    private bool doneRotating = false; // for pre rotating roads
    private bool canAfford;
    Vector2 mousePosition;

    public event EventHandler prevdest;
    public event EventHandler destroymodeturn;
    public event EventHandler<IData> selectprev;
    public event EventHandler<BuyRequestEventArgs> BuyRequest; 
    public event VehicleBase.MileageChangedEventHandler AnyBusMileageChanged;
    public void InputUpdate(Vector2 mousePosition, bool leftClicked, bool rightClicked, bool leftHeld, bool rightHeld)
    {
        this.mousePosition = mousePosition;
        Vector3 worldPos = GetMouseWorldPosition();

        if (destroy)
        {
            HandleDestroyMode(leftClicked);
        }
        else if (Preview != null)
        {
            HandlePreview(worldPos, leftClicked, rightClicked);
        }
        else if (RoutePlanning)
        {
            if (leftHeld)
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit routeHit))
                {
                    AddToRoute(routeHit);
                }
            }
            else if (rightHeld)
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit routeHit))
                {
                    RemFromRoute(routeHit);
                }
            }
        }
    }

    public void DestroyMode()
    {
        destroy = !destroy;
        if (destroy)
        {
            if (Preview != null)
            {
                DestroyPreview();
            }
        }
        else
        {
            DestroyModeTurnOff();
        }
        destroymodeturn?.Invoke(this, EventArgs.Empty);
    }

    public void DestroyModeTurnOff()
    {

        // Reset last hovered road if turning destroy mode off
        if (!destroy && lastHovered != null)
        {
            lastHovered.ChangeState(RoadState.BUILT);
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
    public void CreatePreview(IData build)
    {
        if (destroy)
        {
            destroy = false;
            destroymodeturn.Invoke(this, EventArgs.Empty);
        }

        DestroyPreview();
        Vector3 worldPos = GetMouseWorldPosition();
        selectprev?.Invoke(this, build);

        if (build is RoadData roadData)
        {
            Preview = CreateRoadPreview(roadData, mousePosition);
        }
        else if (build is BusData busData)
        {
            Preview = CreateBusPreview(busData, mousePosition);
        }
        else if (build is TruckData truckData)
        {
            Preview = CreateTruckPreview(truckData, mousePosition);
        }
        else if (build is BusStopData busStopData)
        {
            Preview = CreateBusStopPreview(busStopData, worldPos);
        }
        else return;
        float price = Preview.Data.Cost;    // checking costs for preview material
        var args = new BuyRequestEventArgs { Cost = price, Deduct = false };
        BuyRequest?.Invoke(this, args);
        canAfford = args.IsApproved;
    }

    #region Buses

    public event EventHandler<VehicleBase> vehiclePlaced;

    private void PlaceBus(Vector3 busPosition)
    {
        if (Preview is not BusPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);

        var agent = bus.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        bus.Setup((BusData)((BusPreview)Preview).Data, ((BusPreview)Preview).Model.Rotation);
        bus.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);
        Grid.SetVehicle(bus, snappedPos);
        Destroy(((MonoBehaviour)Preview).gameObject);
        vehiclePlaced?.Invoke(this,bus);
        Preview = null;

    }

    private void PlaceTruck(Vector3 truckPosition)
    {
        if (Preview is not TruckPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(truckPosition);
        Truck truck = Instantiate(truckPrefab, snappedPos, Quaternion.identity);

        var agent = truck.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        truck.Setup((TruckData)((TruckPreview)Preview).Data, ((TruckPreview)Preview).Model.Rotation);
        truck.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);
        Grid.SetVehicle(truck, snappedPos);
        Destroy(((MonoBehaviour)Preview).gameObject);
        vehiclePlaced?.Invoke(this, truck);
        Preview = null;
    } 

    public void SelectVehicleForPlanning(RaycastHit hit)
    {
        VehicleBase hitVehicle = hit.collider.GetComponentInParent<VehicleBase>();

        // 1. CLEANUP: If we are switching away or clicking away, reset the OLD bus visuals
        if (lastVehicleSelected != null && hitVehicle != lastVehicleSelected)
        {
            foreach (Road road in lastVehicleSelected.Route)
            {
                road.ChangeState(RoadState.BUILT);
            }
            lastVehicleSelected.ChangeState(VehicleBase.VehicleState.BUILT);
            roadSelected = null;
            lastRoadSelected = null;
        }

        // 2. TOGGLE/OFF: If we hit nothing OR hit the same bus again, turn planning OFF
        if (hitVehicle == null)
        {
            if (lastVehicleSelected != null)
            {
                // Only reset the actual route data if that's your intended "Cancel" behavior
                // lastVehicleSelected.ResetRoute(); 
                lastVehicleSelected.ChangeState(VehicleBase.VehicleState.BUILT);
            }

            lastVehicleSelected = null;
            vehicleSelected = null;
            RoutePlanning = false;
            return; // Exit early, we are done
        }

        // 3. SELECTION: If we hit a NEW bus
        vehicleSelected = hitVehicle;
        lastVehicleSelected = vehicleSelected;

        vehicleSelected.ChangeState(VehicleBase.VehicleState.SELECTHOVER);
        RoutePlanning = true;
        lastRoadSelected = hitVehicle.GetLast();

        // Highlight the bus's existing route so the player knows where it goes
        if (vehicleSelected.RouteConfirmed)
        {
            vehicleSelected.ChangeState(VehicleBase.VehicleState.CONFIRMED);
            foreach (Road road in vehicleSelected.Route)
            {
                road.ChangeState(RoadState.CONFIRMED);
            }
        } else
        {
            foreach (Road road in vehicleSelected.Route)
            {
                road.ChangeState(RoadState.SELECTED);
            }
        }
    }

    public void AddToRoute(RaycastHit hit)
    {
        if (vehicleSelected.RouteConfirmed) return; // TODO display message: route needs reset
        roadSelected = hit.collider.GetComponentInParent<Road>();
        if (roadSelected is null) return;

        if (lastRoadSelected is null) // Just add the first road
        {
            if (roadSelected.Road_HasBusStop()) // first road must have bus stop
            {
                if (!vehicleSelected.AddToRoute(roadSelected)) return;
                roadSelected.ChangeState(RoadState.SELECTED);
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
            vehicleSelected.AddToRoute(roadSelected);
            roadSelected.ChangeState(RoadState.SELECTED);

            lastRoadSelected = roadSelected;
        }
    }

    public void RemFromRoute(RaycastHit hit)
    {
        if (vehicleSelected.RouteConfirmed) return; // TODO display message: route needs reset
        roadSelected = hit.collider.GetComponentInParent<Road>();

        if (roadSelected == null || vehicleSelected.Route.Count == 0) return;
        if (roadSelected != vehicleSelected.Route.Last()) return; // only remove the last road

        roadSelected.ChangeState(RoadState.BUILT);
        vehicleSelected.RemFromRoute(roadSelected);

        if (vehicleSelected.Route.Count > 0)
        {
            lastRoadSelected = vehicleSelected.Route.Last();
        }
        else
        {
            lastRoadSelected = null;
        }
    }

    public void ResetRoute()
    {
        if (lastVehicleSelected != null)
        {
            lastVehicleSelected.ChangeState(VehicleBase.VehicleState.BUILT);
        }
        foreach (Road road in lastVehicleSelected.Route)
        {
            road.ChangeState(RoadState.BUILT);
        }

        lastVehicleSelected.ResetRoute();
        lastVehicleSelected = null;
        vehicleSelected = null;
        roadSelected = null;
        lastRoadSelected = null;
        RoutePlanning = false;
        return; // Exit early, we are done
    }

    public void BS_ConfirmRoute()
    {
        if (vehicleSelected == null || vehicleSelected.Route.Count() <= 1) return;

        Road fst = vehicleSelected.Route.First();
        Road lst = vehicleSelected.Route.Last();
        Direction? direction = Grid.GetRelativeDirection(fst, lst);
        if (vehicleSelected is Bus busSelected)
        {
            if (direction is not null && fst.IsConnectedTo(lst, (Direction)direction))
            {
                busSelected.ConfirmRoute(false);
            } else
            {
                busSelected.ConfirmRoute(true);
            }
        }
        else if (vehicleSelected is Truck truckSeleced)
        {
            truckSeleced.ConfirmRoute();
        }
    }
    #endregion

    #region Bus stops
    private void PlaceBusStop(Vector3 busPosition, StopType type)
    {
        if (Preview is not BusStopPreview) return;

        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        ILocation foundLocation = Grid.GetLocationAt(snappedPos);

        BusStop busStop = Instantiate(busStopPrefab, snappedPos, Quaternion.identity);
        busStop.Type = type;

        var agent = busStop.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        busStop.SetUp(((BusStopPreview)Preview).Data, ((BusStopPreview)Preview).BusStopModel.Rotation, foundLocation);

        Grid.SetBusStop(busStop, snappedPos);
        Destroy(((BusStopPreview)Preview).gameObject);
        Preview = null;

    }

    #endregion


    #region Roads



    public void HandleDestroyMode(bool shouldIDestroy)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // Raycast to find road under mouse
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hovered = hit.collider.GetComponentInParent<Road>();

            if (hovered != lastHovered)
            {
                // Reset previous hover
                if (lastHovered != null)
                    lastHovered.ChangeState(RoadState.BUILT);

                // Set new hover
                if (hovered != null)
                    hovered.ChangeState(RoadState.DESTROYHOVER);

                lastHovered = hovered;
            }
            if (shouldIDestroy) Destroy();
        }
        else
        {
            // No road under cursor  reset lastHovered
            if (lastHovered != null)
            {
                lastHovered.ChangeState(RoadState.BUILT);
                lastHovered = null;
            }
        }
    }

    public void Destroy()
    {
        if (Preview != null) return;
        if (hovered is null) return;
        if (Grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads
        Grid.RemRoad(hovered.transform.position); // remove from grid
        Destroy(hovered.gameObject);
        lastHovered = null; // clear hover since its gone
    }

    public void PlaceRoad(Vector3 roadPosition)
    {
        if (Preview is not RoadPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(roadPosition);

        //if (grid.HasTrees(snappedPos))
        //{
        //    grid.ClearTrees(snappedPos);
        //}
        Road road = Instantiate(RoadPrefab, snappedPos, Quaternion.identity);

        TerrainShaper shaper = FindFirstObjectByType<TerrainShaper>();
        if (shaper != null)
        {
            shaper.FlattenTerrainUnderRoad(snappedPos);
        }

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
        if (mousePosition == null) return Vector3.zero; // Check if the mouse actually exists and is on screen

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
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


    public event EventHandler vehiclePlacecancelled;
    public void HandlePreview(Vector3 mouseWorldPosition, bool shouldIPlace, bool shouldIDestroy)
    {
        if (shouldIDestroy)
        {
            DestroyPreview();
            vehiclePlacecancelled?.Invoke(this,EventArgs.Empty);
            return;
        }
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

            bool canBuild = roadPreview.Data.Kind == RoadKind.Bridge ? Grid.CanBuildBridge(buildPosition, roadPreview.Data, roadPreview.RoadModel.Rotation) : Grid.CanBuildRoad(buildPosition);
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

						    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
						    BuyRequest?.Invoke(this, checkNext);
						    canAfford = checkNext.IsApproved;
					}
				} else if (shouldIPlace)
                {
                    float price = Preview.Data.Cost;    // update affordability

                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
                    BuyRequest?.Invoke(this, checkNext);
                    canAfford = checkNext.IsApproved;
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
            Vector3 busPosition = busPreview.Model.GetPosition();
            bool canBuild = Grid.CanBuildBus(busPosition);
            if (canBuild && !destroy)
            {
                busPreview.transform.position = GetSnappedCenterPosition(busPosition);

                if (canAfford)
                {
                    busPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        PlaceBus(busPosition);

                       /* float price = busPreview.Data.Cost;    // checking costs
						var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
						BuyRequest?.Invoke(this, checkNext);
                        canAfford = checkNext.IsApproved;*/
                    }
                }
                else if (shouldIPlace)
                {
                    float price = Preview.Data.Cost;    // update affordability

                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
                    BuyRequest?.Invoke(this, checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                busPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (Preview is TruckPreview truckPreview)
        {
            truckPreview.transform.position = mouseWorldPosition;
            Vector3 truckPosition = truckPreview.Model.GetPosition();
            bool canBuild = Grid.CanBuildBus(truckPosition);
            if (canBuild && !destroy)
            {
                truckPreview.transform.position = GetSnappedCenterPosition(truckPosition);

                if (canAfford)
                {
                    truckPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        PlaceTruck(truckPosition);
                    }
                }
                else if (shouldIPlace)
                {
                    float price = Preview.Data.Cost;    // update affordability

                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
                    BuyRequest?.Invoke(this, checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                truckPreview.ChangeState(PreviewState.NEGATIVE);
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
                StopType stopType = Grid.GetStopType(busStopPosition);
                busStopPreview.transform.position = GetSnappedCenterPosition(busStopPosition);

                if (canAfford)
                {

                    busStopPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        PlaceBusStop(busStopPosition, stopType);

                        float price = busStopPreview.Data.Cost;    // checking costs
                        BuyRequestEventArgs checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                        BuyRequest?.Invoke(this, checkNext);
                        canAfford = checkNext.IsApproved;
                    }
                }
                else if (shouldIPlace)
                {
                    float price = Preview.Data.Cost;    // update affordability

                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
                    BuyRequest?.Invoke(this, checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                busStopPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }

    }

    public void invokeBuying(BuyRequestEventArgs checkNext)
    {
        BuyRequest?.Invoke(this, checkNext);
    }
    private RoadPreview CreateRoadPreview(RoadData data, Vector3 position)
    {
        RoadPreview roadPreview = Instantiate(RoadPreviewPrefab, position, Quaternion.identity);
        roadPreview.Setup(data);
        return roadPreview;
    }

    private BusPreview CreateBusPreview(BusData data, Vector3 position)
    {
        BusPreview busPreview = Instantiate(busPreviewPrefab, position, Quaternion.identity);
        busPreview.Setup(data);
        return busPreview;
    }

    private TruckPreview CreateTruckPreview(TruckData data, Vector3 position)
    {
        TruckPreview truckPreview = Instantiate(truckPreviewPrefab, position, Quaternion.identity);
        truckPreview.Setup(data);
        return truckPreview;
    }

    private BusStopPreview CreateBusStopPreview(BusStopData data, Vector3 position)
    {
        BusStopPreview busStopPreview = Instantiate(busStopPreviewPrefab, position, Quaternion.identity);
        busStopPreview.Setup(data);
        return busStopPreview;
    }
    #endregion
}

