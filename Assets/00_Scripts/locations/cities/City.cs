using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class City : MonoBehaviour, ILocation
{
    public Vector3 Position => transform.position;
    [SerializeField] private CityModel model;
    [SerializeField] private float height;
    [SerializeField] private float width;
    [SerializeField] private int population;

    public int DayShift { get; private set; }
    public int EveningShift { get; private set; }
    public int NightShift { get; private set; }

    public void Awake()
    {
        DivideShifts();
    }



    public bool Unload(Shift shift)
    {
        switch (shift)
        {
            case Shift.DAYSHIFT:
                if (DayShift > 0)
                {
                    DayShift--;
                } else
                {
                    return false;
                }
                break;
            case Shift.EVENINGSHIFT:
                if (EveningShift > 0)
                {
                    EveningShift--;
                } else
                {
                    return false;
                }
                break;
            case Shift.NIGHTSHIFT:
                if (NightShift > 0)
                {
                    NightShift--;
                } else
                {
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    public void Load(Shift shift)
    {
        switch (shift)
        {
            case Shift.DAYSHIFT:
                DayShift++;
                break;
            case Shift.EVENINGSHIFT:
                EveningShift++;
                break;
            case Shift.NIGHTSHIFT:
                NightShift++;
                break;
            default:
                break;
        }
    }

    public void DivideShifts()
    {
        DayShift = population / 3;
        EveningShift = population / 3;
        NightShift = population / 3;

        int remains = population % 3;
        if (remains > 0)
        {
            DayShift++;
            remains--;
            if (remains > 0)
            {
                EveningShift++;
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

       // Debug.Log($"City {name} has {positions.Count} building positions.");
        return positions;
    }

}
