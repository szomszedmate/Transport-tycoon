using UnityEngine;

public class ShopVehicleSelector : MonoBehaviour
{
    [SerializeField] private ShopItemButton defaultButton;

    private ShopItemButton Active => ShopItemButton.Current ?? defaultButton;

    public void SelectBasic()
    {
        Active?.SelectBasic();
    }

    public void SelectAdvanced()
    {
        Active?.SelectAdvanced();
    }

    public void Buy()
    {
        Active?.Buy();
    }
}
