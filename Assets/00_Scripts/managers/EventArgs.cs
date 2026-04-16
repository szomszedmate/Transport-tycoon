using System;
using UnityEngine;

public class BuyRequestEventArgs : EventArgs
{
    public float Cost {  get; set; }
    public bool Deduct { get; set; }
    public bool IsApproved { get; set; }
}

public class MoneyChangedEventArgs : EventArgs
{
    public float NewAmount { get; set; }
}