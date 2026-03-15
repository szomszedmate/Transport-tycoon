using System.Collections.Generic;
using UnityEngine;
using static Building;

public class Bus : MonoBehaviour, IVehicle
{
    public enum BusState
    {
        BUILT,
        DESTROYHOVER
    }
    public string Description => data.Description;
    public int Cost => data.Cost;
    private BusModel model;
    private BusData data;
    public BusState State { get; private set; } = BusState.BUILT;
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    private List<Renderer> renderers = new();

    public void Setup(BusData data, float rotation)
    {
        this.data = data;

        // Instantiate the actual model first
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.Rotate(rotation);

        // Grab all renderers from the instantiated model
        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        // Set the default material
        SetBuildingMaterial(BusState.BUILT);
    }

    public void ChangeState(BusState newState)
    {
        if (newState == State) return;
        State = newState;
        SetBuildingMaterial(State);
    }

    private void SetBuildingMaterial(BusState newState)
    {
        if (builtMaterial == null || destroyHoverMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }

        Material targetMat = (newState == BusState.BUILT) ? builtMaterial : destroyHoverMaterial;

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
