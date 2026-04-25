using UnityEngine;

[System.Serializable]
public class Worker
{
    private City homeCity;

    public City HomeCity { get => homeCity; set => homeCity = value; }
    public DayPhase DayPhase { get => dayPhase; set => dayPhase = value; }

    private DayPhase dayPhase;

    public Worker(City homeCity, DayPhase dayPhase)
    {
        this.homeCity = homeCity;
        this.dayPhase = dayPhase;
    }
}
