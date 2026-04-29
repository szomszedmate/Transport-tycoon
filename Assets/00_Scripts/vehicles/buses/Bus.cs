using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bus : VehicleBase
{
    public event System.EventHandler<CancelChargeEventArgs> CancelCharge;
    private DayPhase dayPhase;
    public bool RouteIsLinear { get; private set; }
    private List<Worker> passangers;
    private List<ILocation> locations;
    private int locationIndex;

    private const float cancelPenalty = 75;
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
        locations = new List<ILocation>();
        locationIndex = 0;
        aiAgent = GetComponent<BusAiAgent>();
        aiAgent.type = type;
        aiAgent.speed = data.Speed;
        aiAgent.Arrived += AiAgent_Arrived;
        aiAgent.Reversed += AiAgent_Reversed;
        passangers = new List<Worker>();
    }

    private void AiAgent_Reversed(object sender, EventArgs e)
    {
        locations.Reverse();
        locationIndex = 0;
    }

    private void AiAgent_Arrived(object sender, ArrivedEventArgs e)
    {
        StartCoroutine(OnArrivedAtStop(e.Stop));
    }

    public override void ResetRoute()
    {
        if (!RouteIsLinear && RouteConfirmed) // nonstop resetelese penzbe kerul
        {
            CancelCharge?.Invoke(this, new CancelChargeEventArgs { Penalty = cancelPenalty });
        }
        RouteConfirmed = false;
        aiAgent.RemoveRoute();
        Route.Clear();

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
                locations.Add(road.BusStop.location);
                Stops.Add(road.BusStop);
            }
        }
        locationIndex = 0;
        ChangeState(RouteConfirmed ? VehicleState.CONFIRMED : VehicleState.SELECTHOVER);
        if (RouteConfirmed) aiAgent.GiveRoute(Route, !linear);
    }

    public override IEnumerator OnArrivedAtStop(BusStop stop)
    {
        aiAgent.stopbusz();
        if (stop.IsCityStop()) // ha varos, fel- es leszallnak
        {
            dayPhase = stop.City.dayPhase;
            for (int i = passangers.Count-1; i >= 0; i--)
            {
                if (passangers[i].HomeCity == stop.City)
                {
                    yield return new WaitForSeconds(1f / data.LoadingSpeed); // leszall aki tud
                    stop.City.Unload(passangers[i]);
                    passangers.Remove(passangers[i]);
                }
            }

            while (passangers.Count < data.Capacity) // felszallnak
            {
                yield return new WaitForSeconds(1f / data.LoadingSpeed);
                Worker worker = stop.City.Load(dayPhase);
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
            int canAccept = stop.Industry.SpaceLeft();
            int gettingOffCount = PlanAhead();

            if (gettingOffCount > canAccept) // csak annyi szall le amennyi elfer
            {
                gettingOffCount = canAccept;
            }

            if (gettingOffCount > 0 && passangers.Count >= gettingOffCount)
            {
                // 1. Kivesszük a leszállókat egy külön listába
                List<Worker> off = passangers.GetRange(0, gettingOffCount);

                // 2. Töröljük õket az eredeti listából
                passangers.RemoveRange(0, gettingOffCount);

                stop.Industry.AddWorkers(off, RouteIsLinear, dayPhase);
            }
            int freeSpace = data.Capacity - passangers.Count;
            if (freeSpace > 0)
            {
                var homeGoers = stop.Industry.GoingHome(Stops, freeSpace);
                passangers.AddRange(homeGoers);
            }
        }
        yield return new WaitForSeconds(1f / data.LoadingSpeed);
        locationIndex++;
        aiAgent.startbusz();
        aiAgent.ProcessNextPoint();
    }

    public int PlanAhead() // elosztja egyenletesen a leszallo utasokat
    {
        int industries = 0;
        if (locations[locationIndex] is City)
        {
            return 0;
        }
        Debug.Log("# of locations: " + locations.Count);
        for (int i = locationIndex; i < locations.Count; i++)
        {
            if (locations[i] is City)
            {
                Debug.Log("breaking");
                break;
            }
            industries++;
            Debug.Log("i: " + i + ", industries: " + industries);
        }
        return (passangers.Count / industries);
    }
}
