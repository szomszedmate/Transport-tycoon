using System.Collections.Generic;
using UnityEngine;
using static RoadPreview;

public class BusStopPreview : MonoBehaviour, IPreview
{
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public RoadPreviewState State { get; private set; } = RoadPreviewState.NEGATIVE;
    public BusStopData Data { get; private set; }
    public BusStopModel BusStopModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void Setup(BusStopData data)
    {
        Data = data;

        BusStopModel = Instantiate(data.Model, transform);
        renderers.AddRange(BusStopModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BusStopModel.GetComponentsInChildren<Collider>());

        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        SetPreviewMaterial(State);
    }

    public void ChangeState(RoadPreviewState newState)
    {
        if (newState == State) { return; }
        State = newState;
        SetPreviewMaterial(State);
    }

    public void Rotate(int degrees)
    {
        BusStopModel.Rotate(degrees);
    }

    private void SetPreviewMaterial(RoadPreviewState newState)
    {
        Material previewMat = newState == RoadPreviewState.POSITIVE ? positiveMaterial : negativeMaterial;
        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }
            rend.materials = mats;
        }
    }
}
