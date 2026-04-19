using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class UIitem : MonoBehaviour
{
    public Player inventory;
    public GameUiFunctions uiFunctions;
    public BusData bus;
    public BuildingSystem buildingSystem;
    public bool isplaced = false;
    public TMP_Text itemtext;
    public Button placebutton;
    public GameObject invmenu;
    public TMP_Text placebuttontext;
    public Bus busobject;
    public StopType type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory= GameObject.FindWithTag("Player").GetComponent<Player>();
        uiFunctions= GameObject.FindWithTag("UI").GetComponent<GameUiFunctions>();
        buildingSystem= GameObject.FindWithTag("buildingsys").GetComponent<BuildingSystem>();
        invmenu = GameObject.FindWithTag("invmenu");
        type = bus.Type;
        itemtext.text = type.ToString();
    }

    public void sell()
    {
        inventory.AddMoney(bus.Cost);
        inventory.buszok.Remove(bus);
        Destroy(gameObject);
       
    }

   public void place()
    {
        if (!isplaced)
        {
            buildingSystem.CreatePreview(bus);
            buildingSystem.busplaced += busisplaced;
            buildingSystem.busplacecancelled += cancelled;
            uiFunctions.closeinvshop();
        }
        else
        {
            //TODO remove bus
           // Debug.Log("destroooy");
            Destroy(busobject.gameObject);
            isplaced = false;
            busobject = null;
            placebuttontext.text = "Place";
        }
      
    }

    private void cancelled(object sender, EventArgs e)
    {
        buildingSystem.busplaced -= busisplaced;
        buildingSystem.busplacecancelled -= cancelled;
    }

    private void busisplaced(object sender, Bus e)
    {
        busobject = e;
        buildingSystem.busplaced -= busisplaced;
        buildingSystem.busplaced -= busisplaced;
        isplaced = true;
        placebuttontext.text = "Remove";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
