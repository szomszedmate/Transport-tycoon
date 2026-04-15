using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public delegate void MoneyChangedEventHandler(object sender, MoneyChangedEventArgs e);
    public event MoneyChangedEventHandler MoneyChanged;
    private float money;
    private float taxToPay;

    public float Money
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
        taxToPay = 0;
    }

    public void AddMoney(float amount)
    {
        Money += amount;
    }
    public void LoseMoney(float amount)
    {
        Money -= amount;
    }

    public bool CanAfford(float amount)
    {
        return Money >= amount;
    }

    public void IncreaseTax(float amount)
    {
        taxToPay += amount;
    }
    public float DecreseTax(float amount)
    {
        return taxToPay -= amount;
    }
}
