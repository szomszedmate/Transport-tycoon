using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Bus;
using static UnityEngine.Rendering.DebugUI.Table;
public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 10f;
    [Header("Terrain")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask invisibleButPhysicalLayer;
    [SerializeField] private LayerMask vehicleLayer;
    [SerializeField] private TerrainPainter painter;
    [SerializeField] private int builtLayerIndex = 4;
    [SerializeField] private int positiveIndex = 5;
    [SerializeField] private int negativeIndex = 6;
    [SerializeField] private int destroyHoverIndex = 7;
    [SerializeField] private int selectedIndex = 8;
    [SerializeField] private int confirmedIndex = 9;
    [SerializeField] private List<int> mainLayers;
    [SerializeField] private GameObject bridgeEffect;
    private bool justPlaced = false;

    [Header("Roads")]
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

    [SerializeField] private Cat catPrefab;
    [SerializeField] private CatData catData;
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
    public event EventHandler<CancelChargeEventArgs> CancelCharge;
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
                if (Physics.Raycast(ray, out RaycastHit routeHit, 1000f, terrainLayer))
                {
                    AddToRoute(routeHit);
                }
            }
            else if (rightHeld)
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit routeHit, 1000f, terrainLayer))
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
            if (!Grid.IsRoad(lastSnappedPos) && (Preview is RoadPreview rp))
            {
                painter.ResetLayer(lastSnappedPos, mainLayers, rp.RoadModel.RoadType, rp.RoadModel.Rotation);
            } 
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
            ((RoadPreview)Preview).PreviewStateChanged += BuildingSystem_PreviewStateChanged;
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
        else if (build is CatData catData)
        {
            Preview = CreateCatPreview(catData, mousePosition);
        }
        else return;
        float price = Preview.Data.Cost;    // checking costs for preview material
        var args = new BuyRequestEventArgs { Cost = price, Deduct = false };
        BuyRequest?.Invoke(this, args);
        canAfford = args.IsApproved;
    }

    private void BuildingSystem_PreviewStateChanged(object sender, PreviewStateChangedEventArgs e)
    {
        int index = negativeIndex;
        int lastIndex = negativeIndex;
        switch (e.NewState)
        {
            case PreviewState.NEGATIVE:
                index = negativeIndex;
                lastIndex = positiveIndex;
                break;
            case PreviewState.POSITIVE:
                index = positiveIndex;
                lastIndex = negativeIndex;
                break;
            default:
                break;
        }
        painter.PaintRoadAt(GetSnappedCenterPosition(e.SnappedPosition), e.RoadType, e.Rotation, index, mainLayers, true, lastIndex);
    }

    #region Buses

    public event EventHandler<VehicleBase> vehiclePlaced;

    private void PlaceBus(Vector3 busPosition)
    {
        if (Preview is not BusPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);
        bus.CancelCharge += Bus_CancelCharge;

        var agent = bus.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null)
        {
            agent.enabled = false;
            agent.Warp(snappedPos);
        }
        bus.Setup((BusData)((BusPreview)Preview).Data, ((BusPreview)Preview).Model.Rotation, terrainLayer, grid);
        bus.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);

        Vector3 surfaceLocation = FindSurfaceAt(busPosition);
        //busstop.model.adjustvisualtoground(surfacelocation, offset, rot); todo

        Grid.SetVehicle(bus, snappedPos);
        Destroy(((MonoBehaviour)Preview).gameObject);
        vehiclePlaced?.Invoke(this,bus);
        Preview = null;
        VehicleSpawnAnimation.Play(bus, snappedPos);

        /*
          
        if (Preview is not BusStopPreview) return;
        
        Vector3 snappedPos = GetSnappedCenterPosition(busPosition);
        ILocation foundLocation = Grid.GetLocationAt(snappedPos);
        //Debug.Log("Surface: " + surfaceLocation + ", snapped:" + snappedPos);
        BusStop busStop = Instantiate(busStopPrefab, snappedPos, Quaternion.identity);
        busStop.Type = type;

        var agent = busStop.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        busStop.SetUp(((BusStopPreview)Preview).Data, ((BusStopPreview)Preview).BusStopModel.Rotation, foundLocation);

        (Vector3 offset, Quaternion rot) = FindBSVisualOffset(snappedPos, foundLocation);
        Vector3 surfaceLocation = FindSurfaceAt(busPosition + offset);
        Debug.Log($"Snapped: {snappedPos}, corrected: {surfaceLocation}");
        busStop.model.AdjustVisualToGround(surfaceLocation, offset, rot);
        Grid.SetBusStop(busStop, snappedPos);
        Destroy(((BusStopPreview)Preview).gameObject);
        Preview = null;

        */
    }

    private void Bus_CancelCharge(object sender, CancelChargeEventArgs e)
    {
        CancelCharge?.Invoke(this, e);
    }

    private void PlaceTruck(Vector3 truckPosition)
    {
        if (Preview is not TruckPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(truckPosition);
        Truck truck = Instantiate(truckPrefab, snappedPos, Quaternion.identity);

        var agent = truck.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        truck.Setup((TruckData)((TruckPreview)Preview).Data, ((TruckPreview)Preview).Model.Rotation, terrainLayer, grid);
        truck.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);
        Grid.SetVehicle(truck, snappedPos);
        Destroy(((MonoBehaviour)Preview).gameObject);
        vehiclePlaced?.Invoke(this, truck);
        Preview = null;
        VehicleSpawnAnimation.Play(truck, snappedPos);
    }


    public void SelectVehicleByHit(RaycastHit hit)
    {
        VehicleVisual visual = hit.collider.GetComponentInParent<VehicleVisual>(); // megkeressuk a vizualis reszt amire kattintottunk

        if (visual != null)
        {
            Debug.Log("Visual hit");
            Ray ray = new Ray(visual.transform.position, Vector3.up);

            if (Physics.Raycast(ray, out RaycastHit upHit, 1000f, vehicleLayer))
            {
                VehicleBase baseVehicle = upHit.collider.GetComponentInParent<VehicleBase>();

                if (baseVehicle != null)
                {
                    OpenVehicleInfo(baseVehicle);
                }
            }
        } else
        {
            OpenVehicleInfo(null); // null kikapcsolja
        }
    }


    //DEBUG
    public GameObject uiitem;
    public GameObject content;
    public GameObject panel;
    public void OpenVehicleInfo(VehicleBase hitVehicle)
    {
        if (hitVehicle is null)
        {
            DeselectRoute(hitVehicle);
            return;
        }
        VehicleData data = hitVehicle.data;
        panel.SetActive(true);
        GameObject itemGo=Instantiate(uiitem, content.transform, false);
        UIitem uiItem = itemGo.GetComponent<UIitem>();
        uiItem.vehicle = data;

        uiItem.routebuton.SetActive(true);
        uiItem.isplaced = true;
        uiItem.vehicleObject = hitVehicle;
        uiItem.placebuttontext.text = "Remove";
    }

    public void DeselectRoute(VehicleBase hitVehicle)
    {
        if (hitVehicle == null)
        {
            if (lastVehicleSelected != null)
            {
                // lastVehicleSelected.ResetRoute(); 
                lastVehicleSelected.ChangeState(VehicleBase.VehicleState.BUILT);
                foreach(Road road in lastVehicleSelected.Route)
                {
                    road.ChangeState(RoadState.BUILT);
                }
            }

            lastVehicleSelected = null;
            vehicleSelected = null;
            RoutePlanning = false;
            return; // Exit early, we are done
        }
    }

    public void SelectVehicleForPlanning(VehicleBase hitVehicle)
    {
        //VehicleBase hitVehicle = hit.collider.GetComponentInParent<VehicleBase>();

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
        DeselectRoute(hitVehicle);

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
        //roadSelected = hit.collider.GetComponentInParent<Road>();
        roadSelected = Grid.GetRoad(GetSnappedCenterPosition(hit.point));
        if (roadSelected is null) return;

        if (lastRoadSelected is null) // Just add the first road
        {
            if (roadSelected.Road_HasBusStop()) // first road must have bus stop
            {
                if (!vehicleSelected.AddToRoute(roadSelected)) return;
                Debug.Log("Added to route");
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
        //Debug.Log("Surface: " + surfaceLocation + ", snapped:" + snappedPos);
        BusStop busStop = Instantiate(busStopPrefab, snappedPos, Quaternion.identity);
        busStop.Type = type;

        var agent = busStop.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(); // turn navmesh off
        if (agent != null) agent.enabled = false;

        busStop.SetUp(((BusStopPreview)Preview).Data, ((BusStopPreview)Preview).BusStopModel.Rotation, foundLocation);

        (Vector3 offset, Quaternion rot) = FindBSVisualOffset(snappedPos, foundLocation);
        Vector3 surfaceLocation = FindSurfaceAt(busPosition + offset);
        Debug.Log($"Snapped: {snappedPos}, corrected: {surfaceLocation}");
        busStop.model.AdjustVisualToGround(surfaceLocation, offset, rot);
        Grid.SetBusStop(busStop, snappedPos);
        Destroy(((BusStopPreview)Preview).gameObject);
        Preview = null;

    }

    public (Vector3, Quaternion) FindBSVisualOffset(Vector3 position, ILocation location)
    {
        // Csak az X �s Z s�kon n�zz�k a k�l�nbs�get
        float diffX = location.Position.x - position.x;
        float diffZ = location.Position.z - position.z;

        float absX = Mathf.Abs(diffX);
        float absZ = Mathf.Abs(diffZ);

        float offsetAmount = painter.roadWidth;
        Vector3 finalOffset = Vector3.zero;
        float targetAngle = 0;

        // Csak X �s Z k�z�tt d�nt�nk
        if (absX > absZ)
        {
            // X ir�nyba toljuk el a modell-t
            finalOffset.x = Mathf.Sign(diffX) * offsetAmount;
            targetAngle = (diffX > 0) ? 90f : 270f;
        }
        else
        {
            // Z ir�nyba toljuk el a modell-t
            finalOffset.z = Mathf.Sign(diffZ) * offsetAmount;
            targetAngle = (diffZ > 0) ? 0f : 180f;
        }

        return (finalOffset, Quaternion.Euler(0, targetAngle, 0));
    }

    #endregion


    #region Roads



    public void HandleDestroyMode(bool shouldIDestroy)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // Raycast to find road under mouse
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 snappedPos = GetSnappedCenterPosition(hit.point);
            hovered = Grid.GetRoad(snappedPos);

            if (hovered is not null && hovered != lastHovered)
            {
                // Reset previous hover
                if (lastHovered != null)
                    lastHovered.ChangeState(RoadState.BUILT);

                // Set new hover
                if (hovered != null)
                {
                    hovered.ChangeState(RoadState.DESTROYHOVER);
                }

                lastHovered = hovered;
            }
            else if (hovered != lastHovered)
            {
                // No road under cursor  reset lastHovered
                if (lastHovered != null)
                {
                    lastHovered.ChangeState(RoadState.BUILT);
                    lastHovered = null;
                }
            }
            if (shouldIDestroy && hovered is not null) Destroy();
        }
    }

    public void Destroy()
    {
        if (Preview != null) return;
        if (hovered is null) return;
        if (Grid.IsCityRoad(hovered.transform.position)) return; // dont destroy built in roads

        Vector3 pos = hovered.transform.position;
        
        painter.ResetLayer(pos, mainLayers, hovered.Type, hovered.Model.Rotation);

        Grid.RemRoad(hovered.transform.position); // remove from grid
        Destroy(hovered.gameObject);
        lastHovered = null; // clear hover since its gone
    }

    public void PlaceRoad(Vector3 roadPosition)
    {
        if (Preview is not RoadPreview) return;
        Vector3 snappedPos = GetSnappedCenterPosition(roadPosition);

        if (grid.HasTrees(snappedPos))
        {
            grid.ClearTrees(snappedPos);
        }
        Road road = Instantiate(RoadPrefab, snappedPos, Quaternion.identity);
        road.RoadStateChangedEventHandler += Road_RoadStateChangedEventHandler;

        Debug.Log("Setting up");
        road.Setup(((RoadPreview)Preview).Data, ((RoadPreview)Preview).RoadModel.Rotation);
        if (road.data.Kind == RoadKind.Bridge)
        {
            //Vector3 surfacePoint = FindSurfaceAt(snappedPos);
            //surfacePoint += new Vector3(0, 2, 0);
            float bridgeY = grid.CalculateBridgeHeight(snappedPos, road.Model.Rotation, road.data);
            Vector3 finalPos = snappedPos;
            finalPos.y = bridgeY;
            road.Model.bridgeEffect.transform.position = finalPos;
        }
        Grid.SetRoad(road, snappedPos);

        //painter.PaintRoadAt(snappedPos, road.Type, ((RoadPreview)Preview).RoadModel.Rotation, builtLayerIndex);
        // Destroy(((RoadPreview)preview).gameObject);
        //preview = null;
    }

    public void Road_RoadStateChangedEventHandler(object sender, RoadStateChangedEventArgs e)
    {
        int lIndex;
        int prevIndex = 0;
        switch (e.NewState)
        {
            case RoadState.BUILT:
                lIndex = builtLayerIndex;
                break;
            case RoadState.DESTROYHOVER:
                lIndex = destroyHoverIndex;
                break;
            case RoadState.SELECTED:
                lIndex = selectedIndex;
                break;
            case RoadState.CONFIRMED:
                lIndex = confirmedIndex;
                break;
            default:
                return;

        }
        //Debug.Log("Placing, prev state: " + e.PrevState);
        switch(e.PrevState)
        {
            case RoadState.BUILT:
                if (e.NewState == RoadState.BUILT)
                {
                    prevIndex = positiveIndex;
                    break;
                }
                prevIndex = builtLayerIndex;
                break;
            case RoadState.DESTROYHOVER:
                prevIndex = destroyHoverIndex;
                break;
            case RoadState.SELECTED:
                prevIndex = selectedIndex;
                break;
            case RoadState.CONFIRMED:
                prevIndex = confirmedIndex;
                break;
            default:
                return;
        }
        painter.PaintRoadAt(GetSnappedCenterPosition(e.SnappedPosition), e.RoadType, e.Rotation, lIndex, mainLayers, false, prevIndex);
    }

    public void PaintRegisteredRoads(List<Road> roads)
    {
        foreach (Road road in roads)
        {
            painter.PaintRoadAt(road.transform.position, road.Type, road.Model.Rotation, builtLayerIndex, mainLayers, true, 0);
        }
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
        return new Vector3(snappedX, Grid.transform.position.y, snappedZ);
    }

    public Vector3 GetMouseWorldPosition()
    {
        if (mousePosition == null) return Vector3.zero; // Check if the mouse actually exists and is on screen

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, terrainLayer))
        {
            return hit.point;
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
        (int x, int y) = Grid.WorldToGridPosition(preview.transform.position);
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
        if (Preview == null) return;

        Preview.Rotate(rotation);
        doneRotating = true;
        int lastLayer = Grid.CanBuild(transform.position) ? positiveIndex : negativeIndex;

        if (Preview is RoadPreview rp)
        {
            if (Grid.IsRoad(lastSnappedPos))
            {
                Road road = Grid.GetRoad(lastSnappedPos);
                //painter.PaintRoadAt(lastSnappedPos, road.Type, road.Model.Rotation, builtLayerIndex, mainLayers, true, lastLayer);
            }
            else
            {
                painter.ResetLayer(lastSnappedPos, mainLayers, rp.RoadModel.RoadType, rp.RoadModel.Rotation);
            }

            //int colorIndex = canAfford ? positiveIndex : negativeIndex;
            painter.PaintRoadAt(lastSnappedPos, rp.RoadModel.RoadType, rp.RoadModel.Rotation, lastLayer, mainLayers, true, lastLayer);
        }
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
            Ray ray = Camera.main.ScreenPointToRay(mousePosition); // Vagy ahol az eg�r poz�ci�j�t t�rolod
            Vector3 hitPosition = mouseWorldPosition; // Alap�rtelmezett �rt�k, ha nem tal�lna semmit
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                hitPosition = hit.point;
            }
            roadPreview.transform.position = hitPosition;

            Vector3 buildPosition = roadPreview.RoadModel.GetAllBuildingPositions().First(); //road is only 1 tile
            Vector3 currentSnappedPos = GetSnappedCenterPosition(buildPosition); // for pre rotation

            bool canBuild = false;

            if (roadPreview.Data.Kind == RoadKind.Bridge)
            {
                float bridgeY = grid.CalculateBridgeHeight(buildPosition, roadPreview.RoadModel.Rotation, roadPreview.Data);

                canBuild = Grid.CanBuildBridge(buildPosition, roadPreview.Data, roadPreview.RoadModel.Rotation);
                if (canBuild && !destroy)
                {
                    //roadPreview.transform.position = currentSnappedPos;
                    //Vector3 effectPos = FindSurfaceAt(currentSnappedPos);

                    Vector3 effectPos = currentSnappedPos;
                    effectPos.y = bridgeY;
                    effectPos += new Vector3(0, 2, 0);
                    roadPreview.RoadModel.bridgeEffect.transform.position = effectPos;
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
                            justPlaced = true;
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
                    roadPreview.ChangeState(PreviewState.NEGATIVE);
                }
                if (currentSnappedPos != lastSnappedPos)
                {
                    lastSnappedPos = currentSnappedPos;
                }

                return;
            }


            if (currentSnappedPos != lastSnappedPos)
            {
                if (!justPlaced && !Grid.IsRoad(lastSnappedPos))
                {
                    painter.ResetLayer(lastSnappedPos, mainLayers, roadPreview.RoadModel.RoadType, roadPreview.RoadModel.Rotation);
                } else
                {
                    justPlaced = false;
                }
                doneRotating = false;
            }

            roadPreview.transform.position = mouseWorldPosition;

            canBuild = Grid.CanBuildRoad(buildPosition);
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
                    if (currentSnappedPos != lastSnappedPos)
                    {
                        painter.PaintRoadAt(currentSnappedPos, roadPreview.Data.Model.RoadType, roadPreview.RoadModel.Rotation, positiveIndex, mainLayers, true, 0);
                    }
                    if (shouldIPlace)   // placing the road
                    {
                        PlaceRoad(buildPosition);

						float price = Preview.Data.Cost;    // update affordability

						var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true }; // check for next item
						BuyRequest?.Invoke(this, checkNext);
						canAfford = checkNext.IsApproved;
                        justPlaced = true;
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
            if (currentSnappedPos != lastSnappedPos)
            {
                lastSnappedPos = currentSnappedPos;
            }
        }
        else if (Preview is BusPreview busPreview) // buses
        {
            busPreview.transform.position = mouseWorldPosition;
            //Vector3 busPosition = busPreview.Model.GetPosition();
            Vector3 busPosition = mouseWorldPosition;
            bool canBuild = Grid.CanBuildBus(busPosition);
            if (canBuild && !destroy)
            {
                //busPreview.transform.position = GetSnappedCenterPosition(busPosition);

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
                //truckPreview.transform.position = GetSnappedCenterPosition(truckPosition);

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
            //Debug.Log(Grid.WorldToGridPosition(busStopPosition));
            busStopPreview.transform.position = mouseWorldPosition;
            Vector3 snappedPos = GetSnappedCenterPosition(mouseWorldPosition);
            bool canBuild = Grid.CanBuildBusStop(snappedPos);

            if (canBuild && !destroy)
            {
                StopType stopType = Grid.GetStopType(snappedPos);
                ILocation location = Grid.GetLocationAt(snappedPos);

                Vector3 surfacePoint = FindSurfaceAt(snappedPos);

                (Vector3 offset, Quaternion rotation) = FindBSVisualOffset(surfacePoint, location);

                //busStopPreview.BusStopModel.AdjustVisualToGround(surfacePoint, offset, rotation);
                //busStopPreview.transform.position = busStopPreview.BusStopModel.transform.position;
                if (canAfford)
                {

                    busStopPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        PlaceBusStop(snappedPos, stopType);

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
                busStopPreview.transform.position = mouseWorldPosition;
                busStopPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (Preview is Cat cat)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 pos = hit.point;
                cat.transform.position = pos;
                cat.visual.transform.position = pos;
            }

            cat.ChangeState(PreviewState.POSITIVE);
            if (shouldIPlace)
            {
                DestroyPreview();
                vehiclePlaced?.Invoke(this, null);
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
        var renderers = roadPreview.GetComponentsInChildren<Renderer>(); // todo? kicsit eroforrasigenyes
        foreach (var r in renderers)
        {
            r.enabled = false;
        }

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

    private Cat CreateCatPreview(CatData data, Vector3 position)
    {
        Cat cat = Instantiate(catPrefab, position, Quaternion.identity);
        cat.Setup(data);
        return cat;
    }
    #endregion

    #region Terrain
    
    public Vector3 FindSurfaceAt(Vector3 worldPos)
    {
        if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit))
        {
            return hit.point;
        }
        Debug.LogWarning("Nem talalta meg a felszint! " + worldPos);
        return worldPos;
    }

    public void LocationsRegistered(List<ILocation> locations)
    {
        Vector3 surfacePoint;
        foreach (ILocation location in locations)
        {
            if (location.Visual is null) continue;

            surfacePoint = FindSurfaceAt(location.Position);
            location.AdjustVisualToGround(surfacePoint);
        }
    }

    #endregion


}

