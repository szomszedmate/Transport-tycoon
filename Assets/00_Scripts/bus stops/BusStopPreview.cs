using System.Collections.Generic;
using UnityEngine;
using static RoadPreview;

public class BusStopPreview : MonoBehaviour, IPreview
{
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public PreviewState State { get; private set; } = PreviewState.NEGATIVE;
    public BusStopData Data { get; private set; }
    public BusStopModel BusStopModel { get; private set; }
    IData IPreview.Data => Data;

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

    public void ChangeState(PreviewState newState)
    {
        if (newState == State) { return; }
        State = newState;
        SetPreviewMaterial(State);
    }

    public void Rotate(int degrees)
    {
        BusStopModel.Rotate(degrees);
    }

    private void SetPreviewMaterial(PreviewState newState)
    {
        Material previewMat = newState == PreviewState.POSITIVE ? positiveMaterial : negativeMaterial;
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
