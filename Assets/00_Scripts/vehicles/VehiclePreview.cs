using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class VehiclePreview : MonoBehaviour, IPreview
{
    [SerializeField] protected Material negativeMaterial;
    public PreviewState State { get; protected set; } = PreviewState.NEGATIVE;
    public VehicleData Data { get; protected set; }
    public VehicleModel Model { get; protected set; }
    IData IPreview.Data => Data;

    protected List<Renderer> renderers = new();
    protected List<Collider> colliders = new();
    private List<Material[]> originalMaterials = new();
    private List<Material[]> positiveMaterials = new(); // 50% alpha copies

    public void Setup(VehicleData data)
    {
        Data = data;
        Model = Instantiate(data.Model, transform);
        Model.transform.localPosition = Vector3.zero;
        Model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        renderers.AddRange(Model.GetComponentsInChildren<Renderer>());
        colliders.AddRange(Model.GetComponentsInChildren<Collider>());

        // Disable NavMeshAgent so it doesn't auto-rotate the preview
        foreach (var agent in Model.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
            agent.enabled = false;

        foreach (var rend in renderers)
        {
            originalMaterials.Add(rend.sharedMaterials);

            // Create semi-transparent copies for positive state
            Material[] posMats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < posMats.Length; i++)
            {
                Material copy = new Material(rend.sharedMaterials[i]);
                SetMaterialTransparent(copy, 0.5f);
                posMats[i] = copy;
            }
            positiveMaterials.Add(posMats);
        }

        foreach (var col in colliders)
            col.enabled = false;

        SetPreviewMaterial(State);
    }

    public void ChangeState(PreviewState newState)
    {
        if (newState == State) return;
        State = newState;
        SetPreviewMaterial(State);
    }

    public void Rotate(int degrees)
    {
        Model.Rotate(Model.Rotation + degrees);
    }

    protected void SetPreviewMaterial(PreviewState newState)
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] == null) continue;

            if (newState == PreviewState.POSITIVE)
            {
                if (i < positiveMaterials.Count)
                    renderers[i].materials = positiveMaterials[i];
            }
            else
            {
                // NEGATIVE: replace with negative material
                if (negativeMaterial != null)
                {
                    Material[] mats = new Material[renderers[i].sharedMaterials.Length];
                    for (int j = 0; j < mats.Length; j++)
                        mats[j] = negativeMaterial;
                    renderers[i].materials = mats;
                }
                else if (i < originalMaterials.Count)
                {
                    renderers[i].materials = originalMaterials[i];
                }
            }
        }
    }

    private void SetMaterialTransparent(Material mat, float alpha)
    {
        // URP Lit shader surface type transparent
        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f); // 1 = Transparent

        if (mat.HasProperty("_Blend"))
            mat.SetFloat("_Blend", 0f); // Alpha blend

        // Set alpha on base color
        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
        else if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");
            c.a = alpha;
            mat.SetColor("_Color", c);
        }

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private void OnDestroy()
    {
        // Clean up instantiated positive material copies
        foreach (var mats in positiveMaterials)
            foreach (var mat in mats)
                if (mat != null) Destroy(mat);
    }
}
