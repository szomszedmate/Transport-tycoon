using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Vehicles/MaterialSettings")]
public class VehicleMaterials : ScriptableObject
{
    [SerializeField] private List<Material> materials;

    // Így éred el a listát kódból:
    public List<Material> Materials => materials;
}