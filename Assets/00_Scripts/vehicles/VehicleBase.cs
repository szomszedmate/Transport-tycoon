using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using UnityEngine.EventSystems;
using static Bus;

public abstract class VehicleBase : MonoBehaviour, IVehicle
{
    public enum VehicleState
    {
        BUILT,
        SELECTHOVER,
        CONFIRMED,
        DESTROYHOVER
    }

    public delegate void MileageChangedEventHandler(object sender, MileageChangedEventArgs e);
    public event MileageChangedEventHandler MileageChanged;
    public event EventHandler sold;

    public void Sold()
    {
        sold?.Invoke(this, EventArgs.Empty);
    }

    protected void OnMileageChanged(float distance, bool isNonStop)
    {
        MileageChanged?.Invoke(this, new MileageChangedEventArgs
        {
            NewAmount = distance,
            NonStop = isNonStop
        });
    }
    public int Cost => data.Cost;
    public float WeeklyMileage { get; protected set; }
    public bool NonStop { get; protected set; }

    protected Vector3 lastPosition;
    protected VehicleModel model;
    public VehicleData data;
    public StopType mainType;
    public List<StopType> types;
    public BusAiAgent aiAgent;
    public VehicleState State { get; protected set; } = VehicleState.BUILT;
    [SerializeField]
    protected Material builtMaterial;
    [SerializeField]
    protected Material destroyHoverMaterial;
    [SerializeField]
    protected Material selectHoverMaterial;
    [SerializeField]
    protected Material confirmMaterial;
    [SerializeField]
    protected VehicleMaterials vehicleMaterials;


    protected List<Material> materials;

    public List<Road> Route { get; protected set; }
    public List<BusStop> Stops { get; protected set; } = new List<BusStop>();
    protected List<Renderer> renderers = new();
    public bool RouteConfirmed { get; protected set; } = false;
    public virtual BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.UNKNOWN;
        }
    }

    public void ChangeState(VehicleState newState)
    {
        if (newState == State) return;
        State = newState;
        SetMaterial(State);
    }   

    protected void SetMaterial(VehicleState newState)
    {
        if (builtMaterial == null || destroyHoverMaterial == null || selectHoverMaterial == null)
            return;

        Material targetMat = (newState == VehicleState.BUILT) ? builtMaterial : destroyHoverMaterial;

        switch (newState)
        {
            case VehicleState.BUILT:
                targetMat = builtMaterial;
                break;
            case VehicleState.SELECTHOVER:
                targetMat = selectHoverMaterial;
                break;
            case VehicleState.DESTROYHOVER:
                targetMat = destroyHoverMaterial;
                break;
            case VehicleState.CONFIRMED:
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

    public virtual void ResetRoute()
    {

        RouteConfirmed = false;
        aiAgent.RemoveRoute();
        Route.Clear();
    }

    public Road GetLast()
    {
        if (Route == null || Route.Count == 0) return null;
        return Route.Last();
    }


    // K�z�s logika: Anyagok be�ll�t�sa, fut�steljes�tm�ny m�r�se
    protected virtual void Update()
    {
        // T�vols�gm�r�s k�dja...
    }

    protected virtual void Setup(VehicleData data, float rotation)
    {
        // Alapmodell bet�lt�se, anyagok be�ll�t�sa...
    }

    // Absztrakt met�dus, amit a gyerekeknek k�telez� kifejteni�k, 
    // ha m�sk�pp m�k�dnek (pl. a kamion m�shogy �ll meg)
    public abstract IEnumerator OnArrivedAtStop(BusStop stop);
}