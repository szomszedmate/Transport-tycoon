using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

using System;
using static Bus;
public enum StopType
{
    None,
    Bus,
    Universal,
    Coal,
    IronOre,
    GoldOre,
    Flour,
    Water,
    Farm,
    IronBar,
    GoldBar,
    Mint,
    Bakery
}



public class BusStop : MonoBehaviour, IBuildable
{
    public enum BusStopState
    {
        BUILT,
        DESTROYHOVER
    }

    [Header("Settings")]
    public StopType Type = StopType.None;

    [Header("Materials")]
    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;

    public int Cost => data.Cost;
    public BusStopModel model;
    public BusStopData data;
    private List<Material> materials;
    public ILocation location;
    public BusStopState State { get; private set; }
    private List<Renderer> renderers = new();
    private City city;
    private Industry industry;

    public BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.BUSSTOP;
        }
    }

    public City City { get => city; set => city = value; }
    public Industry Industry { get => industry; set => industry = value; }

    public bool IsCityStop()
    {
        return (location is City);
    }

    public void SetUp(BusStopData data, float rotation, ILocation location)
    {
        this.data = data;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;
        model = Instantiate(data.Model, transform.position, Quaternion.Euler(0, 0, rotation), transform );
        this.location = location;
        if (location is City city) this.city = city;
        else if (location is Industry industry) this.industry = industry;

        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        if (builtMaterial == null)
            Debug.LogError($"BusStop {name}: builtMaterial is not assigned in Inspector!", this);

        SetBusStateMaterial(BusStopState.BUILT);

    }

    public void SetBusStateMaterial(BusStopState newState)
    {
        if (builtMaterial == null || destroyHoverMaterial == null)
        {
            Debug.LogWarning("Materials not assigned!");
            return;
        }



        Material targetMat = (newState == BusStopState.BUILT) ? builtMaterial : destroyHoverMaterial;

        switch (newState)
        {
            case BusStopState.BUILT:
                targetMat = builtMaterial;
                break;
            case BusStopState.DESTROYHOVER:
                targetMat = destroyHoverMaterial;
                break;
            default:
                targetMat = builtMaterial;
                break;
        }
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
    #region Terrain

    #endregion
}
