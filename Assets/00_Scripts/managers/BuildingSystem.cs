using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] private BuildingGrid grid;

    // Sub-components (assigned via child GameObjects in the Inspector)
    [SerializeField] private RoadPlacer roadPlacer;
    [SerializeField] private VehiclePlacer vehiclePlacer;
    [SerializeField] private PreviewController previewController;
    [SerializeField] private RoutePlanner routePlanner;

    // Shared state
    public IPreview Preview { get; set; }
    public bool destroy = false;
    public Vector2 MousePosition { get; private set; }

    // Expose layer masks for sub-components
    public LayerMask TerrainLayer => terrainLayer;
    public LayerMask VehicleLayer => vehicleLayer;

    public BuildingGrid Grid { get => grid; set => grid = value; }

    // Events that remain on BuildingSystem (called by external systems)
    public event EventHandler destroymodeturn;
    public event EventHandler<BuyRequestEventArgs> BuyRequest;

    // Forwarded events from sub-components (external code subscribes here for parity)
    public event EventHandler prevdest
    {
        add => previewController.prevdest += value;
        remove => previewController.prevdest -= value;
    }
    public event EventHandler<IData> selectprev
    {
        add => previewController.selectprev += value;
        remove => previewController.selectprev -= value;
    }
    public event EventHandler vehiclePlacecancelled
    {
        add => previewController.vehiclePlacecancelled += value;
        remove => previewController.vehiclePlacecancelled -= value;
    }
    public event EventHandler<VehicleBase> vehiclePlaced
    {
        add => vehiclePlacer.vehiclePlaced += value;
        remove => vehiclePlacer.vehiclePlaced -= value;
    }
    public event VehicleBase.MileageChangedEventHandler AnyBusMileageChanged
    {
        add => vehiclePlacer.AnyBusMileageChanged += value;
        remove => vehiclePlacer.AnyBusMileageChanged -= value;
    }
    public event EventHandler<CancelChargeEventArgs> CancelCharge
    {
        add => vehiclePlacer.CancelCharge += value;
        remove => vehiclePlacer.CancelCharge -= value;
    }

    private void Awake()
    {
        roadPlacer.Init(grid, painter,
            builtLayerIndex, positiveIndex, negativeIndex,
            destroyHoverIndex, selectedIndex, confirmedIndex,
            mainLayers);

        vehiclePlacer.Init(grid, painter);

        previewController.Init(grid, painter, roadPlacer, vehiclePlacer,
            positiveIndex, negativeIndex, mainLayers);

        routePlanner.Init(grid);
        routePlanner.VehicleInfoRequested += (s, v) => VehicleInfoRequested?.Invoke(s, v);
    }

    // Helper used by sub-components
    public void InvokeBuyRequest(BuyRequestEventArgs args)
    {
        BuyRequest?.Invoke(this, args);
    }

    // Called internally to fire destroymodeturn (used by PreviewController when it clears destroy)
    public void FireDestroyModeTurn()
    {
        destroymodeturn?.Invoke(this, EventArgs.Empty);
    }

    public void InputUpdate(Vector2 mousePosition, bool leftClicked, bool rightClicked, bool leftHeld, bool rightHeld)
    {
        MousePosition = mousePosition;
        Vector3 worldPos = GetMouseWorldPosition();

        if (destroy)
        {
            roadPlacer.HandleDestroyMode(leftClicked, mousePosition);
        }
        else if (Preview != null)
        {
            previewController.HandlePreview(worldPos, leftClicked, rightClicked);
        }
        else if (routePlanner.RoutePlanning)
        {
            routePlanner.HandleRoutePlanning(mousePosition, leftHeld, rightHeld);
        }
    }

    public void DestroyMode()
    {
        destroy = !destroy;
        if (destroy)
        {
            if (Preview != null)
            {
                previewController.DestroyPreview();
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
        if (!destroy)
        {
            roadPlacer.ResetLastHovered();
        }
    }

    public void DestroyPreview()
    {
        previewController.DestroyPreview();
    }

    public void CreatePreview(IData build)
    {
        previewController.CreatePreview(build);
    }

    public void RotatePreview(int rotation)
    {
        previewController.RotatePreview(rotation);
    }

    public void HandlePreview(Vector3 mouseWorldPosition, bool shouldIPlace, bool shouldIDestroy)
    {
        previewController.HandlePreview(mouseWorldPosition, shouldIPlace, shouldIDestroy);
    }

    public void Destroy()
    {
        roadPlacer.DestroyHovered();
    }

    public void PlaceRoad(Vector3 roadPosition)
    {
        roadPlacer.PlaceRoad(roadPosition);
    }

    public void PaintRegisteredRoads(List<Road> roads)
    {
        roadPlacer.PaintRegisteredRoads(roads);
    }

    public void RegisterCityRoadsAsJokers(List<Road> roads)
    {
        roadPlacer.RegisterCityRoadsAsJokers(roads);
    }

    public void Road_RoadStateChangedEventHandler(object sender, RoadStateChangedEventArgs e)
    {
        roadPlacer.Road_RoadStateChangedEventHandler(sender, e);
    }

    // Route planning delegation
    public bool RoutePlanning => routePlanner.RoutePlanning;
    public event EventHandler<VehicleBase> VehicleInfoRequested;

    public void SelectVehicleByHit(RaycastHit hit)
    {
        routePlanner.SelectVehicleByHit(hit);
    }

    public void OpenVehicleInfo(VehicleBase hitVehicle)
    {
        routePlanner.OpenVehicleInfo(hitVehicle);
    }

    public void DeselectRoute(VehicleBase hitVehicle)
    {
        routePlanner.DeselectRoute(hitVehicle);
    }

    public void SelectVehicleForPlanning(VehicleBase hitVehicle)
    {
        routePlanner.SelectVehicleForPlanning(hitVehicle);
    }

    public void AddToRoute(RaycastHit hit)
    {
        routePlanner.AddToRoute(hit);
    }

    public void RemFromRoute(RaycastHit hit)
    {
        routePlanner.RemFromRoute(hit);
    }

    public void ResetRoute()
    {
        routePlanner.ResetRoute();
    }

    public void BS_ConfirmRoute()
    {
        routePlanner.BS_ConfirmRoute();
    }

    // VehiclePlacer passthrough
    public (Vector3, Quaternion) FindBSVisualOffset(Vector3 position, ILocation location)
    {
        return vehiclePlacer.FindBSVisualOffset(position, location);
    }

    public void invokeBuying(BuyRequestEventArgs checkNext)
    {
        BuyRequest?.Invoke(this, checkNext);
    }

    #region Grid

    public Vector3 GetSnappedCenterPosition(Vector3 buildingPosition)
    {
        float snappedX = Mathf.Floor(buildingPosition.x / CellSize) * CellSize + CellSize / 2f;
        float snappedZ = Mathf.Floor(buildingPosition.z / CellSize) * CellSize + CellSize / 2f;
        return new Vector3(snappedX, Grid.transform.position.y, snappedZ);
    }

    public Vector3 GetMouseWorldPosition()
    {
        if (MousePosition == null) return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, terrainLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    #endregion

    #region Terrain

    public Vector3 FindSurfaceAt(Vector3 worldPos)
    {
        if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit))
        {
            return hit.point;
        }
        Debug.LogWarning("Couldnt find surface at: " + worldPos);
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
