using TMPro;
using UnityEngine;

public class InventoryResource : MonoBehaviour
{
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private ResourceEnum type;
    private int amount;
    public int Amount
    {
        get
        {
            return amount;
        }
        set
        {
            amount = value;
            amountText.text = Type.ToString() + ": " + Amount;
        }

    }

    public ResourceEnum Type { get => type; private set => type = value; }
}
