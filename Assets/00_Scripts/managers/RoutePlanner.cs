using System;
using System.Linq;
using UnityEngine;

public class RoutePlanner : MonoBehaviour
{
    public bool RoutePlanning { get; private set; } = false;

    public event EventHandler<VehicleBase> VehicleInfoRequested;

    private VehicleBase vehicleSelected;
    private VehicleBase lastVehicleSelected;
    private Road roadSelected;
    private Road lastRoadSelected;
    private Road hovered;
    private Road lastHovered;

    // Shared references
    private BuildingSystem bs;
    private BuildingGrid grid;

    private void Awake()
    {
        bs = GetComponentInParent<BuildingSystem>();
    }

    public void Init(BuildingGrid grid)
    {
        if (bs == null) bs = GetComponentInParent<BuildingSystem>();
        this.grid = grid;
    }

    public void HandleRoutePlanning(Vector2 mousePosition, bool leftHeld, bool rightHeld)
    {
        if (leftHeld)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit routeHit, 1000f, bs.TerrainLayer))
            {
                AddToRoute(routeHit);
            }
        }
        else if (rightHeld)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit routeHit, 1000f, bs.TerrainLayer))
            {
                RemFromRoute(routeHit);
            }
        }
    }

    public void SelectVehicleByHit(RaycastHit hit)
    {
        VehicleVisual visual = hit.collider.GetComponentInParent<VehicleVisual>();

        if (visual != null)
        {
            Ray ray = new Ray(visual.transform.position, Vector3.up);

            if (Physics.Raycast(ray, out RaycastHit upHit, 1000f, bs.VehicleLayer))
            {
                VehicleBase baseVehicle = upHit.collider.GetComponentInParent<VehicleBase>();

                if (baseVehicle != null)
                {
                    OpenVehicleInfo(baseVehicle);
                }
            }
        }
        else
        {
            OpenVehicleInfo(null);
        }
    }

    public void OpenVehicleInfo(VehicleBase hitVehicle)
    {
        if (hitVehicle is null)
        {
            DeselectRoute(hitVehicle);
            return;
        }
        VehicleInfoRequested?.Invoke(this, hitVehicle);
    }

    public void DeselectRoute(VehicleBase hitVehicle)
    {
        if (hitVehicle == null)
        {
            if (lastVehicleSelected != null)
            {
                lastVehicleSelected.ChangeState(VehicleBase.VehicleState.BUILT);
                foreach (Road road in lastVehicleSelected.Route)
                {
                    road.ChangeState(RoadState.BUILT);
                }
            }

            lastVehicleSelected = null;
            vehicleSelected = null;
            RoutePlanning = false;
            return;
        }
    }

    public void SelectVehicleForPlanning(VehicleBase hitVehicle)
    {
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

        DeselectRoute(hitVehicle);

        vehicleSelected = hitVehicle;
        lastVehicleSelected = vehicleSelected;

        vehicleSelected.ChangeState(VehicleBase.VehicleState.SELECTHOVER);
        RoutePlanning = true;
        lastRoadSelected = hitVehicle.GetLast();

        if (vehicleSelected.RouteConfirmed)
        {
            vehicleSelected.ChangeState(VehicleBase.VehicleState.CONFIRMED);
            foreach (Road road in vehicleSelected.Route)
            {
                road.ChangeState(RoadState.CONFIRMED);
            }
        }
        else
        {
            foreach (Road road in vehicleSelected.Route)
            {
                road.ChangeState(RoadState.SELECTED);
            }
        }
    }

    public void AddToRoute(RaycastHit hit)
    {
        if (vehicleSelected.RouteConfirmed) return;
        roadSelected = grid.GetRoad(bs.GetSnappedCenterPosition(hit.point));
        if (roadSelected is null) return;

        if (lastRoadSelected is null)
        {
            if (roadSelected.Road_HasBusStop())
            {
                if (!vehicleSelected.AddToRoute(roadSelected)) return;
                roadSelected.ChangeState(RoadState.SELECTED);
                lastRoadSelected = roadSelected;
            }
            else
            {
                return;
            }
        }
        Direction? direction = grid.GetRelativeDirection(lastRoadSelected, roadSelected);
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
        if (vehicleSelected.RouteConfirmed) return;
        roadSelected = hit.collider.GetComponentInParent<Road>();

        if (roadSelected == null || vehicleSelected.Route.Count == 0) return;
        if (roadSelected != vehicleSelected.Route.Last()) return;

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
        return;
    }

    public void BS_ConfirmRoute()
    {
        if (vehicleSelected == null || vehicleSelected.Route.Count() <= 1) return;

        Road fst = vehicleSelected.Route.First();
        Road lst = vehicleSelected.Route.Last();
        Direction? direction = grid.GetRelativeDirection(fst, lst);
        if (vehicleSelected is Bus busSelected)
        {
            if (direction is not null && fst.IsConnectedTo(lst, (Direction)direction))
            {
                busSelected.ConfirmRoute(false);
            }
            else
            {
                busSelected.ConfirmRoute(true);
            }
        }
        else if (vehicleSelected is Truck truckSeleced)
        {
            truckSeleced.ConfirmRoute();
        }
    }
}
