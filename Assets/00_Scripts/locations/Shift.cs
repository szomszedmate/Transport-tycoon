using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Shift : MonoBehaviour
{
    public delegate void WorkerChangedEventHandler(object sender, WorkerChangedEventArgs e);
    public event WorkerChangedEventHandler WorkerChanged;

    private List<Worker> workers;
    public double startTime;
    public double endTime;
    public double currTime;
    public bool working;

    public double StartTime { get => startTime; set => startTime = value; }
    public double EndTime { get => endTime; set => endTime = value; }
    public List<Worker> Workers { get => workers; private set => workers = value; }

    public void Start()
    {

    }

    public void StartShift()
    {
        working = true;
    }

    public void StopShift()
    {
        working = false;
    }

    public void ShiftUpdate(double globalTime)
    {
        currTime = globalTime;
        if (currTime >= endTime && working) // munkaido lejart
        {
            working = false;
            WorkerChanged?.Invoke(this, new WorkerChangedEventArgs { Starts = false, WorkerCount = Workers.Count }); // n munkas vegzett
        }
        else if (currTime >= startTime && !working)
        { // munkaido kezdodik
            {
                working = true;
                WorkerChanged?.Invoke(this, new WorkerChangedEventArgs { Starts = true, WorkerCount = Workers.Count }); // n munkas kezd
            }
        }
    }

    public static Shift CreateNewShift(double curr, double start, double end, List<Worker> workers)
    {
        GameObject shiftObject = new GameObject("Shift_" + start);
        Shift newShift = shiftObject.AddComponent<Shift>();

        newShift.StartTime = start;
        newShift.EndTime = end;
        newShift.currTime = curr;
        newShift.Workers = new List<Worker>(workers);
        return newShift;
    }
}
