using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static BuildingPreview;

public class Building : MonoBehaviour
{
    public enum BuildingState
    {
        BUILT,
        DESTROYHOVER
    }
    public string Description => data.Description;
    public int Cost => data.Cost;
    private BuildingModel model;
    private BuildingData data;
    public BuildingState State { get; private set; } = BuildingState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    private List<Renderer> renderers = new();

    public void Setup(BuildingData data, float rotation)
    {
        this.data = data;

        // Instantiate the actual model first
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.Rotate(rotation);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetBuildingMaterial(BuildingState.BUILT);
    }

    public void ChangeState(BuildingState newState)
    {
        if (newState == State) { return; }
        State = newState;
        SetBuildingMaterial(State);
    }

    private void SetBuildingMaterial(BuildingState newState)
    {
        // Make sure your materials are assigned
        if (builtMaterial == null || destroyHoverMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

        Material targetMat = (newState == BuildingState.BUILT) ? builtMaterial : destroyHoverMaterial;

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
