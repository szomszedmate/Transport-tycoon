using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class City : MonoBehaviour, ILocation
{
    public event System.EventHandler<GetTimeEventArgs> GetTime;

    public Vector3 Position => transform.position;
    public StopType Type => StopType.Universal;
    [SerializeField] private CityModel model;
    [SerializeField] private float height;
    [SerializeField] private float width;
    [SerializeField] private int population;
    public DayPhase dayPhase;
    public List<Worker> DayShift { get; private set; }
    public List<Worker> EveningShift { get; private set; }
    public List<Worker> NightShift { get; private set; }


    void Start()
    {
        DayShift = new List<Worker>();
        EveningShift = new List<Worker>();
        NightShift = new List<Worker>();
        DivideShifts();
        dayPhase = DayPhase.NIGHT;
    }

    public Worker Load(DayPhase phase) // parameter miatt nem valtozik felszallas kozben
    {
        switch (phase)
        {
            case DayPhase.DAY:
                if (DayShift.Count > 0)
                {
                    DayShift.RemoveAt(0);
                    return (DayShift.First());
                } else
                {
                    return null;
                }
            case DayPhase.EVENING:
                if (EveningShift.Count > 0)
                {
                    EveningShift.RemoveAt(0);
                    return(EveningShift.First());
                } else
                {
                    return null;
                }
            case DayPhase.NIGHT:
                if (NightShift.Count > 0)
                {
                    NightShift.RemoveAt(0);
                    return (NightShift.First());
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
