using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using System;
using static RoadPreview;

public class Road : MonoBehaviour, IBuildable
{
    public string Description => data.Description;
    public int Cost => data.Cost;
    private RoadModel model;
    [SerializeField]
    public RoadData data;
    public RoadState State { get; private set; } = RoadState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField] 
    private Material bridgeBuiltMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    [SerializeField] 
    private Material selectMaterial;
    [SerializeField] 
    private Material confirmedMaterial;
    [SerializeField]
    private bool isCityRoad;
    public bool IsCityRoad => isCityRoad;
    public bool IsCorner => model is RoadLModel;
    private List<Renderer> renderers = new();
    public bool IsBridge => data != null && data.Kind == RoadKind.Bridge;
    public RoadKind Kind => data != null ? data.Kind : RoadKind.NormalRoad;

    public RoadModel Model { get => model; set => model = value; }

    public Transform leftLane;
    public Transform rightLane;
    public Transform topLane;
    public Transform bottomLane;
    private BusStop busStop;

    public bool rightlanefree = true;
    public bool leftlanefree = true;
    public List<BusAiAgent> buszok=new List<BusAiAgent>();

    public float speedmodifier = 1;

    public BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.ROAD;
        }
    }

    public RoadType Type
    {
        get
        {
            if (model is RoadStraightModel) return RoadType.STRAIGHT;
            if (model is RoadLModel) return RoadType.TURN;
            if (model is RoadTModel) return RoadType.T;
            if (Model is RoadCrossModel) return RoadType.CROSS;
            return RoadType.UNKNOWN;
        }
    }

    public BusStop BusStop { get => busStop; private set => busStop = value; }

    public void Awake()
    {
        // If model is still null (pre-placed road), try to find it in children
        if (Model == null)
        {
            Model = GetComponentInChildren<RoadModel>();
        }

        // Also grab renderers for pre-placed roads so they can change color
        if (renderers.Count == 0 && Model != null)
        {
            renderers.AddRange(Model.GetComponentsInChildren<Renderer>());
        }

    }

    public void AddBusz(BusAiAgent bus)
    {
        buszok.Add(bus);
    }

    public void RemBusz()
    {
       
        if (buszok.Count>0)
        {
            buszok.Remove(buszok.Last());
            if (buszok.Count > 0)
            {
                buszok.Last().startbusz();
            }
        }
    }

    public event EventHandler OnSetFree;
    
    public void OnFreeSetted()
    {
        OnSetFree?.Invoke(this, EventArgs.Empty);
    }

    private void SetupLanes()
    {
        Transform wrapper = Model.transform.Find("Wrapper");
        if (wrapper == null)
        {
            Debug.LogWarning($"Wrapper not found for road: {data.Description}");
            return;
        }

        leftLane = wrapper.Find("laneLeft");
        rightLane = wrapper.Find("laneRight");
        topLane = wrapper.Find("laneFelso");
        bottomLane = wrapper.Find("laneAlso");

        if (leftLane == null) leftLane = wrapper.Find("lane 1");
        if (rightLane == null) rightLane = wrapper.Find("lane 2");
    }

    public void Setup(RoadData data, float rotation)
    {
        
        this.data = data;
        speedmodifier = data.SpeedModifier;

        // Instantiate the actual model first
        Model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        Model.Rotate(rotation);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(Model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetRoadMaterial(RoadState.BUILT);
        SetupLanes();
    }

    public void ChangeState(RoadState newState)
    {
        if (newState == State) return;
        if (newState == RoadState.DESTROYHOVER && isCityRoad) return;

        State = newState;
        SetRoadMaterial(State);
    }

    private void SetRoadMaterial(RoadState newState)
    {
        if (builtMaterial == null || (bridgeBuiltMaterial == null && !isCityRoad) || destroyHoverMaterial == null || selectMaterial == null || confirmedMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

        Material targetMat;

        switch (newState)
        {
            case RoadState.BUILT:
                targetMat = data != null && data.Kind == RoadKind.Bridge ? bridgeBuiltMaterial : builtMaterial;
                break;
            case RoadState.SELECTED:
                targetMat = selectMaterial;
                break;
            case RoadState.DESTROYHOVER:
                targetMat = destroyHoverMaterial;
                break;
            case RoadState.CONFIRMED:
                targetMat = confirmedMaterial;
                break;
            default:
                targetMat = data != null && data.Kind == RoadKind.Bridge ? bridgeBuiltMaterial : builtMaterial;
                break;
        }

        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = targetMat;
            }
            rend.materials = mats;
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

    public bool IsConnectedTo(Road nextRoad, Direction vehicleDirection)
    {
        bool nextToEachOther = false;
        bool roadsMatching = false;
        float firstX = transform.position.x;
        float firstZ = transform.position.z;
        float nextX = nextRoad.transform.position.x;
        float nextZ = nextRoad.transform.position.z;
        switch (vehicleDirection)
        {
            case Direction.N:
                if (firstZ < nextZ && nextZ - firstZ <= 10 && firstX == nextX) // next road is north of it and 1 tile away
                {
                    nextToEachOther = true;
                }
                break;
            case Direction.W:
                if (nextX < firstX && firstX - nextX <= 10 && firstZ == nextZ)
                    nextToEachOther = true;
                break;
            case Direction.S:
                if (nextZ < firstZ && firstZ - nextZ <= 10 && firstX == nextX)
                    nextToEachOther = true;
                break;
            case Direction.E:
                if (firstX < nextX && nextX - firstX <= 10 && firstZ == nextZ) // next road is east of it and 1 tile away
                {
                    nextToEachOther = true;
                }
                break;
            default:
                break;
        }
        if (Model.Outputs.Contains<Direction>(vehicleDirection) && nextRoad.Model.Inputs.Contains<Direction>(vehicleDirection)) roadsMatching = true; // the vehicle can leave this road and enter next road based on direction
        return roadsMatching && nextToEachOther;
    }

    [ContextMenu("Force Setup Pre-placed Road")]
    public void ForceSetup()
    {
        // Try to find the model if it exists
        Model = GetComponentInChildren<RoadModel>();

        // Find all renderers in children
        renderers.Clear();
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        // Refresh the material
        SetRoadMaterial(RoadState.BUILT);

        Debug.Log($"{gameObject.name} has been manually initialized!");
    }

    public void SetBusStop(BusStop busStop)
    {
        this.BusStop = busStop;
    }

    public void RemBusStop()
    {
        this.BusStop = null;
    }

    public bool Road_HasBusStop()
    {
        return (BusStop != null);
    }

    public StopType GetStopType()
    {
        if (BusStop == null) return StopType.None;
        return BusStop.Type;
    }

}
