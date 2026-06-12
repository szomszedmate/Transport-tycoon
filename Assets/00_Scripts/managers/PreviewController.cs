using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PreviewController : MonoBehaviour
{
    [Header("Previews")]
    [SerializeField] private RoadPreview roadPreviewPrefab;
    [SerializeField] private BusPreview busPreviewPrefab;
    [SerializeField] private TruckPreview truckPreviewPrefab;
    [SerializeField] private BusStopPreview busStopPreviewPrefab;

    [Header("Cat")]
    [SerializeField] private Cat catPrefab;
    [SerializeField] private CatData catData;

    private Vector3 lastSnappedPos;
    private bool doneRotating = false;
    private bool canAfford;
    private bool justPlaced = false;

    public event EventHandler prevdest;
    public event EventHandler<IData> selectprev;
    public event EventHandler vehiclePlacecancelled;

    // Shared references
    private BuildingSystem bs;
    private BuildingGrid grid;
    private TerrainPainter painter;
    private RoadPlacer roadPlacer;
    private VehiclePlacer vehiclePlacer;
    private int positiveIndex;
    private int negativeIndex;
    private List<int> mainLayers;

    // Mirror public accessor so BuildingSystem can expose it
    public Vector3 LastSnappedPos => lastSnappedPos;

    private void Awake()
    {
        bs = GetComponentInParent<BuildingSystem>();
    }

    public void Init(BuildingGrid grid, TerrainPainter painter, RoadPlacer roadPlacer, VehiclePlacer vehiclePlacer,
        int positiveIndex, int negativeIndex, List<int> mainLayers)
    {
        if (bs == null) bs = GetComponentInParent<BuildingSystem>();
        this.grid = grid;
        this.painter = painter;
        this.roadPlacer = roadPlacer;
        this.vehiclePlacer = vehiclePlacer;
        this.positiveIndex = positiveIndex;
        this.negativeIndex = negativeIndex;
        this.mainLayers = mainLayers;
    }

    public void DestroyPreview()
    {
        if (bs.destroy && bs.Preview == null)
        {
            // nothing to do
        }
        else if (bs.Preview != null)
        {
            if (!grid.IsRoad(lastSnappedPos) && (bs.Preview is RoadPreview rp))
            {
                painter.ResetLayer(lastSnappedPos, mainLayers, rp.RoadModel.RoadType, rp.RoadModel.Rotation);
            }
            Destroy(((MonoBehaviour)bs.Preview).gameObject);
            bs.Preview = null;
            roadPlacer.ActiveJoker = null;
            prevdest?.Invoke(this, EventArgs.Empty);
        }
    }

    public void CreatePreview(IData build)
    {
        if (bs.destroy)
        {
            bs.destroy = false;
            bs.FireDestroyModeTurn();
        }

        DestroyPreview();
        Vector3 worldPos = bs.GetMouseWorldPosition();
        selectprev?.Invoke(this, build);

        if (build is JokerRoadData jokerData)
        {
            roadPlacer.ActiveJoker = jokerData;
            roadPlacer.LastJokerType = RoadType.UNKNOWN;
            bs.Preview = CreateRoadPreview(jokerData.StraightRoad, bs.MousePosition);
        }
        else if (build is RoadData roadData)
        {
            roadPlacer.ActiveJoker = null;
            bs.Preview = CreateRoadPreview(roadData, bs.MousePosition);
        }
        else if (build is BusData busData)
        {
            bs.Preview = CreateBusPreview(busData, bs.MousePosition);
        }
        else if (build is TruckData truckData)
        {
            bs.Preview = CreateTruckPreview(truckData, bs.MousePosition);
        }
        else if (build is BusStopData busStopData)
        {
            bs.Preview = CreateBusStopPreview(busStopData, worldPos);
        }
        else if (build is CatData catDataArg)
        {
            bs.Preview = CreateCatPreview(catDataArg, bs.MousePosition);
        }
        else return;

        float price = bs.Preview.Data.Cost;
        var args = new BuyRequestEventArgs { Cost = price, Deduct = false };
        bs.InvokeBuyRequest(args);
        canAfford = args.IsApproved;
    }



    private int RotateToMatch(RoadPreview roadPreview, Road otherRoad)
    {
        Direction? direction = grid.GetRelativePreviewDirection(roadPreview, otherRoad);
        if (direction == null) return -1;
        for (int rotation = 0; rotation < 4; rotation++)
        {
            if (grid.IsPreviewConnectedTo((RoadPreview)bs.Preview, otherRoad, (Direction)direction))
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
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Mathf.Abs(i) == Mathf.Abs(j)) continue;

                int newX = x + i;
                int newY = y + j;

                if (newX < 0 || newX >= grid.Width || newY < 0 || newY >= grid.Height) continue;
                if (!(grid.Grid[newX, newY].IsRoad())) continue;

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
        if (bs.Preview == null) return;

        bs.Preview.Rotate(rotation);
        doneRotating = true;
        int lastLayer = grid.CanBuild(transform.position) ? positiveIndex : negativeIndex;

        if (bs.Preview is RoadPreview rp)
        {
            if (grid.IsRoad(lastSnappedPos))
            {
                Road road = grid.GetRoad(lastSnappedPos);
            }
            else
            {
                painter.ResetLayer(lastSnappedPos, mainLayers, rp.RoadModel.RoadType, rp.RoadModel.Rotation);
            }

            painter.PaintRoadAt(lastSnappedPos, rp.RoadModel.RoadType, rp.RoadModel.Rotation, lastLayer, mainLayers, true, lastLayer);
        }
    }

    public void HandlePreview(Vector3 mouseWorldPosition, bool shouldIPlace, bool shouldIDestroy)
    {
        if (shouldIDestroy)
        {
            DestroyPreview();
            vehiclePlacecancelled?.Invoke(this, EventArgs.Empty);
            return;
        }
        if (!canAfford)
        {
            bs.Preview.ChangeState(PreviewState.NEGATIVE);
        }
        if (bs.Preview is RoadPreview roadPreview) // roads
        {
            Ray ray = Camera.main.ScreenPointToRay(bs.MousePosition);
            Vector3 hitPosition = mouseWorldPosition;
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bs.TerrainLayer))
            {
                hitPosition = hit.point;
            }
            roadPreview.transform.position = hitPosition;

            Vector3 buildPosition = roadPreview.RoadModel.GetAllBuildingPositions().First();
            Vector3 currentSnappedPos = bs.GetSnappedCenterPosition(buildPosition);

            // Joker: resolve road type based on neighbors
            if (roadPlacer.ActiveJoker != null && currentSnappedPos != lastSnappedPos)
            {
                var (resolvedData, resolvedRotation) = roadPlacer.ActiveJoker.Resolve(grid, currentSnappedPos);
                if (resolvedData.Model.RoadType != roadPlacer.LastJokerType)
                {
                    roadPlacer.LastJokerType = resolvedData.Model.RoadType;
                    if (!grid.IsRoad(lastSnappedPos))
                        painter.ResetLayer(lastSnappedPos, mainLayers, roadPreview.RoadModel.RoadType, roadPreview.RoadModel.Rotation);
                    Destroy(((MonoBehaviour)bs.Preview).gameObject);
                    bs.Preview = CreateRoadPreview(resolvedData, currentSnappedPos);
                    roadPreview = (RoadPreview)bs.Preview;
                    doneRotating = false;
                }
                int currentRot = Mathf.RoundToInt(roadPreview.RoadModel.Rotation) % 360;
                int targetRot = resolvedRotation % 360;
                if (currentRot != targetRot)
                {
                    int steps = ((targetRot - currentRot) / 90 + 4) % 4;
                    for (int r = 0; r < steps; r++)
                        roadPreview.Rotate(90);
                }
                doneRotating = true;
            }

            bool canBuild = false;

            if (roadPreview.Data.Kind == RoadKind.Bridge)
            {
                float bridgeY = grid.CalculateBridgeHeight(buildPosition, roadPreview.RoadModel.Rotation, roadPreview.Data);

                canBuild = grid.CanBuildBridge(buildPosition, roadPreview.Data, roadPreview.RoadModel.Rotation);
                if (canBuild && !bs.destroy)
                {
                    Vector3 effectPos = currentSnappedPos;
                    effectPos.y = bridgeY;
                    effectPos += new Vector3(0, 2, 0);
                    roadPreview.RoadModel.bridgeEffect.transform.position = effectPos;
                    if (!doneRotating)
                    {
                        CheckRoadsForMatch((RoadPreview)bs.Preview);
                    }

                    if (canAfford)
                    {
                        roadPreview.ChangeState(PreviewState.POSITIVE);
                        if (shouldIPlace)
                        {
                            roadPlacer.PlaceRoad(buildPosition);

                            float price = bs.Preview.Data.Cost;
                            var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                            bs.InvokeBuyRequest(checkNext);
                            canAfford = checkNext.IsApproved;
                            justPlaced = true;
                        }
                    }
                    else if (shouldIPlace)
                    {
                        float price = bs.Preview.Data.Cost;
                        var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                        bs.InvokeBuyRequest(checkNext);
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
                if (!justPlaced && !grid.IsRoad(lastSnappedPos))
                {
                    painter.ResetLayer(lastSnappedPos, mainLayers, roadPreview.RoadModel.RoadType, roadPreview.RoadModel.Rotation);
                }
                else
                {
                    justPlaced = false;
                }
                doneRotating = false;
            }

            roadPreview.transform.position = mouseWorldPosition;

            canBuild = grid.CanBuildRoad(buildPosition);
            if (canBuild && !bs.destroy)
            {
                roadPreview.transform.position = currentSnappedPos;
                if (!doneRotating)
                {
                    CheckRoadsForMatch((RoadPreview)bs.Preview);
                }

                if (canAfford)
                {
                    roadPreview.ChangeState(PreviewState.POSITIVE);
                    if (currentSnappedPos != lastSnappedPos)
                    {
                        painter.PaintRoadAt(currentSnappedPos, roadPreview.Data.Model.RoadType, roadPreview.RoadModel.Rotation, positiveIndex, mainLayers, true, 0);
                    }
                    if (shouldIPlace)
                    {
                        roadPlacer.PlaceRoad(buildPosition);

                        float price = bs.Preview.Data.Cost;
                        var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                        bs.InvokeBuyRequest(checkNext);
                        canAfford = checkNext.IsApproved;
                        justPlaced = true;
                    }
                }
                else if (shouldIPlace)
                {
                    float price = bs.Preview.Data.Cost;
                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                    bs.InvokeBuyRequest(checkNext);
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
        else if (bs.Preview is BusPreview busPreview) // buses
        {
            busPreview.transform.position = mouseWorldPosition;
            Vector3 busPosition = mouseWorldPosition;
            bool canBuild = grid.CanBuildBus(busPosition);
            if (canBuild && !bs.destroy)
            {
                if (canAfford)
                {
                    busPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        vehiclePlacer.PlaceBus(busPosition);
                    }
                }
                else if (shouldIPlace)
                {
                    float price = bs.Preview.Data.Cost;
                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                    bs.InvokeBuyRequest(checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                busPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (bs.Preview is TruckPreview truckPreview)
        {
            truckPreview.transform.position = mouseWorldPosition;
            Vector3 truckPosition = truckPreview.Model.GetPosition();
            bool canBuild = grid.CanBuildBus(truckPosition);
            if (canBuild && !bs.destroy)
            {
                if (canAfford)
                {
                    truckPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        vehiclePlacer.PlaceTruck(truckPosition);
                    }
                }
                else if (shouldIPlace)
                {
                    float price = bs.Preview.Data.Cost;
                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                    bs.InvokeBuyRequest(checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                truckPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (bs.Preview is BusStopPreview busStopPreview) // bus stops
        {
            busStopPreview.transform.position = mouseWorldPosition;
            Vector3 snappedPos = bs.GetSnappedCenterPosition(mouseWorldPosition);
            bool canBuild = grid.CanBuildBusStop(snappedPos);

            if (canBuild && !bs.destroy)
            {
                StopType stopType = grid.GetStopType(snappedPos);
                ILocation location = grid.GetLocationAt(snappedPos);

                Vector3 surfacePoint = bs.FindSurfaceAt(snappedPos);

                (Vector3 offset, Quaternion rotation) = vehiclePlacer.FindBSVisualOffset(surfacePoint, location);

                if (canAfford)
                {
                    busStopPreview.ChangeState(PreviewState.POSITIVE);
                    if (shouldIPlace)
                    {
                        vehiclePlacer.PlaceBusStop(snappedPos, stopType);

                        float price = busStopPreview.Data.Cost;
                        BuyRequestEventArgs checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                        bs.InvokeBuyRequest(checkNext);
                        canAfford = checkNext.IsApproved;
                    }
                }
                else if (shouldIPlace)
                {
                    float price = bs.Preview.Data.Cost;
                    var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
                    bs.InvokeBuyRequest(checkNext);
                    canAfford = checkNext.IsApproved;
                }
            }
            else
            {
                busStopPreview.transform.position = mouseWorldPosition;
                busStopPreview.ChangeState(PreviewState.NEGATIVE);
            }
        }
        else if (bs.Preview is Cat cat)
        {
            Ray ray = Camera.main.ScreenPointToRay(bs.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bs.TerrainLayer))
            {
                Vector3 pos = hit.point;
                cat.transform.position = pos;
                cat.visual.transform.position = pos;
            }

            cat.ChangeState(PreviewState.POSITIVE);
            if (shouldIPlace)
            {
                DestroyPreview();
                vehiclePlacer.InvokeVehiclePlaced(this, null);
            }
        }
    }

    private RoadPreview CreateRoadPreview(RoadData data, Vector3 position)
    {
        RoadPreview roadPreview = Instantiate(roadPreviewPrefab, position, Quaternion.identity);
        roadPreview.Setup(data);
        var renderers = roadPreview.GetComponentsInChildren<Renderer>();
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

    // Keep affordability in sync when called externally
    public void RefreshAffordability()
    {
        if (bs.Preview == null) return;
        float price = bs.Preview.Data.Cost;
        var args = new BuyRequestEventArgs { Cost = price, Deduct = false };
        bs.InvokeBuyRequest(args);
        canAfford = args.IsApproved;
    }
}
