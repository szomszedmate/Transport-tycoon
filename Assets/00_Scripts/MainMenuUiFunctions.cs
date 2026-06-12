using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuUiFunctions : MonoBehaviour
{    
    public void StartGame()
    {
        SpaceLoadingManager.LoadScene("AsteroidScene");
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadScene("CreditScene");
    }
}
