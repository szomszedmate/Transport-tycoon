using System.Collections.Generic;
using UnityEngine;

public abstract class VehicleData : ScriptableObject, IData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] [field: TextArea] public string Description { get; private set; }
    [field: SerializeField] public VehicleModel Model { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public List<StopType> Types { get; private set; }
    [field: SerializeField] public StopType MainType { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float LoadingSpeed { get; private set; }
    [field: SerializeField] public int Capacity { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
}
