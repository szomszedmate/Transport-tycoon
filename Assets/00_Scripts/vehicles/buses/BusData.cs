using UnityEngine;

[CreateAssetMenu(menuName = "Data/Vehicles/Buses/Bus")]
public class BusData : ScriptableObject, IData
{
    [field: SerializeField]
    public BusModel Model { get; private set; }

    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public StopType Type { get;  set; }
    [field: SerializeField]
    public float Speed { get; set; }
}
