using UnityEngine;
using System.Collections.Generic;

public class VehiclePreview : MonoBehaviour, IPreview
{
    protected Material positiveMaterial;
    protected Material negativeMaterial;
    public PreviewState State { get; protected set; } = PreviewState.NEGATIVE;
    public VehicleData Data { get; protected set; }
    public VehicleModel Model { get; protected set; }
    IData IPreview.Data => Data;

    protected List<Renderer> renderers = new();
    protected List<Collider> colliders = new();

    public void Setup(VehicleData data)
    {
        Data = data;
        Model = Instantiate(data.Model, transform);
        Model.transform.localPosition = Vector3.zero;
        Model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        renderers.AddRange(Model.GetComponentsInChildren<Renderer>());
        colliders.AddRange(Model.GetComponentsInChildren<Collider>());

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
        Model.Rotate(degrees);
    }

    protected void SetPreviewMaterial(PreviewState newState)
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
