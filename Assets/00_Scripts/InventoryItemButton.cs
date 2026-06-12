using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    [Header("Item Data")]
    public Image icon;
    public string itemName;
    [TextArea] public string description;
    public ResourceEnum resourceType;

    [Header("Item Description References")]
    public Image descIcon;
    public TMP_Text descName;
    public TMP_Text descDescription;
    public TMP_Text descAmount;

    [Header("Inventory Reference")]
    public GameUiFunctions gameUI;

    private bool isSelected = false;

    void Start()
    {
        if (gameUI != null)
            gameUI.game.Player.InventoryChanged += OnInventoryChanged;
    }

    void OnDestroy()
    {
        if (gameUI != null)
            gameUI.game.Player.InventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.Resource == resourceType && isSelected)
            UpdateAmount();
    }

    public void OnClick()
    {
        isSelected = true;

        if (gameUI != null) gameUI.selectedResource = resourceType;
        if (descIcon != null && icon != null) descIcon.sprite = icon.sprite;
        if (descName != null) descName.text = itemName;
        if (descDescription != null) descDescription.text = description;

        UpdateAmount();
    }

    private void UpdateAmount()
    {
        if (descAmount == null || gameUI == null) return;

        int amount = 0;
        if (gameUI.game.Player.Resources != null && gameUI.game.Player.Resources.ContainsKey(resourceType))
            amount = gameUI.game.Player.Resources[resourceType];
        descAmount.text = "In stock: " + amount;
    }
}
