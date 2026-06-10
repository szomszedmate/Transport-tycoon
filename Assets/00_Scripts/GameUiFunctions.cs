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
    public TMP_Text mainMoneyText;
    public TMP_Text shopMoneyText;
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
    public bool isPauseMenuActive = false;

    [Header("ScrollView")]
    public Color selectedColor;
    public Color defaultcolor;
    public BuildingSystem buildingSystem;
    public Image destroymodebutton;

    [SerializeField] public Game game;

    [Header("UI References")]
    [SerializeField] private GameObject minimap;
    [SerializeField] private List<ButtonDataPair> uiButtons;

    [Header("Settings")]
    private bool isSettingsMenuActive = false;
    public GameObject settingsPanel;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private MusicControl musicControl;
    [SerializeField] private Toggle sfxMuteToggle;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private SFXControl sfxControl;

    [Header("Iventory/Shop")]
    public GameObject invshopmasterpanel;
    public GameObject hud;
    public List<BusData> busdatas;
    public List<TruckData> truckdatas;
    public Player inventory;
    public GameObject inventoryitem;
    public Transform UIcontent;
    public Image invbutton;
    public GameObject invpanel;
    public GameObject infomenupanel;

    [Header("Vehicle Inventory Blocks")]
    [SerializeField] private GameObject vehicleInventoryBlockPrefab;
    [SerializeField] private Transform vehicleInventoryContent;
    [SerializeField] public List<InventoryResource> inventoryResources;
    
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

    void Awake()
    {
        inventory = GameObject.FindWithTag("Player").GetComponent<Player>();
        lastMoney = game.Player.Money;


        game.TimeChanged += Game_TimeChanged;
        game.Player.MoneyChanged += HandleMoneyPop;
        game.Player.TaxChanged += Player_TaxChanged;
        game.Player.InventoryChanged += Player_InventoryChanged;
        game.InputManager.moneyDebugEvent += InputManager_moneyDebugEvent;

        buildingSystem.prevdest += DeselectAllButtons;
        buildingSystem.selectprev += SelectButton;
        buildingSystem.destroymodeturn += DestroyButtonSelect;
    }

    void Start()
    {
        foreach (ButtonDataPair button in uiButtons)
        {
            if (button.Data is not IData)
            {
                Debug.LogWarning("Data must be IData!");
            }
            button.Price.text = "$" + ((IData)button.Data).Cost.ToString();
        }

        SetupUi();
    }

    private void SetupUi()
    {
        hud.SetActive(true);
        minimap.SetActive(true);
        invshopmasterpanel.SetActive(true);

        closeinvshop();
        settingsPanel.SetActive(false);
        PauseMenu.SetActive(false);

    }

    #region inv/shop
    public event EventHandler invopened ;
    public void openinvshop()
    {
        game.InputManager.menuOpen = true;
        hud.SetActive(false);
        CanvasGroup cg = invshopmasterpanel.GetComponent<CanvasGroup>();

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        minimap.SetActive(false);

        string moneyStr = Math.Round(game.Player.Money, 1).ToString();
        shopMoneyText.text = moneyStr;
        invopened?.Invoke(this,EventArgs.Empty);
    }

    public void closeinvshop()
    {
        game.InputManager.menuOpen = false;
        hud.SetActive(true);
        minimap.SetActive(true);
        CanvasGroup cg = invshopmasterpanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        //invshopmasterpanel.SetActive(false);
        GameObject objects=null;
        GameObject content = infomenupanel.transform.Find("content").gameObject;
        if (content.transform.childCount>0)
        {
            objects  = content.GetComponentInChildren<UIitem>().gameObject;
        }
        if (objects != null)
        {
            Destroy(objects);
        }
       
        infomenupanel.SetActive(false);
        
    }

    public void vehicleclicked()
    {
        vehiclebutton.color = selectedColor;
        invbutton.color = defaultcolor;
        shopbutton.color = defaultcolor;
        invpanel.SetActive(false);
        shoppanel.SetActive(false);
       
        CanvasGroup cg = vehiclepanel.GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
    public void invclicked()
    {
        vehiclebutton.color = defaultcolor;
        invbutton.color = selectedColor;
        shopbutton.color = defaultcolor;
        invpanel.SetActive(true);
        shoppanel.SetActive(false);
        CanvasGroup cg = vehiclepanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
    public void shopclicked()
    {
        vehiclebutton.color = defaultcolor;
        invbutton.color = defaultcolor;
        shopbutton.color = selectedColor;
        invpanel.SetActive(false);
        shoppanel.SetActive(true);
        CanvasGroup cg = vehiclepanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public ResourceEnum selectedResource = ResourceEnum.Iron;

    public void SellResource(TMP_InputField input)
    {
        int amount = Convert.ToInt32(input.text);
        game.Player.SellResource(selectedResource, amount);
    }
    public void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        foreach (InventoryResource resource in inventoryResources)
        {
            if (resource.Type == e.Resource)
            {
                resource.Amount += e.NewAmount;
            }
        }
    }
    #endregion

    private void Game_TimeChanged(object sender, TimeChangedEventArgs e)
    {
        TimeSpan t = TimeSpan.FromSeconds(e.NewTime);

        dayTimeText.text = "Day: " + e.Day + " - " + t.ToString(@"hh\:mm");
    }

    private void Player_TaxChanged(object sender, TaxChangedEventArgs e)
    {
        taxText.text = "Tax to pay: $" + Math.Round(e.NewAmount, 1).ToString();
    }


    private void InputManager_moneyDebugEvent(object sender, EventArgs e)
    {
        game.Player.AddMoney(100);
    }

    private void HandleMoneyPop(object sender, MoneyChangedEventArgs e)
    {
        double difference = e.NewAmount - lastMoney;

        if (difference == 0) return; // do nothing if no changes
        TMP_Text sourceText = shoppanel.activeInHierarchy ? shopMoneyText : mainMoneyText;
        GameObject popup = Instantiate(moneyPopup, transform);
        RectTransform popupRt = popup.GetComponent<RectTransform>();
        popupRt.position = popupSpawnPosition != null ? popupSpawnPosition.position : sourceText.GetComponent<RectTransform>().position;

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
            string moneyStr = Math.Round(e.NewAmount, 1).ToString();
            mainMoneyText.text = moneyStr;
            shopMoneyText.text = moneyStr;
                
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
        string moneyStr = Math.Round(targetAmount, 1).ToString();
        mainMoneyText.text = moneyStr;
        shopMoneyText.text = moneyStr;  
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
                break; // Megtal�ltuk, meg�llunk
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
            timeText.text = Time.timeScale + "x";
        }

    }

    public void FastForwardTime()
    {
        if (Time.timeScale < maxTimeSpeed)
        {
            Time.timeScale += 1;
            timeText.text = Time.timeScale + "x";
        }
        else
        {
            Time.timeScale = 1;
            timeText.text = Time.timeScale + "x";
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

    public void BuyVehicle(VehicleData vehicleData)
    {
        if (inventory.Money >= vehicleData.Cost)
        {
            // ScriptableObject p�ld�nyos�t�sa, hogy egyedi adata legyen
            VehicleData newVehicle = Instantiate(vehicleData);

            // Hozz�ad�s az inventory-hoz (t�pus szerint)
            if (newVehicle is BusData bus)
            {
                inventory.buszok.Add(bus);
            }
            else if (newVehicle is TruckData truck)
            {
                inventory.trucks.Add(truck); // Felt�telezve, hogy van ilyen list�d a Player-ben
            }

            // UIitem létrehozása (logika: place/sell)
            GameObject itemGo = Instantiate(inventoryitem, UIcontent, false);
            UIitem uiItem = itemGo.GetComponent<UIitem>();
            uiItem.vehicle = newVehicle;

            // Inventory block létrehozása a vehicle panelen (ikon megjelenítéshez)

            if (vehicleInventoryBlockPrefab != null && vehicleInventoryContent != null)
            {
                GameObject blockGo = Instantiate(vehicleInventoryBlockPrefab, vehicleInventoryContent, false);
                VehicleInventoryBlock block = blockGo.GetComponent<VehicleInventoryBlock>();
                if (block != null)
                    block.Init(newVehicle, uiItem);

            }
            else
            {
                Debug.LogWarning($"[BuyVehicle] Nincs prefab vagy content beállítva!");
            }

            // P�nz levon�sa a BuildingSystemen kereszt�l
            var buyRequest = new BuyRequestEventArgs { Cost = vehicleData.Cost, Deduct = true };
            buildingSystem.invokeBuying(buyRequest);
        }
        else
        {
            Debug.Log("Not enough money for: " + vehicleData.name);
        }
    }


    //public void buyBus1(BusData busdata)
    //{
    //    if (inventory.Money>=busdata.Cost)
    //    {
    //        BusData svb = Instantiate(busdata);
    //        //svb.Type = StopType.Bus;
    //        inventory.buszok.Add(svb);
    //        GameObject sv = Instantiate(inventoryitem, UIcontent, false);
    //        UIitem svitem = sv.GetComponent<UIitem>();
    //        svitem.vehicle = svb;
    //        float price = busdata.Cost;    // checking costs
    //        var checkNext = new BuyRequestEventArgs { Cost = price, Deduct = true };
    //        buildingSystem.invokeBuying(checkNext);
    //    }
    //    else
    //    {
    //        Debug.Log("Not enough money!");
    //    }
       
        
    //}
    
    #endregion

    public void Resume()
    {
        if (isSettingsMenuActive)
        {
            isSettingsMenuActive = false;
            settingsPanel.SetActive(false);
            isPauseMenuActive = true;
            PauseMenu.SetActive(true);
        }
        else if (isPauseMenuActive)
        {
            isPauseMenuActive = false;
            PauseTime();
            PauseMenu.SetActive(false);
            minimap.SetActive(true);
            hud.SetActive(true);
        }
    }
    public void Save()
    {
        //TODO
        Debug.Log("Save clicked");
    }

    public void Settings()
    {
        PauseMenu.SetActive(false);
        isSettingsMenuActive = true;
        settingsPanel.SetActive(true);
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(game.Music.volume * 17f);
        if (sfxSlider != null && game.SoundEffects.Count > 0)
            sfxSlider.SetValueWithoutNotify(game.SoundEffects[0].volume);
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

                hud.SetActive(false);
                minimap.SetActive(false);    

                PauseTime();
                PauseMenu.SetActive(true);

            }
            else
            {
                Resume();
            }
    }

    public void ChangeMusicVolume(float volume)
    {
        game.ChangeMusicVolume(volume);
        if (muteToggle != null)
            muteToggle.SetIsOnWithoutNotify(volume == 0);
        musicControl?.UpdateIcon(volume);
    }

    public void ChangeSFXVolume(float volume)
    {
        game.ChangeSFXVolume(volume);
        if (sfxMuteToggle != null)
            sfxMuteToggle.SetIsOnWithoutNotify(volume == 0);
        sfxControl?.UpdateIcon(volume);
    }

    public void MuteSFX()
    {
        game.MuteSFX();
        float vol = game.SoundEffects.Count > 0 ? game.SoundEffects[0].volume : 0f;
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(vol);
        sfxControl?.UpdateIcon(vol);
    }

    public void MuteMusic()
    {
        game.MuteMusic();
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(game.Music.volume * 17f);
        musicControl?.UpdateIcon(game.Music.volume);
    }

    private bool uiTextVisible = true;
    public void ToggleUIText()
    {
        uiTextVisible = !uiTextVisible;
        foreach (var tmp in FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            tmp.enabled = uiTextVisible;
    }

    [SerializeField] private int hideTextDropdownIndex = 0;
    [SerializeField] private TMPro.TMP_Dropdown languageDropdown;
    public void OnLanguageDropdown(int value)
    {
        bool showText = value != hideTextDropdownIndex;
        PlayerPrefs.SetInt("hideText", showText ? 0 : 1);
        PlayerPrefs.SetInt("languageIndex", value);
        foreach (var tmp in FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            tmp.enabled = showText;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
