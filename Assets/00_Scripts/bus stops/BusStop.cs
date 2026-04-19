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

    public StopType Type = StopType.None;

    public int Cost => data.Cost;
    private BusStopModel model;
    private BusStopData data;
    private List<Material> materials;
    public BusStopState State { get; private set; }

    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;
    private List<Renderer> renderers = new();


    public BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.BUSSTOP;
        }
    }

    public void SetUp(BusStopData data, float rotation)
    {
        this.data = data;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;
        model = Instantiate(data.Model, transform.position, Quaternion.Euler(0, 0, rotation), transform );

        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        switch (Type)
        {
            case StopType.None:
                break;
            case StopType.Bus:
                builtMaterial = materials[0];
                break;
            case StopType.Universal:
                builtMaterial = materials[9];
                break;
            case StopType.Coal:
                builtMaterial = materials[3];
                break;
            case StopType.IronOre:
                builtMaterial = materials[7];
                break;
            case StopType.GoldOre:
                builtMaterial = materials[6];
                break;
            case StopType.Flour:
                builtMaterial = materials[5];
                break;
            case StopType.Water:
                builtMaterial = materials[1];
                break;
            case StopType.Farm:
                builtMaterial = materials[4];
                break;
            case StopType.IronBar:
                builtMaterial = materials[7];
                break;
            case StopType.GoldBar:
                builtMaterial = materials[6];
                break;
            case StopType.Mint:
                builtMaterial = materials[8];
                break;
            case StopType.Bakery:
                builtMaterial = materials[2];
                break;
            default:
                builtMaterial = materials[0];
                break;
        }

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
}
