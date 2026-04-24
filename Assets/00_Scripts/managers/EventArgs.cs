using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuyRequestEventArgs : EventArgs
{
    public double Cost {  get; set; }
    public bool Deduct { get; set; }
    public bool IsApproved { get; set; }
}

public class MoneyChangedEventArgs : EventArgs
{
    public double NewAmount { get; set; }
}

public class MileageChangedEventArgs : EventArgs
{
    public double NewAmount {  set; get; }
    public bool NonStop { get; set; }
}

public class TaxChangedEventArgs : EventArgs
{
    public double NewAmount { set; get; }
}

public class TimeChangedEventArgs : EventArgs
{
    public double NewTime { set; get; }
    public int Day {  get; set; }
}

public class ArrivedEventArgs : EventArgs
{
    public BusStop Stop { get; set; }
}

public class GetTimeEventArgs : EventArgs
{
    public float Time { get; set; }
}

public class WorkerChangedEventArgs : EventArgs
{
    public bool Starts {  get; set; }
    public int WorkerCount { get; set; }
}

public class LocationsRegisteredEventArgs : EventArgs
{
    public List<ILocation> RegisteredLocations { get; set; }
}

public class InventoryChangedEventArgs : EventArgs
{
    public ResourceEnum Resource { get; set; }
    public int NewAmount { get; set; }
}

public class ProducedEventArgs : EventArgs
{
    public ResourceEnum Resouce { get; set; }
    public int Amount { get; set; }
}