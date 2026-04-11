using UnityEngine;

[CreateAssetMenu(menuName = "Data/Vehicles/Buses/Bus")]
public class BusData : ScriptableObject
{
    [field: SerializeField]
    public BusModel Model { get; private set; }

    [field: SerializeField]
    public int Cost { get; private set; }
}
