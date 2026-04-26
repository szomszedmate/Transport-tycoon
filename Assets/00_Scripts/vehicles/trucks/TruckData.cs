using UnityEngine;

[CreateAssetMenu(menuName = "Data/Vehicles/Trucks/Truck")]
public class TruckData : VehicleData
{
    [SerializeField] private ResourceEnum resource;

    public ResourceEnum Resource { get => resource; private set => resource = value; }
}
