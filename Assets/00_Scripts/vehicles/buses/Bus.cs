using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Road;

public class Bus : MonoBehaviour, IVehicle
{
    public enum BusState
    {
        BUILT,
        SELECTHOVER,
        CONFIRMED,
        DESTROYHOVER
    }
    public int Cost => data.Cost;
    private BusModel model;
    private BusData data;

    public BusAiAgent aiAgent;
    public BusState State { get; private set; } = BusState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    [SerializeField]
    private Material selectHoverMaterial;
    [SerializeField]
    private Material confirmMaterial;
  
    public List<Road> Route { get; private set; }
    private List<Renderer> renderers = new();
    public bool RouteConfirmed { get; private set; } = false;

    public void Setup(BusData data, float rotation)
    {
        this.data = data;

        // Instantiate the actual model first
        model = Instantiate(data.Model, transform.position, Quaternion.Euler(-90, 0, rotation), transform);
        //model.transform.localPosition = Vector3.zero;
        //model.Rotate(rotation);
        //model = Instantiate(data.Model, transform);
        //model.transform.localPosition = Vector3.zero;
        //model.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetBusMaterial(BusState.BUILT);
        Route = new List<Road>();
        aiAgent = GetComponent<BusAiAgent>();
    }

    public void ChangeState(BusState newState)
    {
        if (newState == State) return;
        State = newState;
        SetBusMaterial(State);
    }

    private void SetBusMaterial(BusState newState)
    {
        if (builtMaterial == null || destroyHoverMaterial == null || selectHoverMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

        Material targetMat = (newState == BusState.BUILT) ? builtMaterial : destroyHoverMaterial;

        switch (newState)
        {
            case BusState.BUILT:
                targetMat = builtMaterial;
                break;
            case BusState.SELECTHOVER:
                targetMat = selectHoverMaterial;
                break;
            case BusState.DESTROYHOVER:
                targetMat = destroyHoverMaterial;
                break;
            case BusState.CONFIRMED:
                targetMat = confirmMaterial;
                break;
            default:
                targetMat = builtMaterial;
                break;
        }

        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length]; // keep same number of slots
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = targetMat;
            }
            rend.materials = mats; // assigns a runtime instance
        }
    }

    public bool AddToRoute(Road road)
    {
        if (RouteConfirmed) return false; // route needs reset first

        if (Route.Contains(road)) return false;
        if (road == null) return false;
        Route.Add(road);
        return true;
    }

    public bool RemFromRoute(Road road)
    {
        if (RouteConfirmed) return false;
        if (Route.Last() == road)
        {
            Route.Remove(road);
            return true;
        }
        return false;
    }

    public void ResetRoute()
    {
        Route.Clear();
        RouteConfirmed = false;
        aiAgent.RemoveRoute();
    }

    public void ConfirmRoute()
    {
        RouteConfirmed = true;
        
        foreach (Road road in Route)
        {
            road.ChangeState(RoadState.CONFIRMED);
        }

        ChangeState(BusState.CONFIRMED);
        aiAgent.GiveRoute(Route);
    }

    public (bool, Road) HasRoute()
    {
        if (Route.Count == 0) return (false, null);
        return (true, Route.Last());
    }
}
