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
    public BusStopState State { get; private set; }

    [SerializeField]
    private Material builtMaterial;
    [SerializeField]
    private Material destroyHoverMaterial;

  

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

        model = Instantiate(data.Model, transform.position, Quaternion.Euler(0, 0, rotation), transform );
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
    }
}
