using UnityEngine;

[CreateAssetMenu(menuName = "Data/Buildings/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField]
    public BuildingModel Model {  get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public string Description { get; private set; }
}
