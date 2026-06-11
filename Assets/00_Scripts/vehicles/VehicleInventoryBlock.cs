using UnityEngine;
using UnityEngine.UI;

public class VehicleInventoryBlock : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionHighlight;

    private VehicleData vehicleData;
    [SerializeField] private VehicleInfoPanel infoPanel;
    public bool IsPlaced { get; private set; } = false;
    public VehicleBase PlacedVehicle { get; private set; }

    public void SetPlaced(bool placed, VehicleBase vehicle = null)
    {
        IsPlaced = placed;
        PlacedVehicle = vehicle;
    }

    public void Init(VehicleData data)
    {
        vehicleData = data;

        if (infoPanel == null)
            infoPanel = GameObject.FindObjectOfType<VehicleInfoPanel>(true);

        if (iconImage != null && data.Icon != null)
            iconImage.sprite = data.Icon;

        if (selectionHighlight != null)
            selectionHighlight.enabled = false;
    }

    public void OnClicked()
    {
        if (infoPanel != null && vehicleData != null)
            infoPanel.Show(vehicleData, this);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;
    }
}
