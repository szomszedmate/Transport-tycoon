using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameUiFunctions : MonoBehaviour
{
    public TMP_Text timeText;
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

    [Header("UI References")]
    [SerializeField] private List<ButtonDataPair> uiButtons;

    [Serializable]
    public struct ButtonDataPair
    {
        public Image ButtonImage;
        public ScriptableObject Data; // data for the pressed button
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingSystem.prevdest += DeselectAllButtons;
        buildingSystem.selectprev += SelectButton;
        buildingSystem.destroymodeturn += DestroyButtonSelect;
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
        if (dataAsset is IData data)
        {
            Debug.Log(data);

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
