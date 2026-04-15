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
    [SerializeField] private GameObject moneyPopup;
    [SerializeField] private Transform popupSpawnPosition;
    private float lastMoney;

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

    [Serializable]
    public struct ButtonDataPair
    {
        public Image ButtonImage;
        public ScriptableObject Data; // data for the pressed button
    }

    void Start()
    {
        lastMoney = game.Player.Money;
        game.Player.MoneyChanged += HandleMoneyPop;
        game.InputManager.moneyDebugEvent += InputManager_moneyDebugEvent;
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
        float difference = e.NewAmount - lastMoney;

        if (difference == 0) return; // do nothing if no changes
        
        GameObject popup = Instantiate(moneyPopup, moneyText.transform.position, Quaternion.identity, transform);

        var txt = popup.GetComponent<TMPro.TextMeshProUGUI>();
        Animator anim = popup.GetComponent<Animator>();
        if (difference > 0)
        {
            txt.text = "+$" + difference;
            txt.color = Color.green;
            anim.Play("GainMoneyAnimation");
            StartCoroutine(UpdateMoneyDelayed(e.NewAmount, 0.85f));

            Destroy(popup, 2f);
        }
        else
        {
            moneyText.text = "Money: $" + e.NewAmount.ToString();
            txt.text = "-$" + Math.Abs(difference);
            txt.color = Color.red;
            anim.Play("LoseMoneyAnimation");
            Destroy(popup, 1f);
        }
        lastMoney = e.NewAmount;
    }

    private System.Collections.IEnumerator UpdateMoneyDelayed(float targetAmount, float delay)
    {
        yield return new WaitForSeconds(delay);
        moneyText.text = "Money: $" + targetAmount.ToString();
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
            if (pair.Data as IData == data)
            {
                pair.ButtonImage.color = selectedColor;
                break;
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
