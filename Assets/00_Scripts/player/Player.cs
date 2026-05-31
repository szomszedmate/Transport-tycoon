using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
public class  Player : MonoBehaviour
{
    public delegate void MoneyChangedEventHandler(object sender, MoneyChangedEventArgs e);
    public event MoneyChangedEventHandler MoneyChanged;
    public delegate void TaxChangedEventHandler(object sender, TaxChangedEventArgs e);
    public event TaxChangedEventHandler TaxChanged;
    public delegate void InventoryChangedEventHandler(object sender, InventoryChangedEventArgs e);
    public event InventoryChangedEventHandler InventoryChanged;

    private double money;
    private double taxToPay;
    public List<BusData> buszok = new List<BusData>();
    public List<TruckData> trucks = new List<TruckData>();
    public Dictionary<ResourceEnum, int> Resources;
    


    public void SellResource(ResourceEnum resource, int amount)
    {
        if (Resources[resource] >=amount)
        {
            //sell
            Resources[resource] -= amount;
            Money += amount * 10;
            InventoryChanged?.Invoke(this, new InventoryChangedEventArgs { Resource = resource, NewAmount = -amount });
        }
        else
        {
            Debug.Log("Nincs ennyi ebb�l a resource b�l:"+ Resources[resource]);
        }
    }

    public void UpdateInventory(ResourceEnum changedResource, int amount)
    {
        Resources[changedResource] = amount;
        InventoryChanged?.Invoke(this, new InventoryChangedEventArgs { Resource = changedResource, NewAmount = amount });
    }

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
        Resources = new Dictionary<ResourceEnum, int>();
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

    public void Industry_Produced(object sender, ProducedEventArgs e)
    {
        UpdateInventory(e.Resouce, e.Amount);
    }
}
