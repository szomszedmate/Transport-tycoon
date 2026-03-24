using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Rendering.CameraUI;

public abstract class RoadModel : MonoBehaviour
{
    [SerializeField]
    private Transform wrapper;
    public float Rotation => wrapper.transform.eulerAngles.y;
    private BuildingShapeUnit[] roadShapeUnits;
    public abstract Direction[] Inputs {  get; }
    public abstract Direction[] Outputs { get; }
    private void Awake()
    {
        roadShapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }
    public void Rotate(float rotationStep)
    {
        wrapper.Rotate(new Vector3(0, rotationStep, 0));

        int steps = Mathf.RoundToInt(rotationStep / 90f);

        for (int i = 0; i < Inputs.Length; i++) // rotates inputs
        {
            Inputs[i] = (Direction)(((int)Inputs[i] + steps) % 4);
        }

        for (int i = 0; i < Outputs.Length; i++) // rotates outputs
        {
            Outputs[i] = (Direction)(((int)Outputs[i] + steps) % 4);
        }
    }

    public List<Vector3> GetAllBuildingPositions()
    {
        return roadShapeUnits.Select(unit => unit.transform.position).ToList();
    }

}
