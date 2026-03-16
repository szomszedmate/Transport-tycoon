using System.Collections.Generic;
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
}
