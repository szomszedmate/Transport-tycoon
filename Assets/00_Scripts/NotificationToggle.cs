using UnityEngine;

public class NotificationToggle : MonoBehaviour
{
    [SerializeField] private GameObject hudScrollPanel;

    public void TurnOn()
    {
        if (hudScrollPanel != null)
            hudScrollPanel.SetActive(true);
    }

    public void TurnOff()
    {
        if (hudScrollPanel != null)
            hudScrollPanel.SetActive(false);
    }
}
