using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Truck : VehicleBase
{
    [SerializeField] protected int minimumLoad;
    protected int currentLoad;
    protected bool canCheckInventory = false;
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
            lastPosition = transform.position;
        }

        model.AdjustVisualPosition();
    }

    public void Setup(TruckData data, float rotation, LayerMask terrainLayer, BuildingGrid grid)
    {
        this.data = data;
        mainType = data.MainType;
        types = data.Types;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;

        lastPosition = transform.position;
        WeeklyMileage = 0;

        model = Instantiate(data.Model, transform.position, Quaternion.Euler(0, rotation, 0), transform);
        model.grid = grid;

        renderers.Clear();
        renderers.AddRange(model.GetComponentsInChildren<Renderer>());

        //buildmaterials in player inventori in managers
        //switch (mainType)
        //{
        //    case StopType.None:
        //        break;
        //    case StopType.Bus:
        //        //builtMaterial = materials[0];
        //        break;
        //    case StopType.Universal:
        //        builtMaterial = materials[0];
        //        break;
        //    case StopType.Coal:
        //        builtMaterial = vehicleMaterials.Materials[3];
        //        break;
        //    case StopType.IronOre:
        //        builtMaterial = vehicleMaterials.Materials[11];
        //        break;
        //    case StopType.GoldOre:
        //        builtMaterial = vehicleMaterials.Materials[13];
        //        break;
        //    case StopType.Flour:
        //        builtMaterial = vehicleMaterials.Materials[7];
        //        break;
        //    case StopType.Water:
        //        builtMaterial = vehicleMaterials.Materials[15];
        //        break;
        //    case StopType.Farm:
        //        builtMaterial = vehicleMaterials.Materials[5];
        //        break;
        //    case StopType.IronBar:
        //        builtMaterial = vehicleMaterials.Materials[11];
        //        break;
        //    case StopType.GoldBar:
        //        builtMaterial = vehicleMaterials.Materials[9];
        //        break;
        //    case StopType.Mint:
        //        builtMaterial = vehicleMaterials.Materials[13];
        //        break;
        //    case StopType.Bakery:
        //        builtMaterial = vehicleMaterials.Materials[1];
        //        break;
        //    default:
        //        builtMaterial = materials[0];
        //        break;
        //}
        // Set the default material
        SetMaterial(VehicleState.BUILT);
        Route = new List<Road>();
        aiAgent = GetComponent<BusAiAgent>();
        aiAgent.types = types;
        aiAgent.mainType = mainType;
        aiAgent.speed = data.Speed;
        aiAgent.Arrived += AiAgent_Arrived;
        currentLoad = 0;

        model.CalculateOffsets();
        model.terrainLayer = terrainLayer;
        model.AdjustVisualPosition();
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
        if (RouteConfirmed) aiAgent.GiveRoute(Route, false); // truck mindig linear
    }

    private void AiAgent_Arrived(object sender, ArrivedEventArgs e)
    {
        StartCoroutine(OnArrivedAtStop(e.Stop));
    }

    public override IEnumerator OnArrivedAtStop(BusStop stop)
    {
        aiAgent.stopbusz();
        TruckData truckData = (TruckData)data;
        canCheckInventory = true;

        if (!stop.IsCityStop())
        {
            int canAccept = stop.Industry.Accept(truckData.Resource, currentLoad);
            for (int i = 0; i < canAccept; i++) 
            {
                // V�runk a rakod�si sebess�gnek megfelel�en
                yield return new WaitForSeconds(1f / data.LoadingSpeed);
                currentLoad--;
            }

            if (stop.Industry.Produces(truckData.Resource))
            {
                canCheckInventory = true;
                stop.Industry.Produced += Industry_Produced;
                while (currentLoad < minimumLoad)
                { 
                    // Megv�rjuk, am�g az esem�ny (vagy az �rkez�s) azt mondja: "van mi�rt n�zel�dni"
                    yield return new WaitUntil(() => canCheckInventory);
                    canCheckInventory = false;

                    int spaceLeft = truckData.Capacity - currentLoad;
                    int pickedUp = stop.Industry.Pickup(truckData.Resource, spaceLeft);
                    if (pickedUp > 0)
                    {
                        for (int i = 0; i < pickedUp; i++)
                        {
                            yield return new WaitForSeconds(1f / data.LoadingSpeed);
                            currentLoad++;
                        }
                        // Ha a rakod�s ut�n m�g mindig nem �rt�k el a minimumot, 
                        // de maradt m�g a rakt�rban, akkor ne v�rjunk �jabb eventre
                        if (currentLoad < minimumLoad)
                        {
                            // Itt egy gyors csekk: h�tha maradt m�g az Industry-n�l �ru
                            canCheckInventory = true;
                        }
                    }
                }
                stop.Industry.Produced -= Industry_Produced;
            }
        }
        aiAgent.startbusz();
        aiAgent.ProcessNextPoint();
    }

    private void Industry_Produced(object sender, ProducedEventArgs e)
    {
        canCheckInventory = true;
    }
}
