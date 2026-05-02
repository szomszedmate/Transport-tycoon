using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class City : MonoBehaviour, ILocation
{
    //public event System.EventHandler<GetTimeEventArgs> GetTime;
    public List<Bus> buses;
    public Vector3 Position => transform.position;
    public StopType Type => StopType.Universal;
    [SerializeField] private CityModel model;
    [SerializeField] private float height;
    [SerializeField] private float width;
    [SerializeField] private int population;
    private DayPhase dayPhase;
    public List<Worker> DayShift { get; private set; }
    public List<Worker> EveningShift { get; private set; }
    public List<Worker> NightShift { get; private set; }
    public DayPhase DayPhase
    {
        get
        {
            return dayPhase;
        }
        set
        {
            if (value != DayPhase)
            {
                dayPhase = value;
                StartBuses();
            }
        }
    }

    void Start()
    {
        DayShift = new List<Worker>();
        EveningShift = new List<Worker>();
        NightShift = new List<Worker>();
        DivideShifts();
        DayPhase = DayPhase.NIGHT;

        buses = new();
    }

    public void StartBuses()
    {
        Debug.Log($"{this.name} város indítja a buszokat a(z) {dayPhase} mûszakhoz.");

        // Lista másolatot készítünk, mert a Resume hívás ki fogja venni a buszt a listából
        List<Bus> busesToStart = new List<Bus>(buses);
        buses.Clear();

        foreach (Bus bus in busesToStart)
        {
            bus.ResumeFromWaiting(this);
        }
    }

    public Worker Load(DayPhase phase) // parameter miatt nem valtozik felszallas kozben
    {
        Worker worker;
        switch (phase)
        {
            case DayPhase.DAY:
                if (DayShift.Count > 0)
                {
                    worker = DayShift.First();
                    DayShift.Remove(worker);
                    return worker;
                } else
                {
                    return null;
                }
            case DayPhase.EVENING:
                if (EveningShift.Count > 0)
                {
                    worker = EveningShift.First();
                    EveningShift.Remove(worker);
                    return worker;
                } else
                {
                    return null;
                }
            case DayPhase.NIGHT:
                if (NightShift.Count > 0)
                {
                    worker = NightShift.First();
                    NightShift.Remove(worker);
                    return worker;
                } else
                {
                    return null;
                }
            default:
                return null;
        }
    }

    public void Unload(Worker worker)
    {
        switch (worker.DayPhase)
        {
            case DayPhase.DAY:
                DayShift.Add(worker);
                break;
            case DayPhase.EVENING:
                EveningShift.Add(worker);
                break;
            case DayPhase.NIGHT:
                NightShift.Add(worker);
                break;
            default:
                break;
        }
    }

    public void DivideShifts()
    {
        for (int i = 0; i < population / 3; i++)
        {
            DayShift.Add(new Worker(this, DayPhase.DAY));
            EveningShift.Add(new Worker(this, DayPhase.EVENING));
            NightShift.Add(new Worker(this, DayPhase.NIGHT));
        }

        int remains = population % 3;
        if (remains > 0)
        {
            DayShift.Add(new Worker(this, DayPhase.DAY));
            remains--;
            if (remains > 0)
            {
                EveningShift.Add(new Worker(this, DayPhase.EVENING));
            }
        }
    }

    public void RegisterWaitingBus(Bus bus)
    {
        if (!buses.Contains(bus))
        {
            buses.Add(bus);
            Debug.Log($"{bus.name} várakozik a városban: {this.name}");
        }
    }

    public List<Vector3> GetAllBuildingPositions()
    {
        if (model == null)
        {
            //Debug.LogError($"City {name} has no model assigned!");
            return new List<Vector3>();
        }

        var positions = model.GetAllBuildingPositions();
        if (positions == null)
        {
            //Debug.LogError($"City {name} model returned null positions!");
            return new List<Vector3>();
        }

        //Debug.Log($"City {name} has {positions.Count} building positions.");
        return positions;
    }

}
