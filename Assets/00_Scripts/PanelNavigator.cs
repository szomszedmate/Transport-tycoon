using TMPro;
using UnityEngine;

public class PanelNavigator : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject garagePanel;

    [Header("Arrow Texts")]
    public TMP_Text nextArrowText;
    public TMP_Text prevArrowText;

    private enum Panel { Inventory, Shop, Garage }
    private Panel currentPanel = Panel.Shop;

    void Start()
    {
        UpdateArrows();
        SetupWindows();
    }

    private void SetupWindows()
    {
        inventoryPanel.SetActive(false);

        shopPanel.SetActive(true);
        currentPanel = Panel.Shop;

        garagePanel.SetActive(false);
    }

    public void ShowInventory()
    {
        currentPanel = Panel.Inventory;
        inventoryPanel.SetActive(true);
        shopPanel.SetActive(false);
        if (garagePanel != null) garagePanel.SetActive(false);
        UpdateArrows();
    }

    public void ShowShop()
    {
        currentPanel = Panel.Shop;
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(true);
        if (garagePanel != null) garagePanel.SetActive(false);
        UpdateArrows();
    }

    public void ShowGarage()
    {
        currentPanel = Panel.Garage;
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        if (garagePanel != null) garagePanel.SetActive(true);
        UpdateArrows();
    }

    public void Next()
    {
        switch (currentPanel)
        {
            case Panel.Inventory: ShowShop(); break;
            case Panel.Shop: ShowGarage(); break;
            case Panel.Garage: ShowInventory(); break;
        }
    }

    public void Prev()
    {
        switch (currentPanel)
        {
            case Panel.Inventory: ShowGarage(); break;
            case Panel.Shop: ShowInventory(); break;
            case Panel.Garage: ShowShop(); break;
        }
    }

    private void UpdateArrows()
    {
        switch (currentPanel)
        {
            case Panel.Inventory:
                if (nextArrowText != null) nextArrowText.text = "SHOP";
                if (prevArrowText != null) prevArrowText.text = "GARAGE";
                break;
            case Panel.Shop:
                if (nextArrowText != null) nextArrowText.text = "GARAGE";
                if (prevArrowText != null) prevArrowText.text = "INVENTORY";
                break;
            case Panel.Garage:
                if (nextArrowText != null) nextArrowText.text = "INVENTORY";
                if (prevArrowText != null) prevArrowText.text = "SHOP";
                break;
        }
    }
}
