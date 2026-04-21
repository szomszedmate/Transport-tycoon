using UnityEngine;
using System;

public class Game : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BuildingSystem buildingSystem;
    private float globalTime;
    private float gameTime;
    private int day;
    
    public float TimeMultiplier {  get; private set; }
    public Player Player { get => player; private set => player = value; }
    public InputManager InputManager { get => inputManager; private set => inputManager = value; }

    public delegate void TimeChangedEventHandler(object sender, TimeChangedEventArgs e);
    public event TimeChangedEventHandler TimeChanged;

    public void Start()
    {
        globalTime = 0;
        gameTime = 0;
        day = 1;
        TimeMultiplier = 5144; // 1 day = 10 irl minutes (remove the 5)

        if (buildingSystem != null)
        {
            buildingSystem.BuyRequest += BuildingSystem_BuyRequest;
            buildingSystem.AnyBusMileageChanged += BuildingSystem_AnyBusMileageChanged;
        }
    }

    private void BuildingSystem_AnyBusMileageChanged(object sender, MileageChangedEventArgs e)
    {
        double dist = e.NewAmount;
        bool nonStop = e.NonStop;
        double cost;
        if (nonStop) // nonstop sokkal dragabb
        {
            cost = dist * 0.15;
            player.IncreaseTax(cost);
        } else
        {
            cost = dist * 0.05;
            player.IncreaseTax(cost);
        }
    }

    private void BuildingSystem_BuyRequest(object sender, BuyRequestEventArgs e)
    {
        if (e.Deduct)
        {
            if (Player.CanAfford(e.Cost))
            {
                Player.LoseMoney(e.Cost);
                //Debug.Log("Remaining money: " + player.Money);
            } else
            {
                Debug.LogWarning("Insufficient funds, ramaining money: " + Player.Money + " (need " + e.Cost + " )");
            }
        }
        if (Player.CanAfford(e.Cost))
        {
            e.IsApproved = true;
        }
        else
        {
            e.IsApproved = false;
        }
    }

    public void Update()
    {
        globalTime += Time.deltaTime * TimeMultiplier;
        gameTime += Time.deltaTime * TimeMultiplier;
        ConvertTime();
        TimeChanged?.Invoke(this, new TimeChangedEventArgs { NewTime = gameTime, Day = day });
        
    }

    public void ConvertTime()
    {
        if (gameTime >= 86400)
        {
            gameTime -= 86400; // 24 hour format
            day++;

            if (player != null)
            {
                if (day == player.NextTaxDay)
                {
                    player.PayTaxes();
                }
            }
            else
            {
                Debug.LogError("Game: A Player referencia NULL! Ellenõrizd az Inspectort!");
            }
        }
    }
}
