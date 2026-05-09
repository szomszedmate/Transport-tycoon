//using Mono.Cecil;
using TMPro;
using UnityEngine;

public class InventoryResource : MonoBehaviour
{
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private ResourceEnum type;
    [SerializeField]
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
            amountText.text = /*Type.ToString() + ": " +*/ Amount.ToString();
        }

    }

    public ResourceEnum Type { get => type; private set => type = value; }
}
