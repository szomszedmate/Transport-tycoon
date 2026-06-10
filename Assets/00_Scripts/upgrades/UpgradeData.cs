using UnityEngine;

[CreateAssetMenu(menuName = "Data/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
}
