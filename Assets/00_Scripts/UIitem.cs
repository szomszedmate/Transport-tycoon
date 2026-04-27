using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class UIitem : MonoBehaviour
{
    public Player inventory;
    public GameUiFunctions uiFunctions;
    public VehicleData vehicle;
    public BuildingSystem buildingSystem;
    public bool isplaced = false;
    public TMP_Text itemtext;
    public Button placebutton;
    public GameObject invmenu;
    public GameObject routebuton;
    public TMP_Text placebuttontext;
    public VehicleBase vehicleObject;
    public StopType type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory= GameObject.FindWithTag("Player").GetComponent<Player>();
        uiFunctions= GameObject.FindWithTag("UI").GetComponent<GameUiFunctions>();
        buildingSystem= GameObject.FindWithTag("buildingsys").GetComponent<BuildingSystem>();
        invmenu = GameObject.FindWithTag("invmenu");
        
        type = vehicle.Type;
        uiFunctions.invopened += invopenedcheck;

        string typestring= type.ToString();
        switch (type)
        {
            case StopType.None:
                break;
            case StopType.Bus:
                typestring = "Bus";
                break;
            case StopType.Universal:
                typestring = "Bus";
                break;
            case StopType.Coal:
                typestring = "Coal Truck";
                break;
            case StopType.IronOre:
                typestring = "Iron Truck";
                break;
            case StopType.GoldOre:
                typestring = "Gold Truck";
                break;
            case StopType.Flour:
                typestring = "Flour Truck";
                break;
            case StopType.Water:
                typestring = "Water Truck";
                break;
            case StopType.Farm:
                typestring = "Farm Truck";
                break;
            case StopType.IronBar:
                typestring = "Iron Truck";
                break;
            case StopType.GoldBar:
                typestring = "Gold Truck";
                break;
            case StopType.Mint:
                typestring = "Mint Truck";
                break;
            case StopType.Bakery:
                typestring = "Bakery Truck";
                break;
            default:
                typestring = "Bus";
                break;
        }

        if (vehicle.Model is BusAdvancedModel )
        {
            itemtext.text = "Advanced "+ typestring;
        }
        else
        {
           // Debug.Log(vehicle.Model);
            itemtext.text = typestring;
        }
    }

    private void Sold(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void invopenedcheck(object sender, EventArgs e)
    {
        //Debug.Log(vehicleObject);
        if (vehicleObject == null)
        {
            if (isplaced)
            {
                isplaced = false;
                routebuton.SetActive(false);
                placebuttontext.text = "Place";
            }
                       
            
        }
    }

    public void sell()
    {
        inventory.AddMoney(vehicle.Cost);
        if (vehicle is BusData bus)
        {
            inventory.buszok.Remove(bus);
        }
        else
        {
            inventory.trucks.Remove((TruckData)vehicle);
        }
        if (isplaced)
        {
            
            Destroy(vehicleObject.gameObject);
            isplaced = false;
        }
        vehicle = null;
        Destroy(gameObject);
        vehicleObject.Sold();

    }

   public void place()
    {
        if (!isplaced)
        {
            
            buildingSystem.CreatePreview(vehicle);
            buildingSystem.vehiclePlaced += vehicleIsplaced;
            buildingSystem.vehiclePlacecancelled += cancelled;
            routebuton.SetActive(true);
            uiFunctions.closeinvshop();
        }
        else
        {
            //TODO remove bus
           // Debug.Log("destroooy");
            Destroy(vehicleObject.gameObject);
            isplaced = false;
            routebuton.SetActive(false);
            vehicleObject = null;
            placebuttontext.text = "Place";
        }
      
    }

    public void RouteSel()
    {
        buildingSystem.SelectVehicleForPlanning(vehicleObject);
        uiFunctions.closeinvshop();
    }

    private void cancelled(object sender, EventArgs e)
    {
        buildingSystem.vehiclePlaced -= vehicleIsplaced;
        buildingSystem.vehiclePlacecancelled -= cancelled;
    }

    private void vehicleIsplaced(object sender, VehicleBase e)
    {
        vehicleObject = e;
        vehicleObject.sold += Sold;
        buildingSystem.vehiclePlaced -= vehicleIsplaced;
        buildingSystem.vehiclePlaced -= vehicleIsplaced;
        isplaced = true;
        placebuttontext.text = "Remove";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
