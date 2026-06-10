using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleInfoPanel : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private Image vehicleIcon;
    [SerializeField] private TMP_Text vehicleName;
    [SerializeField] private TMP_Text vehicleDescription;
    [SerializeField] private TMP_Text vehiclePrice;

    [Header("Buttons")]
    [SerializeField] private Button placeButton;
    [SerializeField] private Button planRouteButton;
    [SerializeField] private Button sellButton;

    private VehicleData currentData;
    private VehicleInventoryBlock currentBlock;
    private BuildingSystem buildingSystem;
    private GameUiFunctions uiFunctions;
    private Player player;

    void Awake()
    {
        buildingSystem = GameObject.FindWithTag("buildingsys").GetComponent<BuildingSystem>();
        uiFunctions = GameObject.FindWithTag("UI").GetComponent<GameUiFunctions>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        gameObject.SetActive(false);
    }

    public void Show(VehicleData data, VehicleInventoryBlock block)
    {
        currentData = data;
        currentBlock = block;
        gameObject.SetActive(true);

        if (vehicleIcon != null) vehicleIcon.sprite = data.Icon;
        if (vehicleName != null) vehicleName.text = data.Name;
        if (vehicleDescription != null) vehicleDescription.text = data.Description;
        if (vehiclePrice != null) vehiclePrice.text = "Price: $" + data.Cost;

        // Route gomb csak akkor aktív ha a jármű már le van rakva
        if (planRouteButton != null)
            planRouteButton.interactable = block.IsPlaced;
    }

    public void OnPlace()
    {
        if (currentData == null) return;

        // Ha már le van rakva, először eltávolítja
        if (currentBlock != null && currentBlock.IsPlaced && currentBlock.PlacedVehicle != null)
        {
            Destroy(currentBlock.PlacedVehicle.gameObject);
            currentBlock.SetPlaced(false);
        }

        buildingSystem.vehiclePlaced += OnVehiclePlaced;
        buildingSystem.CreatePreview(currentData);
        uiFunctions.closeinvshop();
    }

    private void OnVehiclePlaced(object sender, VehicleBase vehicle)
    {
        buildingSystem.vehiclePlaced -= OnVehiclePlaced;
        currentBlock?.SetPlaced(true, vehicle);
    }

    public void OnPlanRoute()
    {
        if (currentBlock?.PlacedVehicle != null)
            buildingSystem.SelectVehicleForPlanning(currentBlock.PlacedVehicle);
        uiFunctions.closeinvshop();
    }

    public void OnSell()
    {
        if (currentData == null) return;

        player.AddMoney(currentData.Cost);

        if (currentData is BusData bus)
            player.buszok.Remove(bus);
        else if (currentData is TruckData truck)
            player.trucks.Remove(truck);

        if (currentBlock != null)
            Destroy(currentBlock.gameObject);

        gameObject.SetActive(false);
        currentData = null;
        currentBlock = null;
    }
}
