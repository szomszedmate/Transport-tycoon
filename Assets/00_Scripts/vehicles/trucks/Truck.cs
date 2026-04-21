using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Truck : VehicleBase
{
    public TruckType TruckType
    {
        get
        {
            if (model is FarmTruck) return TruckType.FARM;
            if (model is FlourTruck) return TruckType.FLOUR;
            if (model is WaterTruck) return TruckType.WATER;
            if (model is BakeryTruck) return TruckType.BAKERY;
            if (model is CoalTruck) return TruckType.COAL;
            if (model is IronTruck) return TruckType.IRON;
            if (model is GoldTruck) return TruckType.GOLD;
            if (model is MintTruck) return TruckType.MINT;
            return TruckType.UNKNOWN;
        }
    }

    public override BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.TRUCK;
        }
    }

    protected override void Update()
    {
        float distance = Math.Abs(Vector3.Distance(transform.position, lastPosition));

        if (distance > 0)
        {
            OnMileageChanged(distance, this.NonStop); // Ez mûködni fog!

            WeeklyMileage += distance;
            lastPosition = transform.position;
        }
    }

    public void Setup(TruckData data, float rotation)
    {
        this.data = data;
        type = data.Type;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;

        lastPosition = transform.position;
        WeeklyMileage = 0;

        model = Instantiate(data.Model, transform.position, Quaternion.Euler(-90, 0, rotation), transform);
        
        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        //buildmaterials in player inventori in managers
        switch (type)
        {
            case StopType.None:
                break;
            case StopType.Bus:
                builtMaterial = materials[0];
                break;
            case StopType.Universal:
                builtMaterial = materials[0];
                break;
            case StopType.Coal:
                builtMaterial = vehicleMaterials.Materials[3];
                break;
            case StopType.IronOre:
                builtMaterial = vehicleMaterials.Materials[11];
                break;
            case StopType.GoldOre:
                builtMaterial = vehicleMaterials.Materials[13];
                break;
            case StopType.Flour:
                builtMaterial = vehicleMaterials.Materials[7];
                break;
            case StopType.Water:
                builtMaterial = vehicleMaterials.Materials[15];
                break;
            case StopType.Farm:
                builtMaterial = vehicleMaterials.Materials[5];
                break;
            case StopType.IronBar:
                builtMaterial = vehicleMaterials.Materials[11];
                break;
            case StopType.GoldBar:
                builtMaterial = vehicleMaterials.Materials[9];
                break;
            case StopType.Mint:
                builtMaterial = vehicleMaterials.Materials[13];
                break;
            case StopType.Bakery:
                builtMaterial = vehicleMaterials.Materials[1];
                break;
            default:
                builtMaterial = materials[0];
                break;
        }
        // Set the default material
        SetMaterial(VehicleState.BUILT);
        Route = new List<Road>();
        aiAgent = GetComponent<BusAiAgent>();
        aiAgent.type = type;
        aiAgent.speed = data.Speed;
        aiAgent.Arrived += AiAgent_Arrived;
    }

    public void ConfirmRoute()
    {
        if (!(Route.Last().Road_HasBusStop()))
        {
            Debug.Log("Cant confirm, last road needs bus stop");
            return;
        }
        RouteConfirmed = !RouteConfirmed;
        foreach (Road road in Route)
        {
            road.ChangeState(RouteConfirmed ? RoadState.CONFIRMED : RoadState.SELECTED);
        }

        ChangeState(RouteConfirmed ? VehicleState.CONFIRMED : VehicleState.SELECTHOVER);
        if (RouteConfirmed) aiAgent.GiveRoute(Route);
    }

    private void AiAgent_Arrived(object sender, ArrivedEventArgs e)
    {
        StartCoroutine(OnArrivedAtStop(e.Stop));
    }

    public override IEnumerator OnArrivedAtStop(BusStop stop)
    {
        aiAgent.stopbusz();
        Debug.Log("Arrived, load: " + currentLoad + ", capacity: " + data.Capacity);
        Debug.Log(stop.IsCityStop());
        if (!stop.IsCityStop())
        {
            int accepted = stop.Industry.Accept(((TruckData)data).Resource, 1);
            for (int i = 0; i < accepted; i++)
            {
                yield return new WaitForSeconds(1f / data.LoadingSpeed);
                if (currentLoad > 0)
                {
                    currentLoad--;
                }
            }
            int pickedUp = stop.Industry.Pickup(((TruckData)data).Resource, 1);
            for (int i = 0;i < pickedUp; i++)
            {
                yield return new WaitForSeconds(1f / data.LoadingSpeed);
                currentLoad++;
            }
        }
        aiAgent.startbusz();
    }
}
