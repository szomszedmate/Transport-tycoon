using UnityEngine;

public class Worker
{
    private City homeCity;

    public City HomeCity { get => homeCity; private set => homeCity = value; }
    public DayPhase DayPhase { get => dayPhase; private set => dayPhase = value; }

    private DayPhase dayPhase;
}
