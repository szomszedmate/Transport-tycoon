using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityModel : MonoBehaviour
{
    private BuildingShapeUnit[] shapeUnits;
    void Awake()
    {
        shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }
    
    public List<Vector3> GetAllBuildingPositions()
    {
        if (shapeUnits == null || shapeUnits.Length == 0)
            shapeUnits = GetComponentsInChildren<BuildingShapeUnit>(true); // true = inaktivakat is

        if (shapeUnits == null)
        {
            return new List<Vector3>();
        }

        if (shapeUnits.Length == 0)
        {
            return new List<Vector3>();
        }

        return shapeUnits.Select(u => u.transform.position).ToList();
    }
}
