using UnityEngine;

public class ShopVehicleSelector : MonoBehaviour
{
    public void SelectBasic()
    {
        if (ShopItemButton.Current != null)
            ShopItemButton.Current.SelectBasic();
    }

    public void SelectAdvanced()
    {
        if (ShopItemButton.Current != null)
            ShopItemButton.Current.SelectAdvanced();
    }

    public void Buy()
    {
        if (ShopItemButton.Current != null)
            ShopItemButton.Current.Buy();
    }
}
