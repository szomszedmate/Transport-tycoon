using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    protected int currentLoad;
    protected Vector3 lastPosition;
    protected VehicleModel model;
    protected VehicleData data;
    public StopType type;
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
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

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

    public void ResetRoute()
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


    // Közös logika: Anyagok beállítása, futásteljesítmény mérése
    protected virtual void Update()
    {
        // Távolságmérés kódja...
    }

    protected virtual void Setup(VehicleData data, float rotation)
    {
        // Alapmodell betöltése, anyagok beállítása...
    }

    // Absztrakt metódus, amit a gyerekeknek kötelezõ kifejteniük, 
    // ha másképp mûködnek (pl. a kamion máshogy áll meg)
    public abstract IEnumerator OnArrivedAtStop(BusStop stop);
}