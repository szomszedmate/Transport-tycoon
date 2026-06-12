using System;
using UnityEngine;

public class VehiclePlacer : MonoBehaviour
{
    [Header("Vehicles")]
    [SerializeField] private Truck truckPrefab;
    [SerializeField] private Bus busPrefab;
    [SerializeField] private BusStop busStopPrefab;
    [SerializeField] private BusStopData BusStopData;

    public Bus BusPrefab { get => busPrefab; }
    public BusStop BusStopPrefab { get => busStopPrefab; }
    public Truck TruckPrefab { get => truckPrefab; }

    public event EventHandler<VehicleBase> vehiclePlaced;
    public event VehicleBase.MileageChangedEventHandler AnyBusMileageChanged;
    public event EventHandler<CancelChargeEventArgs> CancelCharge;

    public void InvokeVehiclePlaced(object sender, VehicleBase vehicle) =>
        vehiclePlaced?.Invoke(sender, vehicle);

    // Shared references
    private BuildingSystem bs;
    private BuildingGrid grid;
    private TerrainPainter painter;

    private void Awake()
    {
        bs = GetComponentInParent<BuildingSystem>();
    }

    public void Init(BuildingGrid grid, TerrainPainter painter)
    {
        if (bs == null) bs = GetComponentInParent<BuildingSystem>();
        this.grid = grid;
        this.painter = painter;
    }

    public void PlaceBus(Vector3 busPosition)
    {
        if (bs.Preview is not BusPreview) return;
        Vector3 snappedPos = bs.GetSnappedCenterPosition(busPosition);
        Bus bus = Instantiate(busPrefab, snappedPos, Quaternion.identity);
        bus.CancelCharge += Bus_CancelCharge;

        var agent = bus.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            agent.Warp(snappedPos);
        }
        float busRotation = ((BusPreview)bs.Preview).Model.Rotation;
        Road busRoad = grid.GetRoad(snappedPos);
        if (busRoad != null && busRoad.Road_HasBusStop() && busRoad.BusStop.location != null)
        {
            (_, Quaternion stopRot) = FindBSVisualOffset(snappedPos, busRoad.BusStop.location);
            busRotation = stopRot.eulerAngles.y + 90f;
        }
        bus.Setup((BusData)((BusPreview)bs.Preview).Data, busRotation, bs.TerrainLayer, grid);
        bus.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);

        Vector3 surfaceLocation = bs.FindSurfaceAt(busPosition);

        grid.SetVehicle(bus, snappedPos);
        Destroy(((MonoBehaviour)bs.Preview).gameObject);
        vehiclePlaced?.Invoke(this, bus);
        bs.Preview = null;
        VehicleSpawnAnimation.Play(bus, snappedPos);
    }

    private void Bus_CancelCharge(object sender, CancelChargeEventArgs e)
    {
        CancelCharge?.Invoke(this, e);
    }

    public void PlaceTruck(Vector3 truckPosition)
    {
        if (bs.Preview is not TruckPreview) return;
        Vector3 snappedPos = bs.GetSnappedCenterPosition(truckPosition);
        Truck truck = Instantiate(truckPrefab, snappedPos, Quaternion.identity);

        var agent = truck.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        float truckRotation = ((TruckPreview)bs.Preview).Model.Rotation;
        Road truckRoad = grid.GetRoad(snappedPos);
        if (truckRoad != null && truckRoad.Road_HasBusStop() && truckRoad.BusStop.location != null)
        {
            (_, Quaternion stopRot) = FindBSVisualOffset(snappedPos, truckRoad.BusStop.location);
            truckRotation = stopRot.eulerAngles.y + 90f;
        }
        truck.Setup((TruckData)((TruckPreview)bs.Preview).Data, truckRotation, bs.TerrainLayer, grid);
        truck.MileageChanged += (dist, nonStop) => AnyBusMileageChanged?.Invoke(dist, nonStop);
        grid.SetVehicle(truck, snappedPos);
        Destroy(((MonoBehaviour)bs.Preview).gameObject);
        vehiclePlaced?.Invoke(this, truck);
        bs.Preview = null;
        VehicleSpawnAnimation.Play(truck, snappedPos);
    }

    public void PlaceBusStop(Vector3 busPosition, StopType type)
    {
        if (bs.Preview is not BusStopPreview) return;

        Vector3 snappedPos = bs.GetSnappedCenterPosition(busPosition);
        ILocation foundLocation = grid.GetLocationAt(snappedPos);
        BusStop busStop = Instantiate(busStopPrefab, snappedPos, Quaternion.identity);
        busStop.Type = type;

        var agent = busStop.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        busStop.SetUp(((BusStopPreview)bs.Preview).Data, ((BusStopPreview)bs.Preview).BusStopModel.Rotation, foundLocation);

        (Vector3 offset, Quaternion rot) = FindBSVisualOffset(snappedPos, foundLocation);
        Vector3 surfaceLocation = bs.FindSurfaceAt(busPosition + offset);
        busStop.model.AdjustVisualToGround(surfaceLocation, offset, rot);
        grid.SetBusStop(busStop, snappedPos);
        Destroy(((BusStopPreview)bs.Preview).gameObject);
        bs.Preview = null;
    }

    public (Vector3, Quaternion) FindBSVisualOffset(Vector3 position, ILocation location)
    {
        float diffX = location.Position.x - position.x;
        float diffZ = location.Position.z - position.z;

        float absX = Mathf.Abs(diffX);
        float absZ = Mathf.Abs(diffZ);

        float offsetAmount = painter.roadWidth;
        Vector3 finalOffset = Vector3.zero;
        float targetAngle = 0;

        if (absX > absZ)
        {
            finalOffset.x = Mathf.Sign(diffX) * offsetAmount;
            targetAngle = (diffX > 0) ? 90f : 270f;
        }
        else
        {
            finalOffset.z = Mathf.Sign(diffZ) * offsetAmount;
            targetAngle = (diffZ > 0) ? 0f : 180f;
        }

        return (finalOffset, Quaternion.Euler(0, targetAngle, 0));
    }
}
