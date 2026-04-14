using UnityEngine;

public class Player : MonoBehaviour
{
    private float money;
    private float taxToPay;

    public float Money { get => money; set => money = value; }

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
