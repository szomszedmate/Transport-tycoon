using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bus : VehicleBase
{
    public bool RouteIsLinear { get; private set; }
    private List<Worker> passangers;
    public BusType BusType
    {
        get
        {
            if (model is BusBasicModel) return BusType.BASIC;
            if (model is BusAdvancedModel) return BusType.ADVANCED;
            if (model is BusPremiumModel) return BusType.PREMIUM;
            return BusType.UNKNOWN;
        }
    }

    public override BuildCategory BuildCategory
    {
        get
        {
            return BuildCategory.BUS;
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

    public void Setup(BusData data, float rotation)
    {
        this.data = data;
        type = data.Type;
        materials = GameObject.FindGameObjectWithTag("manager").GetComponent<InventoryManager>().materials;

        NonStop = false;
        lastPosition = transform.position;
        WeeklyMileage = 0;
        // Instantiate the actual model first
        model = Instantiate(data.Model, transform.position, Quaternion.Euler(-90, 0, rotation), transform);
        //model.transform.localPosition = Vector3.zero;
        //model.Rotate(rotation);
        //model = Instantiate(data.Model, transform);
        //model.transform.localPosition = Vector3.zero;
        //model.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        // Grab all renderers from the instantiated model
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
        // Set the default material
        SetMaterial(VehicleState.BUILT);
        Route = new List<Road>();
        aiAgent = GetComponent<BusAiAgent>();
        aiAgent.type = type;
        aiAgent.speed = data.Speed;
        aiAgent.Arrived += AiAgent_Arrived;
        passangers = new List<Worker>();
    }

    private void AiAgent_Arrived(object sender, ArrivedEventArgs e)
    {
        StartCoroutine(OnArrivedAtStop(e.Stop));
    }

    public void ConfirmRoute(bool linear)
    {
        if (!(Route.Last().Road_HasBusStop()) && linear)
        {
            Debug.Log("Cant confirm");
            return;
        }
        Stops.Clear();
        RouteConfirmed = !RouteConfirmed;
        RouteIsLinear = linear;
        NonStop = !linear;
        foreach (Road road in Route)
        {
            road.ChangeState(RouteConfirmed ? RoadState.CONFIRMED : RoadState.SELECTED);
            if (road.Road_HasBusStop())
            {
                Stops.Add(road.BusStop);
            }
        }

        ChangeState(RouteConfirmed ? VehicleState.CONFIRMED : VehicleState.SELECTHOVER);
        if (RouteConfirmed) aiAgent.GiveRoute(Route);
    }

    public override IEnumerator OnArrivedAtStop(BusStop stop)
    {
        aiAgent.stopbusz();
        if (stop.IsCityStop()) // ha varos, felszallnak
        {
            while (passangers.Count < data.Capacity)
            {
                yield return new WaitForSeconds(1f / data.LoadingSpeed);
                Worker worker = stop.City.Load();
                if (worker != null)
                {
                    passangers.Add(worker);
                    //Debug.Log("Loading bus");
                } else 
                {
                    Debug.Log("Couldnt load bus");
                    break; // break ha ures a varos
                }
            }
        }
        else // ha industry, leszallnak
        {
            if (passangers.Count > 0)
            {
                stop.Industry.AddWorkers(new List<Worker>(passangers));
                passangers.Clear();

                Debug.Log($"Mindenki leszállt ide: {stop.Industry.name}");
            }
        }
        aiAgent.startbusz();
    }
}
