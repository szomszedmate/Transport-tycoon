using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
    }
}
