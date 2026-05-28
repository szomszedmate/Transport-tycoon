using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RoadPreview : MonoBehaviour, IPreview
{
    [SerializeField]
    private Material positiveMaterial;
    [SerializeField]
    private Material negativeMaterial;
    public PreviewState State { get; private set; } = PreviewState.NEGATIVE;
    public RoadData Data { get; set; }
    public RoadModel RoadModel { get; set; }
    IData IPreview.Data => Data;

    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();
    public event System.EventHandler<PreviewStateChangedEventArgs> PreviewStateChanged;

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

    public void ChangeState(PreviewState newState)
    {
        if (newState == State ) { return; }
        State = newState;
        SetPreviewMaterial(State);
        //PreviewStateChanged?.Invoke(this, new PreviewStateChangedEventArgs { NewState = newState, RoadType = RoadModel.RoadType, Rotation = RoadModel.Rotation, SnappedPosition = transform.position });
    }
    public void Rotate(int degrees)
    {
        RoadModel.Rotate(degrees);
    }

    private void SetPreviewMaterial(PreviewState newState)
    {
        Material previewMat = newState == PreviewState.POSITIVE ? positiveMaterial : negativeMaterial;
        foreach (var rend in renderers)
        {
            if (rend.gameObject.TryGetComponent<UnityEngine.VFX.VisualEffect>(out _))
            {
                continue; // Ha ez egy VFX objektum, ne nyúljunk a materiáltömbhöz
            }
            if (rend.GetType().Name.Contains("VFX"))
            {
                continue;
            }

            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }
            rend.materials = mats;
        }
    }
}
