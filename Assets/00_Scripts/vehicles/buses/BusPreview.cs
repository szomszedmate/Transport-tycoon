using System.Collections.Generic;
using UnityEngine;
using static RoadPreview;

public class BusPreview : MonoBehaviour, IPreview
{
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public PreviewState State { get; private set; } = PreviewState.NEGATIVE;
    public BusData Data { get; private set; }
    public BusModel BusModel { get; private set; }
    IData IPreview.Data => Data;

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

    public void ChangeState(PreviewState newState)
    {
        if (newState == State) { return; }
        State = newState;
        SetPreviewMaterial(State);
    }
    public void Rotate(int degrees)
    {
        BusModel.Rotate(degrees);
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
