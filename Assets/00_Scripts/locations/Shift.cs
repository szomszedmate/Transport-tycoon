using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Shift : MonoBehaviour
{
    public delegate void WorkerChangedEventHandler(object sender, WorkerChangedEventArgs e);
    public event WorkerChangedEventHandler WorkerChanged;

    private List<Worker> workers;
    public float startTime;
    public float endTime;
    public float currTime;
    public bool working;

    public float StartTime { get => startTime; set => startTime = value; }
    public float EndTime { get => endTime; set => endTime = value; }
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
        enabled = false;
    }

    public void ShiftUpdate(float gameTime)
    {
        currTime = gameTime;
        bool shouldBeWorking;

        // Éjszakai mûszak (átnyúlik a napon, pl. 22:00 -> 06:00)
        if (endTime < startTime)
        {
            shouldBeWorking = (currTime >= startTime || currTime < endTime);
        }
        else // Normál nappali mûszak
        {
            shouldBeWorking = (currTime >= startTime && currTime < endTime);
        }

        // CSAK AKKOR váltunk és küldünk eventet, ha változott az állapot
        if (shouldBeWorking && !working)
        {
            working = true;
            WorkerChanged?.Invoke(this, new WorkerChangedEventArgs { Starts = true, WorkerCount = Workers.Count });
        }
        else if (!shouldBeWorking && working)
        {
            working = false;
            WorkerChanged?.Invoke(this, new WorkerChangedEventArgs { Starts = false, WorkerCount = Workers.Count });
            enabled = false;
            // Ha nem a fix 3 fõ mûszak egyike, akkor leállítjuk
            // (A fix mûszakoknak 'enabled' kell maradniuk a következõ napra)
        }
    }

    public void RefillShift(List<Worker> workers) // csak a 3 fo shiftnek
    {
        if (this.workers.Count > 0)
        {
            Debug.LogWarning(name + "nem ures: " + workers.Count);
            return;
        }
        Debug.Log("Refilling shift...");
        this.workers = workers;
        enabled = true;
    }

    public static Shift CreateNewShift(float curr, float start, float end, List<Worker> workers)
    {
        GameObject shiftObject = new GameObject("Shift_" + start);
        Shift newShift = shiftObject.AddComponent<Shift>();

        newShift.working = false;
        newShift.StartTime = start;
        newShift.EndTime = end;
        newShift.currTime = curr;
        newShift.Workers = new List<Worker>(workers);
        return newShift;
    }
}
