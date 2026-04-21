using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bus : MonoBehaviour, IVehicle
{
    public enum BusState
    {
        BUILT,
        SELECTHOVER,
        CONFIRMED,
        DESTROYHOVER
    }
    public delegate void MileageChangedEventHandler(object sender, MileageChangedEventArgs e);
    public event MileageChangedEventHandler MileageChanged;
    public int Cost => data.Cost;
    public float WeeklyMileage {  get; private set; }
    public bool NonStop { get; private set; }

    private Vector3 lastPosition;
    private BusModel model;
    private BusData data;
    public StopType type;
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



    private List<Material> materials;

    public List<Road> Route { get; private set; }
    private List<Renderer> renderers = new();
    public bool RouteConfirmed { get; private set; } = false;
    public bool RouteIsLinear { get; private set; }
    public BusType BusType
    {
        get
        {
            if (model is BusBasicModel) return BusType.BASIC;
            if (model is BusAdvancedModel) return BusType.ADVANCED;
            if (model is BusPremiumModel) return BusType.PREMIUM;
            return BusType.UNKNOWN;
        }
    }

    public BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.BUS;
        }
    }

    void Update()
    {
        float distance = Math.Abs(Vector3.Distance(transform.position, lastPosition));

        if (distance > 0)
        {
            {
                MileageChanged?.Invoke(this, new MileageChangedEventArgs
                {
                    NewAmount = distance, // változást elküldjük
                    NonStop = this.NonStop
                });

                WeeklyMileage += distance; // statisztikának maybe
                lastPosition = transform.position;
            }
        }
    }

    public void Setup(BusData data, float rotation)
    {
        this.data = data;
        type = data.Type;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;
        
        NonStop = false;
        lastPosition = transform.position;
        WeeklyMileage = 0;
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
        
        //buildmaterials in player inventori in managers
        switch (type)
        {
            case StopType.None:
                break;
            case StopType.Bus:
                builtMaterial = materials[0];
                break;
            case StopType.Universal:
                builtMaterial = materials[9];
                break;
            case StopType.Coal:
                builtMaterial = materials[3];
                break;
            case StopType.IronOre:
                builtMaterial = materials[7];
                break;
            case StopType.GoldOre:
                builtMaterial = materials[6];
                break;
            case StopType.Flour:
                builtMaterial = materials[5];
                break;
            case StopType.Water:
                builtMaterial = materials[1];
                break;
            case StopType.Farm:
                builtMaterial = materials[4];
                break;
            case StopType.IronBar:
                builtMaterial = materials[7];
                break;
            case StopType.GoldBar:
                builtMaterial = materials[6];
                break;
            case StopType.Mint:
                builtMaterial = materials[8];
                break;
            case StopType.Bakery:
                builtMaterial = materials[2];
                break;
            default:
                builtMaterial = materials[0];
                break;
        }
        // Set the default material
        SetBusMaterial(BusState.BUILT);
        Route = new List<Road>();
        aiAgent = GetComponent<BusAiAgent>();
        aiAgent.type = type;
        aiAgent.speed = data.Speed;
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
       
        RouteConfirmed = false;
        aiAgent.RemoveRoute();
        Route.Clear();
    }

    public Road GetLast()
    {
        if (Route == null || Route.Count == 0) return null;
        return Route.Last();
    }

    public void ConfirmRoute(bool linear)
    {
        if (!(Route.Last().Road_HasBusStop()) && linear)
        {
            Debug.Log("Cant confirm");
            return;
        }
        RouteConfirmed = !RouteConfirmed;
        RouteIsLinear = linear;
        NonStop = !linear;
        foreach (Road road in Route)
        {
            road.ChangeState(RouteConfirmed ? RoadState.CONFIRMED : RoadState.SELECTED);
        }

        ChangeState(RouteConfirmed ? BusState.CONFIRMED : BusState.SELECTHOVER);
        if (RouteConfirmed) aiAgent.GiveRoute(Route);
    }
}
