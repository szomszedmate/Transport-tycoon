using UnityEngine;
using System;

public class Game : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BuildingSystem buildingSystem;
    private float globalTime;
    private float gameTime;
    
    public float TimeMultiplier {  get; private set; }

    public void Start()
    {
        globalTime = 0;
        gameTime = 0;
        TimeMultiplier = 144; // 1 day = 10 irl minutes

        if (buildingSystem != null)
        {
            buildingSystem.BuyRequest += BuildingSystem_BuyRequest;
        }
    }

    private void BuildingSystem_BuyRequest(object sender, BuyRequestEventArgs e)
    {
        if (e.Deduct)
        {
            if (player.CanAfford(e.Cost))
            {
                player.LoseMoney(e.Cost);
                //Debug.Log("Remaining money: " + player.Money);
            } else
            {
                Debug.Log("Insufficient funds, ramaining money: " + player.Money + " (need " + e.Cost + " )");
            }
        }
        if (player.CanAfford(e.Cost))
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
        ConvertTime();
    }

    public void ConvertTime()
    {
        if (gameTime >= 86400) gameTime -= 86400; // 24 hour format
    }
}
