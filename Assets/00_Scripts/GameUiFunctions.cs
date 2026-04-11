using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
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
    public Image[] buttonimages;
    public Image destroymodebutton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingSystem.prevdest += DeselectAllButtons;
        buildingSystem.selectprev += SelectButton;
        buildingSystem.destroymodeturn += DestroyButtonSelect;
    }

    private void DeselectAllButtons(object sender, EventArgs e)
    {
        foreach (var i in buttonimages)
        {
            i.color = defaultcolor;
        }
    }
    private void SelectButton(object sender, int e)
    {
        switch (e)
        {
            case 1:
                buttonimages[0].color = selectedColor;
                break;
            case 2:
                buttonimages[1].color = selectedColor;
                break;
            case 3:
                buttonimages[3].color = selectedColor;
                break;
            case 4:
                buttonimages[2].color = selectedColor;
                break;
            case 5:
                buttonimages[4].color = selectedColor;
                break;
            default:
                break;
        }
    }

    private void DestroyButtonSelect(object s, EventArgs e)
    {
        if (buildingSystem.destroy == false)
        {
            destroymodebutton.color = defaultcolor;
        }
        else
        {
            destroymodebutton.color = selectedColor;
        }
    }
    public void TurnOnDestroyMode()
    {
        buildingSystem.DestroyMode();
        if (buildingSystem.destroy==false)
        {
            destroymodebutton.color = defaultcolor;
        }
        else
        {
            destroymodebutton.color = selectedColor;
        }
    }

    public void RoadSelect(int type)
    {
        if (buildingSystem.destroy==true)
        {
            TurnOnDestroyMode();

        }
        DeselectAllButtons(this, EventArgs.Empty);
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        Image img = obj.GetComponent<Image>();
        img.color = selectedColor;
        buildingSystem.DestroyPreview();
        buildingSystem.CreatePreview(type);
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
