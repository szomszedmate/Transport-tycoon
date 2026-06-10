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
    public Dictionary<ResourceEnum, List<(Industry industry, int amount)>> ResourceSources = new();
    


    public void SellResource(ResourceEnum resource, int amount)
    {
        if (Resources.ContainsKey(resource) && Resources[resource] >= amount)
        {
            Resources[resource] -= amount;
            Money += amount * 10;
            InventoryChanged?.Invoke(this, new InventoryChangedEventArgs { Resource = resource, NewAmount = -amount });

            // Industry storage csökkentése
            if (ResourceSources.ContainsKey(resource))
            {
                int remaining = amount;
                var sources = ResourceSources[resource];
                for (int i = sources.Count - 1; i >= 0 && remaining > 0; i--)
                {
                    var (industry, stored) = sources[i];
                    int toTake = Mathf.Min(stored, remaining);
                    industry.TakeResource(resource, toTake);
                    remaining -= toTake;
                    if (stored <= toTake)
                        sources.RemoveAt(i);
                    else
                        sources[i] = (industry, stored - toTake);
                }
            }
        }
        else
        {
            int current = Resources.ContainsKey(resource) ? Resources[resource] : 0;
            Debug.Log($"Nincs ennyi ebből a resource ból: {resource}, darab: " + current);
        }
    }

    public void UpdateInventory(ResourceEnum changedResource, int amount)
    {
        if (!Resources.ContainsKey(changedResource))
            Resources[changedResource] = 0;
        Resources[changedResource] += amount;
        InventoryChanged?.Invoke(this, new InventoryChangedEventArgs { Resource = changedResource, NewAmount = Resources[changedResource] });
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
        Money = 5000;
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
        if (sender is Industry industry)
        {
            if (!ResourceSources.ContainsKey(e.Resouce))
                ResourceSources[e.Resouce] = new List<(Industry, int)>();
            ResourceSources[e.Resouce].Add((industry, e.Amount));
        }
    }
}
