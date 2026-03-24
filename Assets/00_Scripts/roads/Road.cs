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
        DESTROYHOVER
    }
    public string Description => data.Description;
    public int Cost => data.Cost;
    private RoadModel model;
    private RoadData data;
    public RoadState State { get; private set; } = RoadState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    [SerializeField]
    private bool isCityRoad;
    public bool IsCityRoad => isCityRoad;
    private List<Renderer> renderers = new();

    public void Setup(RoadData data, float rotation)
    {
        this.data = data;

        // Instantiate the actual model first
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.Rotate(rotation);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetRoadMaterial(RoadState.BUILT);
    }

    public void ChangeState(RoadState newState)
    {
        if (isCityRoad) return;
        if (newState == State) return;
        State = newState;
        SetRoadMaterial(State);
    }

    private void SetRoadMaterial(RoadState newState)
    {
        // Make sure your materials are assigned
        if (builtMaterial == null || destroyHoverMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

        Material targetMat = (newState == RoadState.BUILT) ? builtMaterial : destroyHoverMaterial;

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

    public bool isConnectedTo(Road nextRoad, Direction vehicleDirection)
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
                if (nextX < firstX && nextX - firstX <= 10 && firstZ == nextZ) // next road is west of it and 1 tile away
                {
                    nextToEachOther = true;
                }
                break;
            case Direction.S:
                if (nextZ < firstZ && nextZ - firstZ <= 10 && firstX == nextX) // next road is south of it and 1 tile away
                {
                    nextToEachOther = true;
                }
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
        if (model.Outputs.Contains<Direction>(vehicleDirection) && nextRoad.model.Inputs.Contains<Direction>(vehicleDirection)) roadsMatching = true; // the vehicle can leave this road and enter next road based on direction
        return roadsMatching && nextToEachOther;
    }
}
