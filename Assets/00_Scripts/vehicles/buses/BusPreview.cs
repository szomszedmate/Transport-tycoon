using System.Collections.Generic;
using UnityEngine;
using static RoadPreview;

public class BusPreview : MonoBehaviour, IPreview
{
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public RoadPreviewState State { get; private set; } = RoadPreviewState.NEGATIVE;
    public BusData Data { get; private set; }
    public BusModel BusModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void Setup(BusData data)
    {
        Data = data;
        BusModel = Instantiate(data.Model, transform);
        BusModel.transform.localPosition = Vector3.zero;
        BusModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        renderers.AddRange(BusModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BusModel.GetComponentsInChildren<Collider>());

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
        BusModel.Rotate(degrees);
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
