using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RoadPreview : MonoBehaviour, IPreview
{
    public enum RoadPreviewState
    {
        POSITIVE,
        NEGATIVE
    }
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public RoadPreviewState State { get; private set; } = RoadPreviewState.NEGATIVE;
    public RoadData Data { get; private set; }
    public RoadModel RoadModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();

    public void Setup(RoadData data)
    {
        Data = data;
        RoadModel = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        RoadModel.transform.localPosition = new Vector3(0, 0.01f, 0); // move higher so no z fighting
        renderers.AddRange(RoadModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(RoadModel.GetComponentsInChildren<Collider>());
        
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        SetPreviewMaterial(State);
    }

    public void ChangeState(RoadPreviewState newState)
    {
        if (newState == State ) { return; }
        State = newState;
        SetPreviewMaterial(State);
    }
    public void Rotate(int degrees)
    {
        RoadModel.Rotate(degrees);
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
