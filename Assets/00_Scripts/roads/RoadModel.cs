using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

public abstract class RoadModel : MonoBehaviour
{
    [SerializeField]
    private Transform wrapper;
    public float Rotation => transform.eulerAngles.y;
    private BuildingShapeUnit[] roadShapeUnits;
    public abstract Direction[] Inputs { get; set; }
    public abstract Direction[] Outputs { get; set; }
    public abstract RoadType RoadType { get; }
    public GameObject bridgeEffect;
    private void Awake()
    {
        roadShapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }
    public void Rotate(float rotationStep)
    {
        transform.Rotate(new Vector3(0, rotationStep, 0));

        int steps = Mathf.RoundToInt(rotationStep / 90f);

        for (int i = 0; i < Inputs.Length; i++) // rotates inputs
        {
            Inputs[i] = (Direction)(((int)Inputs[i] + (4 - steps)) % 4); // (4 - steps cuz it rotates the other way)
        }

        for (int i = 0; i < Outputs.Length; i++) // rotates outputs
        {
            Outputs[i] = (Direction)(((int)Outputs[i] + (4 - steps)) % 4); // (4 - steps cuz it rotates the other way);
        }
    }

    public List<Vector3> GetAllBuildingPositions()
    {
        return roadShapeUnits.Select(unit => unit.transform.position).ToList();
    }

}
