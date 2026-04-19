using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameUiFunctions : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Text moneyText;
    public TMP_Text taxText;
    public TMP_Text dayTimeText;
    [SerializeField] private GameObject moneyPopup;
    [SerializeField] private Transform popupSpawnPosition;
    private double lastMoney;

    public bool ispaused;
   
    public float maxTimeSpeed;
    public float minTimeSpeed;
    public Sprite pauseSprite;
    public Sprite StartSprite;
    public Button pauseButton;
  
    public GameObject PauseMenu;
    public bool isPauseMenuActive;

    [Header("ScrollView")]
    public Color selectedColor;
    public Color defaultcolor;
    public BuildingSystem buildingSystem;
    public Image destroymodebutton;

    [SerializeField] public Game game;

    [Header("UI References")]
    [SerializeField] private List<ButtonDataPair> uiButtons;


    [Header("Iventory/Shop")]
    public GameObject invshopmasterpanel;
    public GameObject hud;
    public List<BusData> busdatas;
    public Player inventory;
    public GameObject inventoryitem;
    public Transform UIcontent;
    public Image invbutton;
    public GameObject invpanel;
    
    public Image shopbutton;
    public GameObject shoppanel;
    public Image vehiclebutton;
    public GameObject vehiclepanel;

    [Serializable]
    public struct ButtonDataPair
    {
        public Image ButtonImage;
        public ScriptableObject Data; // data for the pressed button
        public TextMeshProUGUI Price;
    }

    void Start()
    {
        inventory = GameObject.FindWithTag("Player").GetComponent<Player>();
        lastMoney = game.Player.Money;
        game.TimeChanged += Game_TimeChanged;
        game.Player.MoneyChanged += HandleMoneyPop;
        game.Player.TaxChanged += Player_TaxChanged;
        game.InputManager.moneyDebugEvent += InputManager_moneyDebugEvent;

        foreach (ButtonDataPair button in uiButtons)
        {
            if (button.Data is not IData)
            {
                Debug.LogWarning("Data must be IData!");
            }
            button.Price.text = "$" + ((IData)button.Data).Cost.ToString();
        }
    }

    #region inv/shop

    public void openinvshop()
    {
        hud.SetActive(false);
        invshopmasterpanel.SetActive(true);
        PauseTime();
    }

    public void closeinvshop()
    {
        hud.SetActive(true);
        PauseTime();
        invshopmasterpanel.SetActive(false);
        
    }

    public void vehicleclicked()
    {
        vehiclebutton.color = selectedColor;
        invbutton.color = defaultcolor;
        shopbutton.color = defaultcolor;
        invpanel.SetActive(false);
        shoppanel.SetActive(false);
        vehiclepanel.SetActive(true);
    }
    public void invclicked()
    {
        vehiclebutton.color = defaultcolor;
        invbutton.color = selectedColor;
        shopbutton.color = defaultcolor;
        invpanel.SetActive(true);
        shoppanel.SetActive(false);
        vehiclepanel.SetActive(false);
    }
    public void shopclicked()
    {
        vehiclebutton.color = defaultcolor;
        invbutton.color = defaultcolor;
        shopbutton.color = selectedColor;
        invpanel.SetActive(false);
        shoppanel.SetActive(true);
        vehiclepanel.SetActive(false);
    }
    #endregion

    private void Game_TimeChanged(object sender, TimeChangedEventArgs e)
    {
        TimeSpan t = TimeSpan.FromSeconds(e.NewTime);

       // dayTimeText.text = "Day: " + e.Day + " - " + t.ToString(@"hh\:mm");
    }

    private void Player_TaxChanged(object sender, TaxChangedEventArgs e)
    {
        taxText.text = "Tax to pay: $" + Math.Round(e.NewAmount, 1).ToString();
    }

    void Awake()
    {
        buildingSystem.prevdest += DeselectAllButtons;
        buildingSystem.selectprev += SelectButton;
        buildingSystem.destroymodeturn += DestroyButtonSelect;
    }

    private void InputManager_moneyDebugEvent(object sender, EventArgs e)
    {
        game.Player.AddMoney(100);
    }

    private void HandleMoneyPop(object sender, MoneyChangedEventArgs e)
    {
        double difference = e.NewAmount - lastMoney;

        if (difference == 0) return; // do nothing if no changes
        
        GameObject popup = Instantiate(moneyPopup, moneyText.transform.position, Quaternion.identity, transform);

        var txt = popup.GetComponent<TMPro.TextMeshProUGUI>();
        Animator anim = popup.GetComponent<Animator>();
        if (difference > 0)
        {
            txt.text = "+$" + Math.Round(difference, 1);
            txt.color = Color.green;
            anim.Play("GainMoneyAnimation");
            StartCoroutine(UpdateMoneyDelayed(e.NewAmount, 0.85f));

            Destroy(popup, 2f);
        }
        else
        {
            moneyText.text = "Money: $" + Math.Round(e.NewAmount,1).ToString();
            txt.text = "-$" + Math.Round(Math.Abs(difference));
            txt.color = Color.red;
            anim.Play("LoseMoneyAnimation");
            Destroy(popup, 1f);
        }
        lastMoney = e.NewAmount;
    }

    private System.Collections.IEnumerator UpdateMoneyDelayed(double targetAmount, float delay)
    {
        yield return new WaitForSeconds(delay);
        moneyText.text = "Money: $" + Math.Round(targetAmount,1).ToString();
    }

    private void DeselectAllButtons(object sender, EventArgs e)
    {
        if (uiButtons == null) return;
        foreach (var pair in uiButtons)
        {
            pair.ButtonImage.color = defaultcolor;
        }
        destroymodebutton.color = defaultcolor;
    }
    private void SelectButton(object sender, IData data)
    {
        DeselectAllButtons(this, EventArgs.Empty);

        foreach (var pair in uiButtons)
        {
            if (pair.Data != null && (pair.Data as IData) == data)
            {
                pair.ButtonImage.color = selectedColor;
                break; // Megtaláltuk, megállunk
            }
        }
    }

    private void DestroyButtonSelect(object s, EventArgs e)
    {
        if (buildingSystem.destroy)
        {
            DeselectAllButtons(this, EventArgs.Empty);
            destroymodebutton.color = selectedColor;
        }
        else
        {
            destroymodebutton.color = defaultcolor;
        }
    }
    public void TurnOnDestroyMode()
    {
        buildingSystem.DestroyMode();
    }

    public void UIButton_SelectBuilding(ScriptableObject dataAsset)
    {
        if (buildingSystem.destroy)
        {
            buildingSystem.DestroyMode();
        }
        if (dataAsset == null)
        {
            return;
        }
        if (dataAsset is IData data)
        {
            buildingSystem.CreatePreview(data);
        }
    }

    public void PauseTime()
    {
        if (!ispaused)
        {
            Time.timeScale = 0;
            ispaused = true;
            pauseButton.GetComponent<Image>().sprite = StartSprite;
            timeText.text = "Paused";
        }
        else if(!isPauseMenuActive)
        {
            ispaused = false;
            Time.timeScale = 1;
            pauseButton.GetComponent<Image>().sprite = pauseSprite;
            timeText.text = "Time: " + Time.timeScale + "x";
        }
        
    }

    public void FastForwardTime()
    {
        if (Time.timeScale < maxTimeSpeed)
        {
            Time.timeScale += 1;
            timeText.text = "Time: " + Time.timeScale + "x";
        }
        else
        {
            Time.timeScale = 1;
            timeText.text = "Time: " + Time.timeScale + "x";
        }
    }
    /* public void SlowTime()
     {
         if (Time.timeScale>minTimeSpeed)
         {
             Time.timeScale -= 1;
             timeText.text = "Time: " + Time.timeScale+"x";
         }
     }*/



    #region buyvehicles
        
    public void buyBus1()
    {

        BusData svb = Instantiate(busdatas[0]);
        svb.Type = StopType.Bus;
        inventory.buszok.Add(svb);
        GameObject sv = Instantiate(inventoryitem, UIcontent, false); 
        UIitem svitem = sv.GetComponent<UIitem>();
        svitem.bus = svb;
        
    }
    public void buyBus2()
    {
        inventory.buszok.Add(busdatas[1]);
        GameObject sv = inventoryitem;
        UIitem svitem = sv.GetComponent<UIitem>();
        svitem.bus = inventory.buszok[inventory.buszok.Count - 1];
        Instantiate(sv, UIcontent, false);
    }

    public void buynormalwatertruck()
    {
        BusData svb = Instantiate(busdatas[0]);
        svb.Type = StopType.Water;
        inventory.buszok.Add(svb);
        GameObject sv = Instantiate(inventoryitem, UIcontent, false);
        UIitem svitem = sv.GetComponent<UIitem>();
        svitem.bus = svb;
    }
    #endregion

    public void Resume()
    {
        isPauseMenuActive = false;
        PauseTime();
        PauseMenu.SetActive(false); 
    }
    public void Save()
    {
        //TODO
        Debug.Log("Save clicked");
    }
    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenMenu()
    {
            if (!isPauseMenuActive)
            {
                isPauseMenuActive = true;
                PauseTime();
                PauseMenu.SetActive(true);

            }
            else
            {
                Resume();
            }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
