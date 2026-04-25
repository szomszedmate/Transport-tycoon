using UnityEngine;

public abstract class VehicleData : ScriptableObject, IData
{
    [field: SerializeField] public VehicleModel Model { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public StopType Type { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float LoadingSpeed { get; private set; }
    [field: SerializeField] public int Capacity { get; private set; }
}
