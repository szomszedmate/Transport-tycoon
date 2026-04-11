using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

using static RoadPreview;

public class Road : MonoBehaviour
{
    public enum RoadState
    {
        BUILT,
        SELECTED,
        CONFIRMED, // for confirmed routes
        DESTROYHOVER
    }
    public string Description => data.Description;
    
    public int Cost => data.Cost;
    private RoadModel model;
    [SerializeField]
    public RoadData data;
    public RoadState State { get; private set; } = RoadState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    [SerializeField] 
    private Material selectMaterial;
    [SerializeField] 
    private Material confirmedMaterial;
    [SerializeField]
    private bool isCityRoad;
    public bool IsCityRoad => isCityRoad;

    public RoadModel Model { get => model; set => model = value; }

    private List<Renderer> renderers = new();
    public Transform leftLane;
    public Transform rightLane;
    private BusStop busStop;

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

    public void Setup(RoadData data, float rotation)
    {
        
        this.data = data;
       
        // Instantiate the actual model first
        Model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        Model.Rotate(rotation);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(Model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetRoadMaterial(RoadState.BUILT);
        if (data.Description== "Straight Road")
        {
            Debug.Log("asdasdasd");
            leftLane= transform.Find("Road Straight(Clone)/Wrapper/laneLeft");
            rightLane = transform.Find("Road Straight(Clone)/Wrapper/laneRigth");
        }
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
        Material targetMat;

        switch (newState)
        {
            case RoadState.BUILT:
                targetMat = builtMaterial;
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
        this.busStop = busStop;
    }

    public void RemBusStop()
    {
        this.busStop = null;
    }

    public bool Road_HasBusStop()
    {
        return (busStop != null);
    }
}
