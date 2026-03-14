using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuUiFunctions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void StartGame()
    {
        
        SceneManager.LoadScene("SampleScene");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void OpenSettings()
    {
        //TODO
    }
    public void Load()
    {
        //TODO
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
