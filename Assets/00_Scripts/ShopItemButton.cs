using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [Header("Item Options")]
    public ScriptableObject basicData;
    public ScriptableObject advancedData;

    [Header("Option Buttons (Basic / Advanced)")]
    public Sprite basicIcon;
    public Sprite advancedIcon;

    [Header("Item Description References")]
    public Image descBasicIcon;
    public Image descAdvancedIcon;
    public TMP_Text descName;
    public TMP_Text descDescription;
    public TMP_Text descPrice;

    [Header("References")]
    public GameUiFunctions gameUI;

    public static ShopItemButton Current;
    private ScriptableObject selectedData;

    public void OnClick()
    {
        Current = this;
        if (basicIcon != null && descBasicIcon != null) descBasicIcon.sprite = basicIcon;
        if (advancedIcon != null && descAdvancedIcon != null) descAdvancedIcon.sprite = advancedIcon;
        SelectBasic();
    }

    public void SelectBasic()
    {
        selectedData = basicData;
        UpdateInfo(basicData);
    }

    public void SelectAdvanced()
    {
        selectedData = advancedData;
        UpdateInfo(advancedData);
    }

    private void UpdateInfo(ScriptableObject data)
    {
        if (data is VehicleData v)
        {
            if (descName != null) descName.text = v.Name;
            if (descDescription != null) descDescription.text = v.Description;
            if (descPrice != null) descPrice.text = "$" + v.Cost;
        }
        else if (data is BusStopData b)
        {
            if (descName != null) descName.text = b.Name;
            if (descDescription != null) descDescription.text = b.Description;
            if (descPrice != null) descPrice.text = "$" + b.Cost;
        }
    }

    public void Buy()
    {
        if (selectedData == null) return;
        if (gameUI == null) return;
        if (selectedData is not IData idata) return;

        if (!gameUI.game.Player.CanAfford(idata.Cost))
        {
            Debug.Log("Nincs elég pénz!");
            return;
        }

        if (selectedData is VehicleData vehicleData)
        {
            gameUI.BuyVehicle(vehicleData);
        }
        else if (selectedData is BusStopData busStopData)
        {
            gameUI.game.Player.LoseMoney(busStopData.Cost);
            gameUI.buildingSystem.CreatePreview(busStopData);
            gameUI.closeinvshop();
        }

        Debug.Log("Megvásárolva: " + selectedData.name);
    }
}
