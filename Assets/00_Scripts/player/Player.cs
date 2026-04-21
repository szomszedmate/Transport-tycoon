using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
public class Player : MonoBehaviour
{
    public delegate void MoneyChangedEventHandler(object sender, MoneyChangedEventArgs e);
    public event MoneyChangedEventHandler MoneyChanged;
    public delegate void TaxChangedEventHandler(object sender, TaxChangedEventArgs e);
    public event TaxChangedEventHandler TaxChanged;
    private double money;
    private double taxToPay;
    public List<BusData> buszok = new List<BusData>();
    public List<TruckData> trucks = new List<TruckData>();
    public double TaxToPay
    {
        get
        {
            return taxToPay;
        }
        set
        {
            taxToPay = value;
            TaxChanged?.Invoke(this, new TaxChangedEventArgs { NewAmount = TaxToPay });
        }
    }
    public int NextTaxDay {  get; private set; }

    public double Money
    {
        get
        {
            return money;
        } 
        private set
        {
            money = value;
            MoneyChanged?.Invoke(this, new MoneyChangedEventArgs { NewAmount = money });
        }
    }

    public void Start()
    {
        Money = 500;
        TaxToPay = 0;
        NextTaxDay = 7;
    }

    public void AddMoney(double amount)
    {
        Money += amount;
    }
    public void LoseMoney(double amount)
    {
        Money -= amount;
    }

    public bool CanAfford(double amount)
    {
        return Money >= amount;
    }

    public void IncreaseTax(double amount)
    {
        TaxToPay += amount;
        TaxChanged?.Invoke(this, new TaxChangedEventArgs { NewAmount = TaxToPay });
    }
    public void DecreseTax(double amount)
    {
        TaxToPay -= amount;
        TaxChanged?.Invoke(this, new TaxChangedEventArgs { NewAmount = TaxToPay });
    }

    public void PayTaxes()
    {
        LoseMoney(TaxToPay);
        TaxToPay = 0;
        NextTaxDay += 7;
    }
}
