using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BackToMenu : MonoBehaviour
{
    [SerializeField] private CreditsScroll credits;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            credits.Reset();
            SceneManager.LoadScene("MainMenu");
        }
    }

}
