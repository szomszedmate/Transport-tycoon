using System;
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